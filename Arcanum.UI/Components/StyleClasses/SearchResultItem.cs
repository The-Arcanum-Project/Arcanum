

namespace Arcanum.UI.Components.StyleClasses;

public class SearchResultItem
{
   // We are replacing the string for text with a string for the image path
   // public string IconText { get; set; } 
   public string? IconPath { get; set; } // e.g., "/Assets/csharp_icon.png"

   public required string MainText { get; set; }
   public required string Description { get; set; }
}