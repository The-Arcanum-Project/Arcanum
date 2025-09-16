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
      OrderedProperties = Settings.CustomSaveOrder
                             ? SortBySettings(ags.SaveableProps, Settings.SaveOrder)
                             : ags.SaveableProps.OrderBy(p => p.Keyword).ToList();
      CommentChar = commentChar;
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

      using (sb.BlockWithName(Ags, Settings.Format))
         for (var i = 0; i < OrderedProperties.Count; i++)
         {
            if (Settings.Format == SavingFormat.Spacious && i > 0)
               sb.AppendLine();
            OrderedProperties[i].Format(Ags, sb, CommentChar, Settings);
         }
   }
}