#define TIME_QUEASTOR

using System.Diagnostics;
using System.Reflection;
using Arcanum.API.UtilServices.Search;
using Arcanum.API.UtilServices.Search.SearchableSetting;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;

namespace Arcanum.Core.CoreSystems.Queastor;

public class Queastor : IQueastor
{
   private readonly Dictionary<string, List<ISearchable>> _invertedIndex =
      new(40_000, StringComparer.OrdinalIgnoreCase);

   private readonly BkTree _bkTree = new();

   public bool IsInitializing { get; set; } = true;
   public readonly HashSet<string> BkTreeTerms = new(40_000, StringComparer.OrdinalIgnoreCase);

   public int SearchIndexSize { get; private set; }

   public Queastor(IQueastorSearchSettings queastorSearchSettings)
   {
      Settings = queastorSearchSettings ?? throw new ArgumentNullException(nameof(queastorSearchSettings));
   }

   public static readonly Queastor GlobalInstance = new(new QueastorSearchSettings());

   public bool UsesDefaultEnum { get; set; } = true;
   public IQueastorSearchSettings Settings { get; set; }

   public void AddToIndex(ISearchable item)
   {
      foreach (var term in item.SearchTerms)
         InternalAddToIndex(item, term);
   }

   public void AddToIndex(ISearchable item, string term)
   {
      InternalAddToIndex(item, term);
   }

   public void RemoveFromIndex(ISearchable item, string term)
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

   private void InternalAddToIndex(ISearchable item, string term)
   {
      if (string.IsNullOrWhiteSpace(term))
         return;

      var lowerTerm = term.ToLowerInvariant();
      if (!_invertedIndex.TryGetValue(lowerTerm, out var list))
      {
         list = [];
         _invertedIndex[lowerTerm] = list;
      }

      list.Add(item);
      if (IsInitializing)
         BkTreeTerms.Add(lowerTerm.ToLowerInvariant());
      SearchIndexSize++;
   }

   public void RemoveFromIndex(ISearchable item)
   {
      foreach (var term in item.SearchTerms)
      {
         RemoveFromIndex(item, term);
      }
   }

   /// <summary>
   /// Build the BK-Tree from the collected terms. This should be called after all initial indexing is done.
   /// This is separated from the indexing process to optimize performance during bulk additions.
   /// </summary>
   public void RebuildBkTree()
   {
      _bkTree.Clear();
      _bkTree.BuildFrom(BkTreeTerms);
      IsInitializing = false;
      BkTreeTerms.Clear();
      BkTreeTerms.TrimExcess();
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

      var exact = SearchExact(query);

      // If the search mode is ExactMatch, we return only exact matches
      if (Settings.WholeWord)
         return exact;

      HashSet<SearchResult> filteredResults = [];
      // Fuzzy matches via BK-Tree
      foreach (var term in _bkTree.Search(query, Settings.MaxLevinsteinDistance))
      {
         if (UsesDefaultEnum)
         {
            if (_invertedIndex.TryGetValue(term, out var items))
            {
               if ((IQueastorSearchSettings.DefaultCategories)Settings.SearchCategory ==
                   IQueastorSearchSettings.DefaultCategories.All)
               {
                  foreach (var item in items)
                     filteredResults.Add(new(term, item));
                  continue;
               }

               // Filter by search category if specified
               foreach (var item in items)
                  if ((Convert.ToInt64(Settings.SearchCategory) & Convert.ToInt64(item.SearchCategory)) != 0)
                     filteredResults.Add(new(term, item));
            }
         }
      }

#if TIME_QUEASTOR
      Debug.WriteLine($"Queastor Search took: {sw.ElapsedMilliseconds} ms for query: {query} with {exact.Count} exact matches and {filteredResults
        .Count} fuzzy matches.");
#endif

      var sortedFuzzy = ApplySorting(filteredResults, query);
      return exact.Concat(sortedFuzzy).ToList();
   }

