namespace Arcanum.API.UtilServices.Search;

public interface ISearchable
{
   public string GetNamespace { get; }
   public string ResultName { get; } // => GetNamespace.Length > 0 ? GetNamespace[^1] : string.Empty
   public List<string> SearchTerms { get; set; }
   public void OnSearchSelected();
   public float GetRelevanceScore(string query) => 1f;
   public ISearchResult VisualRepresentation { get; }
   public ISearchSettings.Category SearchCategory { get; }
}
