using Arcanum.Core.GlobalStates.BackingClasses;
using Arcanum.Core.Settings;

namespace Arcanum.Core.GlobalStates;

/// <summary>
/// This contains all user-defined configuration settings.
/// </summary>
public static class Config
{
   public const string CONFIG_FILE_NAME = "config.json";
   public const string WINDOW_DATA_CONFIG_FILE_NAME = "window_config.json";
   internal const string DIAGNOSTIC_CONFIG_NAME = "diagnostics.json";
   public const int START_DRAG_DISTANCE = 5;
   public static MainSettingsObj Settings { get; set; } = new();

   public static WindowData WindowData { get; set; } = new();
}