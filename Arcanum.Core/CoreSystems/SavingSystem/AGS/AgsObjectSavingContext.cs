using Arcanum.Core.CoreSystems.Common;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

/// <summary>
/// A context object used during the saving process to hold relevant information.
/// </summary>
public class AgsObjectSavingContext
{
   /// <summary>
   /// The AGS instance being saved.
   /// </summary>
   public IAgs Ags { get; init; }
   /// <summary>
   /// The settings used for the object being saved.
   /// </summary>
   public AgsSettings Settings { get; }
   /// <summary>
   /// The string builder used to construct the saved output.
   /// </summary>
   public List<PropertySavingMetaData> OrderedProperties { get; init; }

   public AgsObjectSavingContext(IAgs ags)
   {
      Ags = ags;
      Settings = ags.Settings;
      OrderedProperties = Settings.CustomSaveOrder
                             ? SortBySettings(ags.SaveableProps, Settings.SaveOrder)
                             : ags.SaveableProps.OrderBy(p => p.Keyword).ToList();
   }

   private static List<PropertySavingMetaData> SortBySettings(IReadOnlyList<PropertySavingMetaData> propertiesToSave,
                                                              List<Enum> saveOrder)
   {
      var sortedList = new List<PropertySavingMetaData>();
      foreach (var enumValue in saveOrder)
      {
         var prop = propertiesToSave.FirstOrDefault(p => p.ValueType.Equals(enumValue));
         if (prop != null)
            sortedList.Add(prop);
      }

      return sortedList;
   }

   public void BuildContext(IndentedStringBuilder sb)
   {
      foreach (var prop in OrderedProperties)
      {
      }
   }
}