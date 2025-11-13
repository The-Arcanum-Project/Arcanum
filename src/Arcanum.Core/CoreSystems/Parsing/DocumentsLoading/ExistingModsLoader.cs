using System.IO;
using System.Text.Json;
using Arcanum.Core.CoreSystems.SavingSystem;

namespace Arcanum.Core.CoreSystems.Parsing.DocumentsLoading;

public static class ExistingModsLoader
{
   private const string DEFAULT_VERSION = "1.0.0";

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
      {
         return null;
      }

      using var doc = JsonDocument.Parse(content);
      var root = doc.RootElement;

      var modName = root.GetProperty("name").GetString();
      if (string.IsNullOrEmpty(modName))
      {
         return null;
      }

      var metadata = new ModMetadata(modName);

      var hasId = root.TryGetProperty("id", out var id);
      metadata.Id = !hasId ? string.Empty : root.GetProperty("id").GetString() ?? string.Empty;

      var hasDescription = root.TryGetProperty("short_description", out var description);
      metadata.ShortDescription = !hasDescription
                                     ? string.Empty
                                     : root.GetProperty("short_description").GetString() ?? string.Empty;

      var hasVersion = root.TryGetProperty("version", out var version);
      metadata.Version = !hasVersion ? DEFAULT_VERSION : root.GetProperty("version").GetString() ?? DEFAULT_VERSION;

      var hasSupportedGameVersion = root.TryGetProperty("supported_game_version", out var supportedGameVersion);
      metadata.SupportedGameVersion = !hasSupportedGameVersion
                                         ? DEFAULT_VERSION
                                         : root.GetProperty("supported_game_version").GetString() ?? DEFAULT_VERSION;

      var hasTags = root.TryGetProperty("tags", out var tags);
      metadata.Tags = !hasTags
                         ? []
                         : tags
                          .EnumerateArray()
                          .Select(tag => tag.GetString() ?? string.Empty)
                          .ToArray() ?? [];

      var hasPicture = root.TryGetProperty("picture", out var picture);
      metadata.ThumbnailPath = !hasPicture ? string.Empty : picture.GetString() ?? string.Empty;

      var hasDependencies = root.TryGetProperty("relationships", out var dependencies);
      metadata.Dependencies = !hasDependencies
                                 ? []
                                 : dependencies
                                  .EnumerateArray()
                                  .Select(dep => dep.GetString() ?? string.Empty)
                                  .ToArray() ?? [];

      var hasCustomData = root.TryGetProperty("game_custom_data", out var customData);
      if (!hasCustomData)
      {
         metadata.IsMultiplayerSynchronized = false;
         metadata.ReplacePaths = [];
         return metadata;
      }

      var gameCustomData = root.GetProperty("game_custom_data");

      var hasIsMultiplayerSynchronized =
         gameCustomData.TryGetProperty("multiplayer_synchronized", out var multiplayerSynchronized);

      metadata.IsMultiplayerSynchronized =
         hasIsMultiplayerSynchronized && multiplayerSynchronized.GetBoolean();

      var hasReplacePaths = gameCustomData.TryGetProperty("replace_paths", out var replacePaths);
      metadata.ReplacePaths = !hasReplacePaths
                                 ? []
                                 : replacePaths
                                  .EnumerateArray()
                                  .Select(path => path.GetString() ?? string.Empty)
                                  .ToArray() ?? [];

      return metadata;
   }
}