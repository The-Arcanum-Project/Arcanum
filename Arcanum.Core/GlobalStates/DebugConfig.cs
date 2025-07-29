#if DEBUG
namespace Arcanum.Core.GlobalStates;

public static class DebugConfig
{
    public const string DEBUG_CONFIG_FILE_PATH = "debug_config.json";
    
    /// <summary>
    /// This contains all user-defined configuration settings for debugging purposes.
    /// </summary>
    public static DebugConfigSettings Settings { get; set; } = new ();
}

/// <summary>
/// This will only be used for debugging purposes, and if the project is built in Debug mode.
/// </summary>
public class DebugConfigSettings
{
    public bool EnableDebugLogging { get; set; } = true;
    public bool SkipMainMenu { get; set; } = false;
}
#endif