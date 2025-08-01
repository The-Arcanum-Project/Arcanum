
namespace Arcanum.API.UtilServices.Search;

public interface ISearchSettings
{
   public enum SortingOptions
   {
      Acending,
      Descending,
   }

   public enum SearchModes
   {
      //Contains,
      //StartsWith,
      //EndsWith,
      ExactMatch,
      Fuzzy,
      Regex,
      Default, // Fuzzy, Exact
   }

   public SearchModes SearchMode { get; set; }
   public SortingOptions SortingOption { get; set; }
   public int MaxLevinsteinDistance { get; set; }
}