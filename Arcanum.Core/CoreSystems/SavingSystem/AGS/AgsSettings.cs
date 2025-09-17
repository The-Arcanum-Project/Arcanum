using System.ComponentModel;
using System.Reflection;
using Arcanum.API.Attributes;
using Arcanum.Core.GameObjects;
using Arcanum.Core.GameObjects.BaseTypes;
using Nexus.Core;

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
   /// The format to use when saving. <br/>
   /// Default is <see cref="SavingFormat.Default"/>
   /// </summary>
   [Description("The format to use when saving.")]
   [DefaultValue(SavingFormat.Default)]
   public SavingFormat Format { get; set; } = SavingFormat.Default;

   #region Reset Methods

   public object ResetSaveOrder(PropertyInfo propInfo)
   {
      var emptyInstance = TryGetStaticEmptyObject(propInfo);
      if (emptyInstance == null)
         throw new
            InvalidOperationException($"Cannot reset SaveOrder for {propInfo.DeclaringType?.FullName}.{propInfo.Name} because the static Empty property could not be found.");

      if (emptyInstance is not INexus nx)
         throw new
            InvalidOperationException($"Cannot reset SaveOrder for {propInfo.DeclaringType?.FullName}.{propInfo.Name} because the static Empty property is not an INexus.");

      return nx.GetAllProperties();
   }

   private static object? TryGetStaticEmptyObject(PropertyInfo? propertyInfo)
   {
      if (propertyInfo == null)
         return null;

      var declaringType = propertyInfo.DeclaringType;
      if (declaringType == null)
         return null;

      var implementsIEmpty = declaringType.GetInterfaces()
                                          .Any(i =>
                                                  i.IsGenericType &&
                                                  i.GetGenericTypeDefinition() == typeof(IEmpty<>));

      if (!implementsIEmpty)
         return null;

      var emptyProperty = declaringType.GetProperty("Empty",
                                                    BindingFlags.Public |
                                                    BindingFlags.Static |
                                                    BindingFlags.FlattenHierarchy);

      if (emptyProperty == null)
         return null;

      var emptyObject = emptyProperty.GetValue(null);
      return emptyObject;
   }

   #endregion
}