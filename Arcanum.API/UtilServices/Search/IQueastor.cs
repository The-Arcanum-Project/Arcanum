namespace Arcanum.API.UtilServices.Search;

/// <summary>
/// The Search engine for the Arcanum project.
/// </summary>
public interface IQueastor
{
   /// <summary>
   /// Settings for the search engine. <br/>
   /// These are editable by the user in the serach interface and will always load the last used settings.
   /// </summary>
   public IQueastorSearchSettings Settings { get; set; }
   /// <summary>
   /// This method must be called to add an item to the search index. 
   /// </summary>
   /// <param name="item"></param>
   public void AddToIndex(ISearchable item);
   /// <summary>
   /// Remove the item from the search index.
   /// </summary>
   /// <param name="item"></param>
   public void RemoveFromIndex(ISearchable item);
   /// <summary>
   /// Modifies the search terms for an item already in the index.
   /// </summary>
   /// <param name="item"></param>
   /// <param name="oldTerms"></param>
   public void ModifyInIndex(ISearchable item, IReadOnlyList<string> oldTerms);

   /// <summary>
   /// Queries a search for the given term following the current settings. <br/>
   /// Internally a search for exact matches is performed first (O(n)) and then a fuzzy search (O(log n)) is performed on the results of the first search
   /// </summary>
   /// <param name="query"></param>
   /// <returns></returns>
   public List<ISearchable> Search(string query);
   /// <summary>
   /// Performs a search for exact matches of the given query.
   /// </summary>
   /// <param name="query"></param>
   /// <returns></returns>
   public List<ISearchable> SearchExact(string query);
   
   /// <summary>
   /// Returns the minimum Levenshtein distance to the terms of the item in the index.
   /// </summary>
   /// <param name="item"></param>
   /// <param name="query"></param>
   /// <returns></returns>
   public int MinLevinsteinDistanceToTerms(ISearchable item, string query);

   /// <summary>
   /// Adds all <see cref="SearchableSetting"/> settings to the index.
   /// </summary>
   public void IndexSettings();
}