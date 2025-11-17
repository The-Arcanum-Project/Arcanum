using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;

/// <summary>
/// Optimized lookup if a word is contained in a predefined set of words.
/// Where each word starts with a capital letter.
/// </summary>
public static class UppercaseWordChecker
{
   public static bool IsMatch(string input, out InjRepType type, out string key)
   {
      if (string.IsNullOrEmpty(input) || input[0] < 'A' || input[0] > 'Z')
      {
         type = InjRepType.None;
         key = input;
         return false;
      }

      var parts = input.Split(':', StringSplitOptions.RemoveEmptyEntries);
      if (parts.Length != 2)
      {
         type = InjRepType.None;
         key = input;
         return false;
      }

      if (Enum.TryParse<InjRepType>(parts[0], out var parsedType))
      {
         type = parsedType;
         key = parts[1];
         return true;
      }

      type = InjRepType.None;
      key = input;
      return false;
   }
}