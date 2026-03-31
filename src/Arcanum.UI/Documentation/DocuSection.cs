namespace Arcanum.UI.Documentation;

public struct DocuSection
{
   public FeatureSection Section { get; set; }
   public string Content { get; set; }
}

public enum FeatureSection
{
   Notes,
   Debug,
   // expand if required
}