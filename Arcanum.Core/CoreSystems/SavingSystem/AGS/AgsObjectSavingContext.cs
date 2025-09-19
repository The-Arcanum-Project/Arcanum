using Arcanum.Core.CoreSystems.Common;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

/// <summary>
/// A context object used during the saving process to hold relevant information.
/// </summary>
public class AgsObjectSavingContext
{
   public string CommentChar { get; init; }
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
   public List<PropertySavingMetadata> OrderedProperties { get; init; }

   public AgsObjectSavingContext(IAgs ags, string commentChar = "#")
   {
      Ags = ags;
      Settings = ags.AgsSettings;
      OrderedProperties = SortSaveableProperties(ags);
      CommentChar = commentChar;
   }

   private List<PropertySavingMetadata> SortSaveableProperties(IAgs ags)
   {
      if (Settings.CustomSaveOrder)
         return SortBySettings(ags.SaveableProps, Settings.SaveOrder);

      if (Settings.SortCollectionsAndPropertiesSeparately)
      {
         List<PropertySavingMetadata> collections = [];
         List<PropertySavingMetadata> properties = [];

         foreach (var property in ags.SaveableProps)
            if (property.IsCollection)
               collections.Add(property);
            else
               properties.Add(property);

         collections = collections.OrderBy(p => p.Keyword).ToList();
         properties = properties.OrderBy(p => p.Keyword).ToList();

         properties.AddRange(collections);
         return properties;
      }

      return ags.SaveableProps.OrderBy(p => p.Keyword).ToList();
   }

   private static List<PropertySavingMetadata> SortBySettings(IReadOnlyList<PropertySavingMetadata> propertiesToSave,
                                                              List<Enum> saveOrder)
   {
      var sortedList = new List<PropertySavingMetadata>();
      foreach (var enumValue in saveOrder)
      {
         var prop = propertiesToSave.FirstOrDefault(p => p.ValueType.Equals(enumValue));
         if (prop != null)
            sortedList.Add(prop);
      }

      return sortedList;
   }

   /// <summary>
   /// Uses the context to generate the AGS formatted string for the object and appends it to the provided StringBuilder.
   /// </summary>
   /// <param name="sb"></param>
   public void BuildContext(IndentedStringBuilder sb)
   {
      if (Settings.HasSavingComment && Ags.ClassMetadata.CommentProvider != null)
         Ags.ClassMetadata.CommentProvider(Ags, CommentChar, sb);

      if (Ags.ClassMetadata.SavingMethod != null)
      {
         Ags.ClassMetadata.SavingMethod.Invoke(Ags, [..OrderedProperties], sb);
         return;
      }

      var isInCollections = false;

      using (sb.BlockWithName(Ags, Settings.Format))
         for (var i = 0; i < OrderedProperties.Count; i++)
         {
            var prop = OrderedProperties[i];
            if (prop.IsCollection && !isInCollections)
            {
               sb.AppendLine();
               isInCollections = true;
            }

            if (Settings.Format == SavingFormat.Spacious && i > 0)
               sb.AppendLine();
            prop.Format(Ags, sb, CommentChar, Settings);
         }
   }
}