

using Arcanum.API.UtilServices.Search;

namespace Arcanum.UI.Components.StyleClasses;

public class SearchResultItem : ISearchResult
{
   public SearchResultItem(string? iconPath, string mainText, string description)
   {
      IconPath = iconPath;
      MainText = mainText;
      Description = description;
   }
   public string? IconPath { get; set; } // e.g., "/Assets/csharp_icon.png"
   public string MainText { get; set; }
   public string Description { get; set; }
}
