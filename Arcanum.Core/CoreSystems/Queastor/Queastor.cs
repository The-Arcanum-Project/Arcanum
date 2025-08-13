#define TIME_QUEASTOR

using System.Diagnostics;
using Arcanum.API.UtilServices.Search;

namespace Arcanum.Core.CoreSystems.Queastor;

public class Queastor : IQueastor
{
   private readonly Dictionary<string, List<ISearchable>> _invertedIndex = new(StringComparer.OrdinalIgnoreCase);
   private readonly BkTree _bkTree = new();

   public int SearchIndexSize { get; private set; } = 0;

   public Queastor(IQueastorSearchSettings queastorSearchSettings)
   {
      Settings = queastorSearchSettings ?? throw new ArgumentNullException(nameof(queastorSearchSettings));
   }

   public static readonly Queastor GlobalInstance = new(new QueastorSearchSettings());

   public IQueastorSearchSettings Settings { get; set; }

   public void AddToIndex(ISearchable item)
   {
      foreach (var term in item.SearchTerms)
         InternalAddToIndex(item, term);
   }

   private void InternalAddToIndex(ISearchable item, string term)
   {
      var lowerTerm = term.ToLowerInvariant();
      if (!_invertedIndex.TryGetValue(lowerTerm, out var list))
      {
         list = [];
         _invertedIndex[lowerTerm] = list;
      }

      list.Add(item);
      _bkTree.Add(lowerTerm);
      SearchIndexSize++;
   }

   public void RemoveFromIndex(ISearchable item)
   {
      foreach (var term in item.SearchTerms)
      {
         var lowerTerm = term.ToLowerInvariant();
         if (_invertedIndex.TryGetValue(lowerTerm, out var list))
         {
            list.Remove(item);
            SearchIndexSize--;
            if (list.Count == 0)
               _invertedIndex.Remove(lowerTerm);
         }
      }
   }

   public void ModifyInIndex(ISearchable item, IReadOnlyList<string> oldTerms)
   {
      var newTerms = item.SearchTerms.Select(x => x.ToLowerInvariant()).ToList();
      foreach (var term in oldTerms)
      {
         var lowerTerm = term.ToLowerInvariant();
         
         if (newTerms.Remove(lowerTerm))
            continue;

         if (_invertedIndex.TryGetValue(lowerTerm, out var list))
         {
            list.Remove(item);
            if (list.Count == 0)
               _invertedIndex.Remove(lowerTerm);
         }
      }

      foreach (var term in newTerms)
         InternalAddToIndex(item, term);
   }

   public List<ISearchable> Search(string query)
   {
#if TIME_QUEASTOR
      var sw = Stopwatch.StartNew();
#endif
      query = query.ToLowerInvariant();
      var results = new HashSet<ISearchable>();

      results.UnionWith(SearchExact(query));

      // If the search mode is ExactMatch, we return only exact matches
      if (Settings.WholeWord)
         return results.ToList();

      HashSet<ISearchable> filteredResults = [];
      // Fuzzy matches via BK-Tree
      foreach (var term in _bkTree.Search(query, Settings.MaxLevinsteinDistance))
         if (_invertedIndex.TryGetValue(term, out var items))
         {
            if (Settings.SearchCategory == IQueastorSearchSettings.Category.All)
            {
               filteredResults.UnionWith(items);
               continue;
            }

            // Filter by search category if specified
            foreach (var item in items.Where(item => (item.SearchCategory & Settings.SearchCategory) != 0))
               filteredResults.Add(item);
         }
#if TIME_QUEASTOR
      Debug.WriteLine($"Queastor Search took: {sw.ElapsedMilliseconds} ms for query: {query} with {results.Count} exact matches and {filteredResults.Count} fuzzy matches.");
#endif

      return ApplySorting(filteredResults, query);
   }

   private List<ISearchable> ApplySorting(HashSet<ISearchable> results, string query)
   {
      if (results.Count == 0 || string.IsNullOrWhiteSpace(query))
         return [];

      var itemsList = results.ToList();

      switch (Settings.SortingOption)
      {
         case IQueastorSearchSettings.SortingOptions.Relevance:
            itemsList.Sort((a, b) =>
                              -b.GetRelevanceScore(query).CompareTo(a.GetRelevanceScore(query)));
            break;
         case IQueastorSearchSettings.SortingOptions.Namespace:
            itemsList.Sort((a, b) =>
            {
               var nsA = a.GetNamespace.Split(a.NamespaceSeparator);
               var nsB = b.GetNamespace.Split(b.NamespaceSeparator);
               var len = Math.Min(nsA.Length, nsB.Length);
               for (var i = 0; i < len; i++)
               {
                  var cmp = string.Compare(nsA[i], nsB[i], StringComparison.Ordinal);
                  if (cmp != 0)
                     return cmp;
               }

               return nsA.Length.CompareTo(nsB.Length);
            });
            break;
         case IQueastorSearchSettings.SortingOptions.Alphabetical:
            itemsList.Sort((a, b) => string.Compare(a.ResultName, b.ResultName, StringComparison.Ordinal));
            break;
         default:
            throw new ArgumentOutOfRangeException(typeof(IQueastorSearchSettings.SortingOptions).ToString());
      }

      return itemsList;
   }

   public List<ISearchable> SearchExact(string query)
   {
      query = query.ToLowerInvariant();
      if (_invertedIndex.TryGetValue(query, out var exact))
         return exact;

      return [];
   }
   //
   // public List<(string, ISearchable)> SortSearchResults(List<ISearchable> results,
   //                                                      string query,
   //                                                      bool sortAscending = false)
   // {
   //    if (results.Count == 0 || string.IsNullOrWhiteSpace(query))
   //       return [];
   //
   //    var sorted = sortAscending
   //                    ? results.OrderBy(x => x.GetRelevanceScore(query)).ToList()
   //                    : results.OrderByDescending(x => x.GetRelevanceScore(query)).ToList();
   //
   //    return sorted.Select(x => (GetClosestMatch(query, x.SearchTerms), x)).ToList();
   // }

   public string GetClosestMatch(string query, IList<string> terms)
   {
      if (terms.Count == 0)
         return string.Empty;

      var closest = terms[0];
      var minDistance = LevinsteinDistance(query, closest);

      foreach (var term in terms)
      {
         var distance = LevinsteinDistance(query, term);
         if (distance < minDistance)
         {
            minDistance = distance;
            closest = term;
         }
      }

      return closest;
   }

   public static int LevinsteinDistance(string a, string b)
   {
      var dp = new int[a.Length + 1, b.Length + 1];
      for (var i = 0; i <= a.Length; i++)
         dp[i, 0] = i;
      for (var j = 0; j <= b.Length; j++)
         dp[0, j] = j;

      for (var i = 1; i <= a.Length; i++)
         for (var j = 1; j <= b.Length; j++)
         {
            var cost = a[i - 1] == b[j - 1] ? 0 : 1;
            dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                                dp[i - 1, j - 1] + cost);
         }

      return dp[a.Length, b.Length];
   }

   public Dictionary<IQueastorSearchSettings.Category, int> GetEntriesPerCategory()
   {
      var categoryCounts = new Dictionary<IQueastorSearchSettings.Category, int>();
      foreach (var entry in _invertedIndex)
      {
         var category = (IQueastorSearchSettings.Category)entry.Value.FirstOrDefault()?.SearchCategory!;
         var count = categoryCounts.GetValueOrDefault(category, 0);

         categoryCounts[category] = count + entry.Value.Count;
      }
      
      return categoryCounts;
   }
}