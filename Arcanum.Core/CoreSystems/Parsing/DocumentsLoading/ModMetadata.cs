namespace Arcanum.Core.CoreSystems.Parsing.DocumentsLoading;

public class ModMetadata(string name)
{
   public string Name { get; } = name;
   public string Id { get; set; } = null!;
   public string Version { get; set; } = null!;
   public string SupportedGameVersion { get; set; } = null!;
   public string ShortDescription { get; set; } = null!;
   public string[] Tags { get; set; } = null!;
   public string ThumbnailPath { get; set; } = null!;
   public string[] Dependencies { get; set; } = null!;
   public bool IsMultiplayerSynchronized { get; set; }
   public string[] ReplacePaths { get; set; } = null!;

   public override string ToString()
   {
      return $"{Name} (v{Version}) - {ShortDescription}\n" +
             $"Tags: {string.Join(", ", Tags)}\n" +
             $"Dependencies: {string.Join(", ", Dependencies)}\n" +
             $"Thumbnail: {ThumbnailPath}\n" +
             $"Supported Game Version: {SupportedGameVersion}\n" +
             $"Multiplayer Synchronized: {IsMultiplayerSynchronized}\n" +
             $"Replace Paths: {string.Join(", ", ReplacePaths)}";
   }

   public override bool Equals(object? obj)
   {
      if (obj is ModMetadata other)
         return Name == other.Name && Id == other.Id;

      return false;
   }

   public override int GetHashCode() => HashCode.Combine(Name, Id);
}