using Arcanum.Core.Settings;
using Arcanum.Core.Settings.BaseClasses;

#if DEBUG
namespace Arcanum.Core.GlobalStates;

public static class DebugConfig
{
   public const string DEBUG_CONFIG_FILE_PATH = "debug_config.json";

   /// <summary>
   /// This contains all user-defined configuration settings for debugging purposes.
   /// </summary>
   public static DebugConfigSettings Settings { get; set; } = new();
}

/// <summary>
/// This will only be used for debugging purposes, and if the project is built in Debug mode.
/// </summary>
public class DebugConfigSettings() : InternalSearchableSetting(Config.Settings)
{
   public bool EnableDebugLogging { get; set; } = true;
   public bool SkipMainMenu { get; set; } = false;
   public bool SuppressAllErrors { get; set; } = false;
   public bool OnlyHandleSpecifiedErrors { get; set; } = false;
   public List<string> ErrorsToHandle { get; set; } = [];
}
#endif