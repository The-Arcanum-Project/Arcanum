#region

using Arcanum.Core.Settings.BaseClasses;

#endregion

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
   // ReSharper disable once CollectionNeverUpdated.Global
   public List<string> ErrorsToHandle { get; set; } = [];
   public bool SkipLoading { get; set; } = false;
   public string ExternalDocumentationPath { get; set; } = string.Empty;
   public bool UseExternalDocumentation { get; set; } = false;
   public bool ProduceCrashLogs { get; set; } = false;
}
#endif