   private List<ISearchable> ApplySorting(HashSet<SearchResult> results, string query)
   {
      if (results.Count == 0 || string.IsNullOrWhiteSpace(query))
         return [];

      var itemsList = results.ToList();

      switch (Settings.SortingOption)
      {
         case IQueastorSearchSettings.SortingOptions.Relevance:
            itemsList.Sort((a, b) =>
            {
               var scoreA = a.Value.GetRelevanceScore(query, a.Key);
               var scoreB = b.Value.GetRelevanceScore(query, a.Key);
               var cmp = scoreA.CompareTo(scoreB);
               if (cmp != 0)
                  return cmp;

               // If scores are equal, sort by minimum Levinstein distance
               var distA = MinLevinsteinDistanceToTerms(a.Value, query);
               var distB = MinLevinsteinDistanceToTerms(b.Value, query);
               cmp = distA.CompareTo(distB); // Lower distances first
               if (cmp != 0)
                  return cmp;

               // If still equal, sort alphabetically
               return string.Compare(a.Value.ResultName, b.Value.ResultName, StringComparison.Ordinal);
            });
            break;
         case IQueastorSearchSettings.SortingOptions.Namespace:
            itemsList.Sort((a, b) =>
            {
               var nsA = a.Value.GetNamespace.Split(a.Value.NamespaceSeparator);
               var nsB = b.Value.GetNamespace.Split(b.Value.NamespaceSeparator);
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
            itemsList.Sort((a, b) => string.Compare(a.Value.ResultName, b.Value.ResultName, StringComparison.Ordinal));
            break;
         default:
            throw new ArgumentOutOfRangeException(typeof(IQueastorSearchSettings.SortingOptions).ToString());
      }

      return itemsList.Select(x => x.Value).ToList();
   }

   public List<ISearchable> SearchExact(string query)
   {
      query = query.ToLowerInvariant();
      if (_invertedIndex.TryGetValue(query, out var exact))
         return exact;

      return [];
   }

   public int MinLevinsteinDistanceToTerms(ISearchable item, string query)
   {
      var minDistance = int.MaxValue;
      foreach (var term in item.SearchTerms)
         if (LevinsteinDistance(term, query) < minDistance)
            minDistance = LevinsteinDistance(term, query);
      return minDistance;
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
      if (a.Length > b.Length)
         (a, b) = (b, a);

      var n = a.Length;
      var m = b.Length;

      // If one string is empty, the distance is the length of the other
      if (n == 0)
         return m;

      // Use a single array for the previous row of distances. This is the key optimization.
      var previousRow = new int[n + 1];

      for (var i = 0; i <= n; i++)
         previousRow[i] = i;

      for (var j = 1; j <= m; j++)
      {
         var previousDiagonal = previousRow[0];
         previousRow[0]++;

         for (var i = 1; i <= n; i++)
         {
            var oldDiagonal = previousRow[i];
            var cost = a[i - 1] == b[j - 1] ? 0 : 1;

            previousRow[i] = Math.Min(Math.Min(previousRow[i] + 1, // Deletion
                                               previousRow[i - 1] + 1), // Insertion
                                      previousDiagonal + cost); // Substitution
            previousDiagonal = oldDiagonal;
         }
      }

      return previousRow[n];
   }

   public Dictionary<IQueastorSearchSettings.DefaultCategories, int> GetEntriesPerCategory()
   {
      var categoryCounts = new Dictionary<IQueastorSearchSettings.DefaultCategories, int>();
      foreach (var entry in _invertedIndex)
      {
         var category = (IQueastorSearchSettings.DefaultCategories)entry.Value.FirstOrDefault()?.SearchCategory!;
         var count = categoryCounts.GetValueOrDefault(category, 0);

         categoryCounts[category] = count + entry.Value.Count;
      }

      return categoryCounts;
   }

   public void IndexSettings()
   {
      var settings = FindAllSearchableSettings(Config.Settings);
      foreach (var setting in settings)
      {
         var serachables = setting.GetAllSearchableObjects();
         foreach (var searchable in serachables)
            AddToIndex(searchable);
      }
   }

   /// <summary>
   /// Returns all <see cref="SearchableSettings"/> found in the given object tree. <br/>
   /// Currently collections of <see cref="SearchableSettings"/> are not supported, only single instances.
   /// They can be enabled by uncommenting the collection support code in the method below but is currently not required.
   /// </summary>
   /// <param name="root"></param>
   /// <returns></returns>
   private static List<SearchableSettings> FindAllSearchableSettings(object root)
   {
      var results = new List<SearchableSettings>();
      var stack = new Stack<object>();
      stack.Push(root);

      while (stack.Count > 0)
      {
         var current = stack.Pop();
         if (current == null!)
            continue;

         var type = current.GetType();
         foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
         {
            if (!prop.CanRead)
               continue;

            object? value;
            try
            {
               value = prop.GetValue(current);
            }
            catch
            {
               continue; // skip problematic getters
            }

            if (value is SearchableSettings ss)
               results.Add(ss);

            // For now there is no need for collection support

            // if (value is System.Collections.IEnumerable enumerable && value is not string)
            // {
            //    foreach (var item in enumerable)
            //       if (item != null)
            //          stack.Push(item);
            // }
            // else
            // {
            //    stack.Push(value);
            // }
         }
      }

      return results;
   }

   public static void AddIEu5ObjectsToQueastor(IQueastor queastor, IReadOnlyList<Type> eu5ObjectTypes)
   {
      foreach (var type in eu5ObjectTypes)
      {
         var empty = (IEu5Object)EmptyRegistry.Empties[type];
         foreach (var obj in empty.GetGlobalItemsNonGeneric().Values)
            queastor.AddToIndex((IEu5Object)obj);
      }
   }
}