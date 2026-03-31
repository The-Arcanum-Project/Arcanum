using System.Text;
using Arcanum.UI.AppFeatures;

namespace Arcanum.UI.Documentation;

public class DocuPage
{
   // YAML Metadata
   public FeatureId Id { get; set; } = FeatureIds.Empty;
   public string Title { get; set; } = string.Empty;
   public string Summary { get; set; } = string.Empty;
   public string[] Links { get; set; } = [];
   public string[] SearchKeywords { get; set; } = [];
   public DocuSection[] Sections { get; set; } = [];
   public FeatureCategory Category { get; set; }
   public FeatureLevel Level { get; set; }
   public FeatureScale Scale { get; set; }
   public FeatureLocation Location { get; set; }
   public FeatureStatus Status { get; set; }

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

   public override string ToString() => Id.Value;
}