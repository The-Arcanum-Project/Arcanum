using System.Text.RegularExpressions;
using Arcanum.API.Settings;
using Arcanum.API.UtilServices.Search;

namespace Arcanum.Core.CoreSystems.Queastor;

public class SimpleCollectionViewFilterProvider : ICollectionViewFilterProvider
{
   public static Predicate<object> GenerateFilter(ISearchSettings settings,
                                                  string query,
                                                  string targetPropName = "")
   {
      if (settings == null)
         throw new ArgumentNullException(nameof(settings));

      if (string.IsNullOrEmpty(query))
         return _ => true;

      var queryParts = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

      switch (settings.SearchMode)
      {
         case ISearchSettings.SearchModes.ExactMatch:
            return input =>
            {
               var searchStringFromObject = GetSearchStringFromObject(input, targetPropName);

               return queryParts.All(part =>
                                        IsExactMatch(part, searchStringFromObject));
            };
         case ISearchSettings.SearchModes.Fuzzy:
            if (settings.MaxLevinsteinDistance < 0)
               throw new ArgumentOutOfRangeException(nameof(settings.MaxLevinsteinDistance),
                                                     "MaxLevinsteinDistance must be non-negative.");

            return input =>
            {
               var searchStringFromObject = GetSearchStringFromObject(input, targetPropName);
               var searchParts = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
               return searchParts.Any(part => IsExactMatch(part, searchStringFromObject) ||
                                              IsFuzzyMatch(part,
                                                           searchStringFromObject,
                                                           settings.MaxLevinsteinDistance));
            };
         case ISearchSettings.SearchModes.Regex:
            var searchRegex = new Regex(query, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            return input =>
            {
               var searchString = GetSearchStringFromObject(input, targetPropName);
               return searchRegex.IsMatch(searchString);
            };
         case ISearchSettings.SearchModes.Default:
            if (settings.MaxLevinsteinDistance < 0)
               throw new ArgumentOutOfRangeException(nameof(settings.MaxLevinsteinDistance),
                                                     "MaxLevinsteinDistance must be non-negative.");

            return input =>
            {
               var searchStringFromObject = GetSearchStringFromObject(input, targetPropName);
               return queryParts.Any(queryPart => IsExactMatch(queryPart, searchStringFromObject) ||
                                                  IsFuzzyMatch(queryPart,
                                                               searchStringFromObject,
                                                               settings.MaxLevinsteinDistance));
            };
         default:
            throw new ArgumentOutOfRangeException();
      }
   }

   private static bool IsFuzzyMatch(string searchString, string input, int maxLevinsteinDistance)
   {
      if (string.IsNullOrEmpty(searchString) || string.IsNullOrEmpty(input))
         return false;

      return Queastor.LevinsteinDistance(input, searchString) <= maxLevinsteinDistance;
   }

   private static bool IsExactMatch(string searchString, string input)
   {
      if (string.IsNullOrEmpty(searchString) || string.IsNullOrEmpty(input))
         return false;

      var queryParts = searchString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
      return queryParts.Any(part => input.Contains(part, StringComparison.OrdinalIgnoreCase));
   }

   private static string GetSearchStringFromObject(object? input, string targetPropPath)
   {
      if (string.IsNullOrEmpty(targetPropPath))
         return input?.ToString() ?? string.Empty;

      var current = input;
      foreach (var part in targetPropPath.Split('.'))
      {
         if (current == null)
            return string.Empty;

         var prop = current.GetType().GetProperty(part);
         if (prop == null)
            return string.Empty;

         current = prop.GetValue(current);
      }

      return current?.ToString() ?? string.Empty;
   }
}