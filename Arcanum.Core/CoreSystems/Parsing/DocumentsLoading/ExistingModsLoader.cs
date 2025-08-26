using System.IO;
using System.Text.Json;
using Arcanum.Core.CoreSystems.SavingSystem;

namespace Arcanum.Core.CoreSystems.Parsing.DocumentsLoading;

public static class ExistingModsLoader
{
   public static List<ModMetadata> LoadExistingMods()
   {
      var allModFolders = IO.IO.GetDirectories(FileManager.GetDocumentsPath("mod"));
      if (allModFolders.Count == 0)
         return [];

      List<ModMetadata> existingMods = [];
      foreach (var modFolder in allModFolders)
      {
         var metadata = ParseModMetadata(modFolder);
         if (metadata != null)
            existingMods.Add(metadata);
      }
      return existingMods;
   }

   public static ModMetadata? ParseModMetadata(string modFolder)
   {
      var modPath = Path.Combine(modFolder, ".metadata", "metadata.json");
      var content = IO.IO.ReadAllTextUtf8(modPath);
      if (string.IsNullOrEmpty(content))
         return null;

      using var doc = JsonDocument.Parse(content);
      var root = doc.RootElement;

      var modName = root.GetProperty("name").GetString();
      if (string.IsNullOrEmpty(modName))
         return null;

      var metadata = new ModMetadata(modName);
      metadata.Id = root.GetProperty("id").GetString() ?? string.Empty;
      metadata.ShortDescription = root.GetProperty("short_description").GetString() ?? string.Empty;
      metadata.Version = root.GetProperty("version").GetString() ?? "1.0.0";
      metadata.SupportedGameVersion = root.GetProperty("supported_game_version").GetString() ?? "1.0.0";
      metadata.Tags = root.GetProperty("tags")
                          .EnumerateArray()
                          .Select(tag => tag.GetString() ?? string.Empty)
                          .ToArray();
      metadata.ThumbnailPath = root.GetProperty("picture").GetString() ?? string.Empty;
      metadata.Dependencies = root.GetProperty("relationships")
                                  .EnumerateArray()
                                  .Select(dep => dep.GetString() ?? string.Empty)
                                  .ToArray();

      var gameCustomData = root.GetProperty("game_custom_data");
      metadata.IsMultiplayerSynchronized = gameCustomData.GetProperty("multiplayer_synchronized").GetBoolean();
      metadata.ReplacePaths = gameCustomData.GetProperty("replace_paths")
                                            .EnumerateArray()
                                            .Select(path => path.GetString() ?? string.Empty)
                                            .ToArray();
      
      return metadata;
   }
}