using System.ComponentModel;

namespace Arcanum.Core.Settings.SmallSettingsObjects;

public class CompatilitySettings
{
   [Description("Whether to use a custom folder for game logs.")]
   [DefaultValue(false)]
   public bool UseCustomGameLogFolder { get; set; } = false;

   [Description("The custom folder to use for game logs. Only used if 'Use Custom Game Log Folder' is enabled.")]
   [DefaultValue("")]
   public string CustomGameLogFolder { get; set; } = string.Empty;
}