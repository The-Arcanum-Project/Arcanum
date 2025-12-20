using System.ComponentModel;
using System.Reflection;
using Arcanum.API.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

/// <summary>
/// Automatic Generated Saving settings.
/// </summary>
public class AgsSettings
{
   /// <summary>
   /// If true, the saving order will follow the order specified in SaveOrder. <br/>
   /// If false, the default alphabetical order will be used.
   /// </summary>
   [Description("If true, the saving order will follow the order specified in SaveOrder. If false, the default alphabetical order will be used.")]
   [DefaultValue(false)]
   public bool CustomSaveOrder { get; set; } = false;

   /// <summary>
   /// Whether properties with default values should be skipped during saving. <br/>
   /// Default is true. 
   /// </summary>
   [Description("Whether properties with default values should be skipped during saving.")]
   [DefaultValue(true)]
   public bool SkipDefaultValues { get; set; } = true;

   /// <summary>
   /// If true, a comment will be added before the object
   /// </summary>
   [Description("If true, a comment will be added before the object")]
   [DefaultValue(true)]
   public bool HasSavingComment { get; set; } = true;

   /// <summary>
   /// If true, empty collections will still write their header (e.g., "MyList = { \n\n }")
   /// </summary>
   [Description("If true, empty collections will still write their header (e.g., \"MyList = { \\n\\n }\")")]
   [DefaultValue(true)]
   public bool WriteEmptyCollectionHeader { get; set; } = true;

   /// <summary>
   /// The order in which properties will be saved if CustomSaveOrder is true. <br/>
   /// The list should contain Enum values corresponding to the properties to be saved. 
   /// </summary>
   [Description("The order in which properties will be saved if CustomSaveOrder is true. The list should contain Enum values corresponding to the properties to be saved.")]
   [CustomResetMethod(nameof(ResetSaveOrder))]
   public List<Enum> SaveOrder { get; set; } = [];

   /// <summary>
   /// If true, first properties and then collections will appear in alphabetical order. <br/>
   /// If false, everything will be sorted alphabetically together. <br/>
   /// Default is true.
   /// </summary>
   [Description("If true, first properties and then collections will appear in alphabetical order. If false, everything will be sorted alphabetically together.")]
   [DefaultValue(true)]
   public bool SortCollectionsAndPropertiesSeparately { get; set; } = true;

   /// <summary>
   /// The format to use when saving. <br/>
   /// Default is <see cref="SavingFormat.Default"/>
   /// </summary>
   [Description("The format to use when saving.")]
   [DefaultValue(SavingFormat.Default)]
   public SavingFormat Format { get; set; } = SavingFormat.Default;

   [Description("A custom default name to use when saving objects of this type. If empty, the object's ToString() method will be used.")]
   [DefaultValue("")]
   public string CustomDefaultName { get; set; } = string.Empty;

   public string GetDefaultOrCustomFileName(IEu5Object obj)
   {
      return string.IsNullOrWhiteSpace(CustomDefaultName) ? obj.GetType().Name : CustomDefaultName;
   }

   #region Reset Methods

   public object ResetSaveOrder(PropertyInfo propInfo)
   {
      // propInfo is to a List<Enum>
      var listType = propInfo.PropertyType;
      if (!listType.IsGenericType || listType.GetGenericTypeDefinition() != typeof(List<>))
         return SaveOrder;

      var enumType = listType.GetGenericArguments()[0];
      var enumValues = (List<Enum>)Activator.CreateInstance(typeof(List<>).MakeGenericType(enumType))!;
      return enumValues;
   }

   #endregion
}