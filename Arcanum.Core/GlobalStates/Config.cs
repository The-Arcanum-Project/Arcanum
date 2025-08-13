using Arcanum.Core.Settings;

namespace Arcanum.Core.GlobalStates;

/// <summary>
/// This contains all user-defined configuration settings.
/// </summary>
public static class Config
{
   internal const string CONFIG_FILE_PATH = "config.json";
   
   public static MainSettingsObj Settings { get; set; } = new ();
}