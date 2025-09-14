using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public class SavingMetaData
{
   public required string Keyword { get; init; }
   public TokenType Separator { get; init; } = TokenType.Equals;
   public required Enum NxProp { get; init; }
   public SavingValueType ValueType { get; init; }
   public required Func<IAgs, Enum, string>? CommentProvider { get; set; }
   public required Action<IAgs, SavingMetaData, IndentedStringBuilder>? SavingMethod { get; set; }
}

public class SavingContext
{
   public IAgs Ags { get; init; }
   public AgsSettings Settings { get; }
   public IndentedStringBuilder Sb { get; init; }
   public List<SavingMetaData> OrderedProperties { get; init; }

   public SavingContext(IAgs ags, IndentedStringBuilder sb, List<SavingMetaData> propertiesToSave)
   {
      Ags = ags;
      Settings = ags.Settings;
      Sb = sb;
      OrderedProperties = Settings.CustomSaveOrder
                             ? SortBySettings(propertiesToSave, Settings.SaveOrder)
                             : propertiesToSave.OrderBy(p => p.Keyword).ToList();
   }

   private static List<SavingMetaData> SortBySettings(List<SavingMetaData> propertiesToSave, List<Enum> saveOrder)
   {
      var sortedList = new List<SavingMetaData>();
      foreach (var enumValue in saveOrder)
      {
         var prop = propertiesToSave.FirstOrDefault(p => p.ValueType.Equals(enumValue));
         if (prop != null)
            sortedList.Add(prop);
      }

      return sortedList;
   }
}