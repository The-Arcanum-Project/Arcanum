using Arcanum.Core.Settings;

namespace Arcanum.Core.GlobalStates;

/// <summary>
/// This contains all user-defined configuration settings.
/// </summary>
public static class Config
{
   public const string CONFIG_FILE_PATH = "config.json";
   internal const string DIAGNOSTIC_CONFIG_PATH = "diagnostics.json";
   public const int START_DRAG_DISTANCE = 5;
   public static MainSettingsObj Settings { get; set; } = new();
}