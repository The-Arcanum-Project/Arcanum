using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;

/// <summary>
/// Optimized lookup if a word is contained in a predefined set of words.
/// Where each word starts with a capital letter.
/// </summary>
public class UppercaseWordChecker
{
   public UppercaseWordChecker(IEnumerable<string> words)
   {
      foreach (var word in words)
         _targetWords.Add(word);

      _targetWords.TrimExcess();
   }

   private readonly HashSet<string> _targetWords = new(StringComparer.Ordinal);

   public bool IsMatch(string input, out InjRepType type)
   {
      if (string.IsNullOrEmpty(input) || input[0] < 'A' || input[0] > 'Z')
      {
         type = InjRepType.None;
         return false;
      }

      if (_targetWords.Contains(input))
      {
         type = Enum.Parse<InjRepType>(input);
         return true;
      }

      type = InjRepType.None;
      return false;
   }
}