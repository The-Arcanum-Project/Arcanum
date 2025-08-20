using Arcanum.API.UtilServices.Search.SearchableSetting;

namespace Arcanum.API.UtilServices.Search;

/// <summary>
/// A generic interface for objects that can be searched and displayed in a search results view. <br/>
/// For any items to become searchable, they must implement this interface and also add themselves to the
/// <see cref="IQueastor"/> preferably in the constructor of the derivative class. <br/>
/// </summary>
public interface ISearchable
{
   /// <summary>
   /// A namespace pointing to the location of the search result. <br/>
   /// This can either be UI navigation help (menu>submenu>item) or a file path (e.g. "Arcanum.Nexus.Core.GameObjects.Economy.Market").
   /// </summary>
   public string GetNamespace { get; }
   /// <summary>
   /// The name which will be displayed in the search results view
   /// </summary>
   public string ResultName { get; } // => GetNamespace.Length > 0 ? GetNamespace[^1] : string.Empty
   /// <summary>
   /// A list of all terms that can be used to search for this result.
   /// Usually this consists of the name, namespace, and any other relevant keywords.
   /// </summary>
   public List<string> SearchTerms { get; set; }

   /// <summary>
   /// What happens when the search result is selected. <br/>
   /// Either highlights the result, open a window, or navigates to a specific location.
   /// </summary>
   public void OnSearchSelected();

   /// <summary>
   /// Returns a relevance score for the search result based on the query. <br/>
   /// By default this is 1f which makes the <see cref="IQueastor"/> sort them after the Levinstein distance.
   /// </summary>
   /// <param name="query"></param>
   /// <returns></returns>
   public float GetRelevanceScore(string query) => 1f;

   /// <summary>
   /// The visual representation of the search result in the search results view.
   /// </summary>
   public ISearchResult VisualRepresentation { get; }
   /// <summary>
   /// The Category of the search result. <br/>
   /// This is used to group and or filter search results in the search results view.
   /// </summary>
   public IQueastorSearchSettings.Category SearchCategory { get; }
   /// <summary>
   /// The separator used to denote namespaces in the search results. 
   /// </summary>
   [IgnoreSettingProperty]
   public char NamespaceSeparator => '>';
}