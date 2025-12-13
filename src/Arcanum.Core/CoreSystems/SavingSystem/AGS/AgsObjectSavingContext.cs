using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

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
      OrderedProperties = PropertyOrderCache.GetOrCreateSortedProperties(ags);
      CommentChar = commentChar;
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
         Ags.ClassMetadata.SavingMethod.Invoke(Ags, [..OrderedProperties], sb, Ags.ClassMetadata.AsOneLine);
         return;
      }

      var asOneLine = Ags.ClassMetadata.AsOneLine;
      using (sb.BlockWithName(Ags, Settings.Format, asOneLine))
         for (var i = 0; i < OrderedProperties.Count; i++)
         {
            var prop = OrderedProperties[i];
            if (prop.NxProp.ToString().Contains("ate"))
            {
            }

            if (Settings.Format == SavingFormat.Spacious && i > 0)
               sb.AppendLine();
            prop.Format(Ags, sb, asOneLine, CommentChar, Settings);
         }
   }

   public void BuildContext(IndentedStringBuilder sb,
                            HashSet<PropertySavingMetadata> properties,
                            InjRepType strategy,
                            bool serializeDefaultValues = false)
   {
      if (Settings.HasSavingComment && Ags.ClassMetadata.CommentProvider != null)
         Ags.ClassMetadata.CommentProvider(Ags, CommentChar, sb);

      if (Ags.ClassMetadata.SavingMethod != null)
      {
         Ags.ClassMetadata.SavingMethod.Invoke(Ags, [..OrderedProperties], sb, Ags.ClassMetadata.AsOneLine);
         return;
      }

      var asOneLine = Ags.ClassMetadata.AsOneLine;
      using (sb.BlockWithNameAndInjection(Ags, strategy, asOneLine))
         for (var i = 0; i < OrderedProperties.Count; i++)
         {
            var prop = OrderedProperties[i];
#if DEBUG
            if (prop.NxProp.ToString().Contains("regency_date"))
            {
            }
#endif

            if (!properties.Contains(prop))
               continue;

            if (Settings.Format == SavingFormat.Spacious && i > 0)
               sb.AppendLine();
            prop.Format(Ags, sb, asOneLine, CommentChar, Settings, serializeDefaultValues);
         }
   }
}