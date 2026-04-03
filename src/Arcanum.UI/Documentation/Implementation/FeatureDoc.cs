#region

using System.Text;
using System.Windows;
using System.Windows.Media;
using Arcanum.Core.Utils;
using Arcanum.UI.AppFeatures;

#endregion

namespace Arcanum.UI.Documentation.Implementation;

public class FeatureDoc
{
   // ----- YAML Metadata ----- \\

   /// <summary>
   ///    The unique identifier of the documentation page, used for linking and referencing.
   /// </summary>
   public FeatureId Id { get; set; } = FeatureIds.Empty;
   /// <summary>
   ///    The title of the documentation page / feature
   /// </summary>
   public string Title { get; set; } = string.Empty;
   /// <summary>
   ///    A brief summary of the documentation page / feature, used for quick reference and overviews.
   /// </summary>
   public string Summary { get; set; } = string.Empty;
   /// <summary>
   ///    An array of related links or references to other documentation pages, providing additional context or information.
   /// </summary>
   public string[] Links { get; set; } = [];
   /// <summary>
   ///    An array of keywords that users might search for when looking for this documentation page.
   ///    These should be relevant terms, synonyms, or related concepts that help users find the page more easily.
   /// </summary>
   public string[] SearchKeywords { get; set; } = [];
   /// <summary>
   ///    CommandScopes used within this feature.
   /// </summary>
   public string[] AssociatedScopes { get; set; } = [];
   /// <summary>
   ///    An array of context sensitive sections
   /// </summary>
   public DocuSection[] Sections { get; set; } = [];
   /// <summary>
   ///    The general area of the app this feature belongs to.
   /// </summary>
   public FeatureCategory Category { get; set; }
   /// <summary>
   ///    How big/important is this feature?
   /// </summary>
   public FeatureLevel Level { get; set; }
   /// <summary>
   ///    How much screen space does this feature typically occupy? Used in visualizations.
   /// </summary>
   public FeatureScale Scale { get; set; }
   /// <summary>
   ///    Where on the screen is this feature located.
   ///    This is a general guideline for UI placement, not a strict rule.
   /// </summary>
   public FeatureLocation Location { get; set; }
   /// <summary>
   ///    The current status of the feature, such as whether it's in development, released, or deprecated.
   /// </summary>
   public FeatureStatus Status { get; set; }
   /// <summary>
   ///    The VersionNumber the feature was introduced.
   /// </summary>
   public VersionNumber IntroducedIn { get; set; } = VersionNumbers.Undefined;
   /// <summary>
   ///    An optional path to an icon representing this feature.
   /// </summary>
   public string? IconPath { get; set; }

   // The file path of the .md file
   public string SourcePath { get; set; } = string.Empty;

   // Content:
   public string Content { get; set; } = string.Empty;

   // Data Verifying
   public bool IsAllContentFilled(out string errorMessage)
   {
      var sb = new StringBuilder();

      if (string.IsNullOrEmpty(Title))
         sb.AppendLine("Title is empty.");
      if (string.IsNullOrEmpty(Summary))
         sb.AppendLine("Summary is empty.");
      if (string.IsNullOrEmpty(Content))
         sb.AppendLine("Content is empty.");
      if (Id == FeatureIds.Empty)
         sb.AppendLine("FeatureId is not set.");
      if (Content == string.Empty)
         sb.AppendLine("Content is empty.");

      errorMessage = sb.ToString();
      return string.IsNullOrEmpty(errorMessage);
   }

   public DocuSection? GetSection(FeatureSection section) => Sections.FirstOrDefault(s => s.Section == section);
   private const string BASE_DOCU_ICON_PATH = "pack://application:,,,/Arcanum_UI;component/Documentation/DocuPages/DocuPages/Icons/";
   private const string DEFAULT_ICON_GEOMETRY = "M10,0 L20,10 L10,20 L0,10 Z"; // Default diamond shape
   private readonly Geometry _defaultIconGeometry = Geometry.Parse(DEFAULT_ICON_GEOMETRY);
   public string GetIconUri => string.IsNullOrEmpty(IconPath)
                                  ? "pack://application:,,,/Resources/Icons/default_feature_icon.png"
                                  : $"{BASE_DOCU_ICON_PATH}{IconPath}";
   public Geometry GetIconGeometry
   {
      get
      {
         if (string.IsNullOrEmpty(IconPath))
            return _defaultIconGeometry;

         // Get the geometry from $"/Arcanum_UI;component/Assets/ArcanumShared/DefaultGeometry.xaml" by name
         return Application.Current.TryFindResource(IconPath) as Geometry ?? _defaultIconGeometry;
      }
   }
   public bool IconIsPng => IconPath?.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ?? false;

   public override string ToString() => Id.Value;
}