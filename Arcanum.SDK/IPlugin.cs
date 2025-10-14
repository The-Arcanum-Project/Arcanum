namespace Arcanum.API;

/// <summary>
/// Represents the different statuses that a plugin can have during its lifecycle.
/// The statuses indicate the current state of a plugin in the hosting application.
/// </summary>
public enum PluginStatus
{
   Error = 0, // Plugin encountered an error during its lifecycle, preventing it from functioning correctly.
   Disabled = 1, // Plugin is currently inactive but still loaded in the host application.
   Unloaded = 2, // Plugin has been completely unloaded from the host application and is no longer active.
   Unloading = 3, // Plugin is in the process of being unloaded from the host application.
   Created = 4, // Plugin has been created but not yet loaded into the host application,

   // nor have its dependencies been resolved.
   Initialized = 5, // Plugin has been initialized, loaded and is ready for use.
   Enabled = 6, // Plugin is currently active and operational within the host application.
}

/// <summary>
/// Represents the basic interface for implementing plugins.
/// Defines the essential methods required for plugins to integrate with a host application.
/// </summary>
public interface IPlugin : IPluginMetadata, ISubroutineLogger
{
   /// <summary>
   /// Gets or sets the current operational state of the plugin.
   /// Represents the lifecycle status of the plugin, including states such as
   /// not loaded, loading, initialized, enabled, or error.
   /// </summary>
   public PluginStatus Status { get; set; }

   /// <summary>
   /// Gets or sets a value indicating whether the plugin is currently active.
   /// Represents the activation state of the plugin, which can be used to enable or
   /// disable its functionality within the host application.
   /// </summary>
   bool IsActive { get; set; }

   /// <summary>
   /// Gets or sets the runtime information of the plugin.
   /// Provides details such as the load state, load and unload times, associated assembly,
   /// and any exceptions encountered during the plugin's lifecycle.
   /// </summary>
   PluginRuntimeInfo RuntimeInfo { get; set; }

   /// <summary>
   /// Gets or sets the file path to the assembly from which the plugin was loaded.
   /// This property typically stores the absolute path of the plugin's binary file
   /// and is used for reference or for reloading purposes.
   /// </summary>
   public string AssemblyPath { get; set; }

   /// <summary>
   /// Initializes the plugin with the specified host.
   /// This method is called when the plugin is loaded by the host application,
   /// allowing the plugin to set up any necessary resources or register events.
   /// </summary>
   /// <param name="host">The host application that the plugin is being initialized with.</param>
   bool Initialize(IPluginHost host);

   /// <summary>
   /// Called when the plugin is enabled within the host application.
   /// This method allows the plugin to activate its functionality,
   /// subscribe to necessary events, or perform any initialization required for its operation.
   /// </summary>
   void OnEnable();

   /// <summary>
   /// Invoked when the plugin is disabled within the host application.
   /// This method allows the plugin to deactivate its functionality, unsubscribe from events,
   /// and perform any necessary cleanup of resources allocated during its active state.
   /// </summary>
   void OnDisable();

   /// <summary>
   /// Releases resources used by the plugin and performs any necessary cleanup operations.
   /// This method is invoked when the plugin is being unloaded from the host application.
   /// Implementations should ensure that all resources allocated during the plugin's lifecycle
   /// are appropriately released to avoid memory leaks or lingering references.
   /// </summary>
   void Dispose();
}

/// <summary>
/// Represents runtime information about a plugin.
/// </summary>
public record PluginRuntimeInfo
{
   public PluginRuntimeInfo(bool isLoaded, TimeSpan loadTime)
   {
      IsLoaded = isLoaded;
      LoadTime = loadTime;
      UnloadTime = TimeSpan.Zero;
      LastException = null;
   }

   /// <summary>
   /// Gets or sets a value indicating whether the plugin has been successfully loaded into the host application.
   /// </summary>
   internal bool IsLoaded { get; set; }

   /// <summary>
   /// Gets or sets the duration of time it took to load the plugin.
   /// </summary>
   internal TimeSpan LoadTime { get; set; }
   /// <summary>
   /// Gets or sets the duration of time it took to unload the plugin.
   /// </summary>
   internal TimeSpan UnloadTime { get; set; }
   /// <summary>
   /// Gets or sets the last exception that occurred during the plugin's execution.
   /// </summary>
   internal Exception? LastException { get; set; }
}

/// <summary>
/// Defines metadata properties for plugins.
/// Provides essential information about the plugin, such as version, author, and compatibility with the host application.
/// </summary>
public interface IPluginMetadata
{
   /// <summary>
   /// Gets the unique identifier of the plugin.
   /// This globally unique identifier (GUID) is used to distinguish the plugin
   /// from other plugins within the application, ensuring there are no conflicts
   /// in identification or functionality.
   /// Generates this GUID using a tool or library that ensures uniqueness across all plugins.
   /// Do NOT change this GUID once it has been assigned to the plugin, otherwise it may lead to conflicts,
   /// break existing installations, or cause issues with plugin management systems.
   /// </summary>
   public Guid Guid { get; }
   /// <summary>
   /// Gets the version of the plugin.
   /// Represents the version number of the plugin, which can be used to identify
   /// and manage different versions of the same plugin.
   /// </summary>
   public Version PluginVersion { get; }

   /// <summary>
   /// Represents the minimum required version of the host application that is compatible with the plugin.
   /// This property is used to ensure that the plugin can operate correctly within the specified version of the host environment.
   /// </summary>
   public Version RequiredHostVersion { get; }

   /// <summary>
   /// Gets the name of the plugin.
   /// Represents the unique name or identifier of the plugin,
   /// often used for logging, display in interfaces, or distinguishing between different plugins.
   /// </summary>
   public string Name { get; }

   /// <summary>
   /// Gets the author of the plugin.
   /// Represents the name or identifier of the individual or organization
   /// that created the plugin, providing credit and traceability for the plugin's source.
   /// </summary>
   public string Author { get; }

   /// <summary>
   /// Gets the collection of dependencies required by the plugin.
   /// Each dependency specifies another plugin, identified by a unique identifier and a minimum required version,
   /// that must be available and meet the specified version criteria for this plugin to function correctly.
   /// </summary>
   public IEnumerable<PluginDependency> Dependencies { get; }

   /// <summary>
   /// Represents a required dependency for a plugin.
   /// Specifies another plugin by its unique identifier and the minimum required version.
   /// </summary>
   /// <param name="RequiredVersion">The minimum version of the required plugin that satisfies the dependency.</param>
   public record PluginDependency(Guid PluginGuid, Version RequiredVersion);
}