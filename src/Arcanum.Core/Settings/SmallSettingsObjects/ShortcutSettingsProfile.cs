using Arcanum.Core.CoreSystems.KeyMap;

namespace Arcanum.Core.Settings.SmallSettingsObjects;

public class ShortcutSettingsProfile
{
   public List<CommandShortcutProfile> CommandProfiles { get; set; } = [];
}

public sealed record ShortcutEntry(string Key, string Modifiers);

public sealed class CommandShortcutProfile
{
   public string CommandId { get; set; } = string.Empty;
   public string CommandDisplayName { get; set; } = string.Empty;
   public string Scope { get; set; } = string.Empty;
   public List<ShortcutChord> Shortcuts { get; set; } = [];
}