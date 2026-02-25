using Arcanum.Core.Utils;

namespace Arcanum.UI.AppFeatures;

public interface IAppFeature
{
   /// <summary>
   /// The unique identifier for this feature.
   /// </summary>
   FeatureId Id { get; }
   string DisplayName { get; }
   string Description { get; }

   /// <summary>
   /// The general area of the app this feature belongs to. Used for grouping in documentation and visualizations.
   /// </summary>
   FeatureCategory Category { get; }

   /// <summary>
   /// How big/important is this feature?
   /// </summary>
   FeatureLevel Level { get; }

   /// <summary>
   /// The parent feature that this is a part of, if any. E.g., "Search Bar" might be a child of "Entity Selector Panel".
   /// </summary>
   FeatureId? ParentFeatureId { get; }

   /// <summary>
   /// Where on the screen is this feature located? This is a general guideline for UI placement, not a strict rule.
   /// </summary>
   FeatureLocation Location { get; }

   /// <summary>
   /// How much screen space does this feature typically occupy? Used in visualizations.
   /// </summary>
   FeatureScale Scale { get; }

   /// <summary>
   /// CommandScopes used within this feature.
   /// </summary>
   IEnumerable<string> AssociatedScopes { get; }

   /// <summary>
   /// Synonyms or related terms that users might search for when looking for this feature. E.g., "Auto-sync" might have synonyms like "Auto-save", "Sync toggle", etc.
   /// </summary>
   IEnumerable<string> SearchSynonyms { get; }

   /// <summary>
   /// Key details or "quick points" about this feature that can be shown in tooltips, documentation,
   /// or quick reference guides. These should be concise and informative, providing users with a clear
   /// understanding of what the feature does and how to use it effectively.
   /// </summary>
   IEnumerable<FeatureNote> QuickPoints { get; }

   /// <summary>
   /// Links to relevant documentation, wiki pages, community discussions, or videos that provide more information about this feature.
   /// </summary>
   IEnumerable<ExternalReference>? Links { get; }

   /// <summary>
   /// The version of the application in which this feature was first introduced.
   /// </summary>
   VersionNumber IntroducedIn { get; }

   /// <summary>
   /// The current status of this feature, such as whether it's stable, experimental, in beta, or legacy.
   /// </summary>
   FeatureStatus Status { get; }

   /// <summary>
   /// An optional path to an icon representing this feature. 
   /// </summary>
   string? IconPath { get; }
}

public record ExternalReference(string Label, string Url, ReferenceType Type);
public record FeatureNote(string Label, string Description);

public enum ReferenceType
{
   Wiki,
   Manual,
   Community,
   Video,
}

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
   Nano,     // Occupies a tiny corner
   Compact,  // Small sidebar component
   Standard, // Normal panel size
   Major,    // Takes up 50%+ of screen
   Full,     // Full-screen / Modal
}
// @formatter:on