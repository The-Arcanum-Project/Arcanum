namespace Arcanum.API.UtilServices.Search;


public interface ISearchResult
{
   string? IconPath { get; set; }
   string MainText { get; set; } 
   string Description { get; set; } 
}