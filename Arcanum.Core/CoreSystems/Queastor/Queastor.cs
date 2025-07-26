using Arcanum.API.UtilServices.Search;

namespace Arcanum.Core.CoreSystems.Queastor;

public class Queastor : IQueastor
{
   private readonly Dictionary<string, List<ISearchable>> _invertedIndex = new(StringComparer.OrdinalIgnoreCase);
   private readonly BkTree _bkTree = new();
   
   public static readonly Queastor GlobalInstance = new();
   
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
   }

   public void RemoveFromIndex(ISearchable item)
   {
      foreach (var term in item.SearchTerms)
      {
         var lowerTerm = term.ToLowerInvariant();
         if (_invertedIndex.TryGetValue(lowerTerm, out var list))
         {
            list.Remove(item);
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
         if (newTerms.Contains(lowerTerm))
         {
            newTerms.Remove(lowerTerm);
            continue;
         }

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

   public List<ISearchable> Search(string query, int maxDistance = 2)
   {
      query = query.ToLowerInvariant();
      var results = new HashSet<ISearchable>();

      results.UnionWith(SearchExact(query));

      // Fuzzy matches via BK-Tree
      foreach (var term in _bkTree.Search(query, maxDistance))
         if (_invertedIndex.TryGetValue(term, out var items))
            results.UnionWith(items);

      return results.ToList();
   }

   public List<ISearchable> SearchExact(string query)
   {
      query = query.ToLowerInvariant();
      if (_invertedIndex.TryGetValue(query, out var exact))
         return exact;

      return [];
   }

   public List<(string, ISearchable)> SortSearchResults(List<ISearchable> results, string query, bool sortAscending = false)
   {
      if (results.Count == 0 || string.IsNullOrWhiteSpace(query))
         return [];

      var sorted = sortAscending
                      ? results.OrderBy(x => x.GetRelevanceScore(query)).ToList()
                      : results.OrderByDescending(x => x.GetRelevanceScore(query)).ToList();

      return sorted.Select(x => (GetClosestMatch(query, x.SearchTerms), x)).ToList();
   }

   public string GetClosestMatch(string query, IList<string> terms)
   {
      if (terms.Count == 0)
         return string.Empty;

      var closest = terms[0];
      var minDistance = LevenshteinDistance(query, closest);

      foreach (var term in terms)
      {
         var distance = LevenshteinDistance(query, term);
         if (distance < minDistance)
         {
            minDistance = distance;
            closest = term;
         }
      }

      return closest;
   }

   public static int LevenshteinDistance(string a, string b)
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
}