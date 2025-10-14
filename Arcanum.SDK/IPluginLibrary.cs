namespace Arcanum.API;

/// <summary>
/// Represents the interface for a plugin library within the application.
/// Extends the IPlugin interface to include additional functionality
/// specific to managing plugin services.
/// </summary>
public interface IPluginLibrary : IPlugin
{
   /// <summary>
   /// Gets or sets a value indicating whether the plugin's services should be automatically unloaded
   /// when the plugin is disabled or unloaded.
   /// </summary>
   /// <remarks>
   /// When set to true, the plugin system automatically releases resources and unloads services
   /// associated with the plugin upon its deactivation or unloading. This setting can be especially
   /// useful for maintaining resource efficiency and ensuring proper cleanup of plugin dependencies.
   /// </remarks>
   public bool AutoUnloadServices { get; set; }
}