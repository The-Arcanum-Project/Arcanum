using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.CoreSystems.KeyMap;

namespace Arcanum.Core.Settings.SmallSettingsObjects;

public sealed class CommandShortcut
{
   public string CommandId { get; set; } = string.Empty;
   public string CommandDisplayName { get; set; } = string.Empty;
   public string Scope { get; set; } = string.Empty;
   public List<ShortcutChord> Shortcuts { get; set; } = [];
}

public sealed class KeyMapState
{
   public const string DEFAULT_PROFILE_NAME = "Default";
   public string Active { get; set; } = DEFAULT_PROFILE_NAME;
   public List<KeyMapProfile> KeyMaps { get; set; } = [new()];

   public KeyMapProfile GetActiveMap()
   {
      return KeyMaps.FirstOrDefault(km => km.ProfileName == Active) ?? KeyMaps.First();
   }
}

public sealed class KeyMapProfile
{
   private const string FILE_NAME = "keymap_profiles.json";

   public string ProfileName { get; set; } = KeyMapState.DEFAULT_PROFILE_NAME;
   public List<CommandShortcut> CommandProfiles { get; set; } = [];

   public static void Deserialize()
   {
      var state = JsonProcessor.DefaultDeserialize<KeyMapState>(Path.Combine(IO.GetConfigPath, FILE_NAME));

      Config.Settings.KeyMapState = state ?? new();
   }

   public static void Serialize()
   {
      var maps = Config.Settings.KeyMapState;
      JsonProcessor.Serialize(Path.Combine(IO.GetConfigPath, FILE_NAME), maps);
   }

   public void AddProfile(CommandShortcut profile)
   {
      CommandProfiles.Add(profile);
      Serialize();
   }
}