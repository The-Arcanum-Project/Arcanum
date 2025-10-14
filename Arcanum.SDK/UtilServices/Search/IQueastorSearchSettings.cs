namespace Arcanum.API.UtilServices.Search;

public class IQueastorSearchSettings
{
   public enum SortingOptions
   {
      Relevance,
      Namespace,
      Alphabetical,
   }

   public enum SearchModes
   {
      //Contains,
      //StartsWith,
      //EndsWith,
      ExactMatch,
      Fuzzy,

      //Regex,
      Default, // Fuzzy, Exact
   }

   [Flags]
   public enum DefaultCategories
   {
      None = 0,
      Settings = 1 << 0, // 1
      UiElements = 1 << 1, // 2
      GameObjects = 1 << 2, // 4
      MapObjects = 1 << 3, // 8
      AbstractObjects = 1 << 4, // 16
      All = Settings | UiElements | GameObjects | MapObjects | AbstractObjects, // 31,
   }

   public SearchModes SearchMode { get; set; } = SearchModes.Default;
   public SortingOptions SortingOption { get; set; } = SortingOptions.Relevance;
   public Enum SearchCategory { get; set; } = DefaultCategories.All;
   public bool WholeWord { get; set; }
   public int MaxLevinsteinDistance { get; set; } = 2;
}