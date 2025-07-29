using Arcanum.Core.GlobalStates.BackingClasses;

namespace Arcanum.Core.GlobalStates;

/// <summary>
/// This contains all user-defined configuration settings.
/// </summary>
public static class Config
{
   internal const string CONFIG_FILE_PATH = "config.json";
   
   public static UserKeyBinds UserKeyBinds { get; set; } = new ();
}