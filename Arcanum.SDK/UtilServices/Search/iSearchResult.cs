namespace Arcanum.API.UtilServices.Search;

/// <summary>
/// The visual representation of a search result.
/// </summary>
public interface ISearchResult
{
   /// <summary>
   /// An optional path to an icon representing the search result.
   /// </summary>
   string? IconPath { get; set; }
   /// <summary>
   /// The main text displayed for the search result, typically the name or title of the item.
   /// </summary>
   string MainText { get; set; }
   /// <summary>
   /// A secondary text displayed for the search result by default the namespace,
   /// but often providing additional context or information.
   /// </summary>
   string Description { get; set; }
}