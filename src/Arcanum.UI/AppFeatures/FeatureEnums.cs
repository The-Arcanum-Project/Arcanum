namespace Arcanum.UI.AppFeatures;

public enum FeatureStatus
{
   Stable,
   Experimental,
   Beta,
   Legacy,
}

public enum FeatureLocation
{
   Center,
   Top,
   TopRight,
   Right,
   BottomRight,
   Bottom,
   BottomLeft,
   Left,
   TopLeft,
   Floating, // For popups/dialogs
   Contextual, // Appears only on demand
}

public enum FeatureCategory
{
   Editor,
   Debug,
   Configuration,
   EditorMap,
   SpecializedEditor,
}

// @formatter:off
public enum FeatureLevel
{
   System, // The App itself, Settings, DataEditor, MapEditor...
   Module, // Major areas: Political Editor, Pops Editor
   Panel,  // Sub-sections: Entity Selector, Inspector
   Widget, // Smaller tools: Search Bar, Status Indicator
   Action, // Sentence-level detail: "Auto-sync toggle"
}

public enum FeatureScale
{
   Compact,  // Small sidebar component
   Standard, // Normal panel size
   Major,    // Takes up 50%+ of screen
   Full,     // Full-screen / Modal
}
// @formatter:on