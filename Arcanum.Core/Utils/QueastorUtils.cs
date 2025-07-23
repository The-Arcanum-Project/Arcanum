namespace Arcanum.Core.Utils;

public static class QueastorUtils
{
   public static List<string> ExtractSearchTerms(string str)
   {
      return str.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x)).ToList();
   }
}