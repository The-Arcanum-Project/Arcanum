namespace Common;

public static class QueastorUtils
{
   /// <summary>
   /// Creates a list of search terms from a string by splitting it on spaces and removing empty entries.
   /// </summary>
   /// <param name="str"></param>
   /// <returns></returns>
   public static List<string> ExtractSearchTerms(string str)
   {
      return str.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
   }
}