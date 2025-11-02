using System.Text.RegularExpressions;

namespace Arcanum.API.UtilServices.Search.SearchableSetting;

public static partial class SearchHelper
{
   /// <summary>
   /// We also want to add each part of the camelCase name as a search term and
   /// a substring of the full name getting longer with each camelCase part <br/>
   /// example: ThisIsAnExample -> this, is, an, example, thisIs, thisIsAn, thisIsAnExample
   /// </summary>
   /// <returns></returns>
   public static List<string> GenerateSearchTerms(string desc)
   {
      var parts = CamelCaseSplitter().Split(desc);

      List<string> terms = [..parts.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.ToLower())];

      // Build progressive substrings
      for (var i = 2; i <= parts.Length; i++) // start at 2 to avoid first match being empty
         terms.Add(string.Join("", parts[..i]));

      return terms;
   }

   [GeneratedRegex("(?=[A-Z])")]
   private static partial Regex CamelCaseSplitter();
}