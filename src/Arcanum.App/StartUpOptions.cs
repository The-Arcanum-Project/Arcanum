namespace Arcanum.App;

public class StartupOptions
{
   public bool IsHeadless { get; set; } = false;
   public string? ModPath { get; set; }
   public List<string> BaseMods { get; set; } = [];

   // Helper to validate requirements
   public bool IsValid => !IsHeadless || !string.IsNullOrWhiteSpace(ModPath);
}