namespace Arcanum.UI.Documentation.Implementation;

public struct DocuSection
{
   public FeatureSection Section { get; set; }
   public string Content { get; set; }
}

public enum FeatureSection
{
   Notes,
   Debug,
   Tips,
   // expand if required
}