using System.Diagnostics;
using System.IO;
using System.Reflection;
using Arcanum.API;
using Arcanum.API.CrossPluginServices;
using Arcanum.API.Events;
using Arcanum.PluginHost;
using Arcanum.PluginHost.PluginServices;

namespace Arcanum.Core.PluginServices;

public class PluginManager : ISubroutineLogger
{
   private const string PM_LOG_PREFIX = "PlgnMngr"; // Prefix should be short, max 8 characters.
   private readonly Dictionary<Guid, IPlugin> _plugins = [];
   private readonly string _pluginFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
   public IReadOnlyList<IPlugin> LoadedPlugins => _plugins.Values.ToList();
   public IPlugin? this[Guid id] => _plugins.GetValueOrDefault(id);

   private readonly IEventBus _events;
   private readonly ICrossPluginCommunicator _cpcService;
   private IPluginInfoService PInfoService { get; }

   public IPluginHost Host { get; }

   public PluginManager(IPluginHost host)
   {
      Host = host;
      PInfoService = new PluginInfoService(this);
      // Register the plugin info service so that it can be used by other plugins. 
      // We do this here to not expose the PluginInfoService directly to the plugins.
      _cpcService = new CrossPluginCommunicatorService(host, PInfoService);
      Host.RegisterService(PInfoService);
      Host.RegisterService(_cpcService);
      _events = host.GetService<IEventBus>();
   }

#if DEBUG
   public void InjectPluginForTesting(IPlugin plugin)
   {
      _plugins[plugin.Guid] = plugin;
   }
#endif

   // How Plugins are loaded:
   // 1. Load all plugins from the specified folder.
   // 2. For each DLL, find all types that implement IPlugin.
   // 3. For each plugin type, create an instance and add it to the _plugins dictionary.
   // 4. We rebuild the dependency graph for all plugins.
   // 5. Initialize each plugin in the sorted order.

   private void InitializeWithDependencies()
   {
      // We only pass in the plugins that are in the Created state or already initialized.
      var sortedPlugins =
         DependencyManager.TopologicalSort(_plugins.Where(x => x.Value.Status >= PluginStatus.Created)
                                                   .ToDictionary(kvp => kvp.Key,
                                                                 kvp => new DependencyManager.PluginNode(kvp.Value)));

      foreach (var plugin in sortedPlugins)
      {
         var pluginInstance = _plugins[plugin.Guid];
         // Only if the plugin is in the Created state, we initialize it.
         // Otherwise, we assume it has already been initialized or is in an error state.
         if (pluginInstance.Status == PluginStatus.Created)
            SaveInitializePlugin(pluginInstance);
      }
   }

   private class ReloadedPluginInfo(Guid guid, string dllPath, bool reloaded)
   {
      public Guid Guid { get; } = guid;
      public string DllPath { get; } = dllPath;
      public bool Reloaded { get; set; } = reloaded;
   }

   public bool ReloadPlugin(IPlugin plugin, bool reloadDependencies, bool reloadDependants)
   {
      ArgumentNullException.ThrowIfNull(plugin);
      if (_plugins.ContainsKey(plugin.Guid) == false)
      {
         Log($"Plugin {plugin.Name} is not loaded, cannot reload.", LoggingVerbosity.Warning);
         return false;
      }

      // Reloading Process:
      // 1. We dispose of the plugin and its dependencies if specified.
      // 2. We find all the disposed plugins that were either dependants or dependencies of the plugin
      //    from the assembly location of the plugins.
      // 3. We load each plugin from its assembly location.
      // 4. We rebuild the dependency graph for all plugins as dependencies might have changed.
      // 5. We initialize each plugin in the sorted order if it is not already initialized.

      try
      {
         var pluginsToReload = DisposePlugin(plugin, reloadDependants, reloadDependencies);

         var reloadInfo = pluginsToReload
                         .Select(p => new ReloadedPluginInfo(p.Guid, p.AssemblyPath, false))
                         .ToArray();

         foreach (var reload in reloadInfo)
            reload.Reloaded = GetSpecificPluginFromDll(reload.DllPath, reload.Guid) != null;

         _events.Trigger(PluginEventId.Plugin_OnPluginReloaded,
                         new BasePluginEventArgs(plugin.Guid, EventSource.Core));
         if (!reloadInfo.All(r => r.Reloaded))
         {
            Log($"Failed to reload plugin {plugin.Name} and its dependencies.",
                LoggingVerbosity.Error);
            return false;
         }

         Log($"Reloaded plugin {plugin.Name} and its dependencies successfully.");
         return true;
      }
      catch (Exception ex)
      {
         Log($"Failed to reload plugin {plugin.Name}: {ex.Message}", LoggingVerbosity.Error);
         return false;
      }
   }

   /// <summary>
   /// This is the initial method to load and initialize plugins.
   /// </summary>
   public void LoadAndInitializePlugins()
   {
      Log("Loading plugins from folder...");

      var loadedPlugins = LoadPluginsFromFolder();
      if (loadedPlugins.Count == 0)
      {
         Log("No plugins found in the plugin folder.");
         return;
      }

      InitializeWithDependencies();
      foreach (var plugin in loadedPlugins.Where(plugin => plugin.Status == PluginStatus.Initialized))
      {
         plugin.OnEnable();
         plugin.Status = PluginStatus.Enabled;
      }
   }

   /// <summary>
   /// Gathers all plugins from the specified folder and creates an instance of each plugin.
   /// </summary>
   private List<IPlugin> LoadPluginsFromFolder()
   {
      if (!Directory.Exists(_pluginFolder))
      {
         Log($"Plugin folder does not exist: {DesensitizePath(_pluginFolder)}");
         Directory.CreateDirectory(_pluginFolder);
         return [];
      }

      List<IPlugin> loadedPlugins = [];
      foreach (var dll in Directory.GetFiles(_pluginFolder, "*.dll"))
         try
         {
            loadedPlugins.AddRange(LoadPluginsFromDll(dll));
         }
         catch (Exception ex)
         {
            Log($"Failed to load {dll}: {ex.Message}");
         }

      Log($"Loaded {loadedPlugins.Count} plugins from folder: {DesensitizePath(_pluginFolder)}");
      return loadedPlugins;
   }

   // Finds all plugins in the specified DLL and only loads the one where the name matches the pluginName parameter.
   private IPlugin? GetSpecificPluginFromDll(string dllPath, Guid guid)
   {
      var context = new PluginLoadContext(dllPath);

      var pluginTypes = GetPluginTypesFromDll(context.LoadFromAssemblyPath(dllPath));

      if (pluginTypes == null! || pluginTypes.Length == 0)
         return null;

      IPlugin? plugin = null;
      foreach (var pluginType in pluginTypes)
      {
         if (Activator.CreateInstance(pluginType) is not IPlugin instance)
            return plugin;

         if (instance.Guid == guid)
            plugin = CreateInstanceAndAddDependencies(pluginType, dllPath);
      }

      return plugin;
   }

   private List<IPlugin> LoadPluginsFromDll(string dllPath)
   {
      var context = new PluginLoadContext(dllPath);

      var assembly = context.LoadFromAssemblyPath(dllPath);
      var pluginTypes = GetPluginTypesFromDll(assembly);

      if (pluginTypes.Length == 0)
         return [];

      List<IPlugin> loadedPlugins = [];
      foreach (var pluginType in pluginTypes)
      {
         var plugin = CreateInstanceAndAddDependencies(pluginType, dllPath);
         if (plugin == null)
            continue;

         loadedPlugins.Add(plugin);
      }

      return loadedPlugins;
   }

   /// <summary>
   /// Creates an instance of the plugin and adds it to the loaded plugins list.
   /// Generates a PluginNode for the plugin and adds it to the dependency graph.
   /// </summary>
   /// <param name="pluginType"></param>
   /// <param name="assemblyPath"></param>
   private IPlugin? CreateInstanceAndAddDependencies(Type pluginType, string assemblyPath)
   {
      var plugin = CreatePluginInstance(pluginType);
      if (plugin.RequiredHostVersion > HostInfo.Version)
      {
         Log($"Plugin {pluginType.FullName} requires a newer version of the host: {plugin.RequiredHostVersion}",
             LoggingVerbosity.Error);
         return null;
      }

      // Plugins are only added to _plugins if an instance could be created.
      // We still want to have them in the _plugins list even if they fail to initialize,
      // so we can show them in the UI and allow the user to reload them and or look at the logs.
      _plugins.Add(plugin.Guid, plugin);
      // We set the status to Created so we know that the plugin was created and valid.
      plugin.Status = PluginStatus.Created;
      plugin.AssemblyPath = assemblyPath;
      return plugin;
   }

   private void SaveInitializePlugin(IPlugin plugin)
   {
      try
      {
         var sw = Stopwatch.StartNew();
         // if the plugin returns false, it will be marked as unloaded and inactive.;
         plugin.IsActive = plugin.Initialize(Host);
         sw.Stop();
         plugin.RuntimeInfo = new(plugin.IsActive, sw.Elapsed);
         plugin.Status = !plugin.IsActive ? PluginStatus.Error : PluginStatus.Initialized;
      }
      catch (Exception ex)
      {
         Log($"Failed to initialize plugin {plugin.Name}: {ex.Message}", LoggingVerbosity.Error);
         plugin.Status = PluginStatus.Error;
         plugin.IsActive = false;
         return;
      }

      Log($"Loaded plugin: {plugin.Name}");
   }

   private static Type[] GetPluginTypesFromDll(Assembly assembly)
   {
      return assembly.GetTypes()
                     .Where(t => typeof(IPlugin).IsAssignableFrom(t) &&
                                 t is { IsAbstract: false, IsInterface: false })
                     .ToArray();
   }

   private static IPlugin CreatePluginInstance(Type pluginType)
   {
      var plugin = (IPlugin)Activator.CreateInstance(pluginType)!;
      if (plugin.RequiredHostVersion > HostInfo.Version)
         throw new($"Plugin {pluginType.FullName} requires a newer version of the host: {plugin.RequiredHostVersion}");

      return plugin;
   }

   public void UnloadAll()
   {
      foreach (var plugin in _plugins.Values)
      {
         if (plugin.Status is PluginStatus.Unloading or PluginStatus.Disabled)
            continue;

         // We unload all plugins anyway, so there is no need to take care of dependencies or dependants here.
         DisposePlugin(plugin, false);
      }

      _plugins.Clear();
      GC.Collect();
      GC.WaitForPendingFinalizers();
   }

   /// <summary>
   /// Disposes the specified plugin and optionally unloads its dependencies.
   /// Updates the plugin's status and cleans up related resources in the plugin manager.
   /// </summary>
   /// <param name="plugin">The plugin to be disposed of.</param>
   /// <param name="unloadDependants">Indicates whether dependants of the plugin should also be unloaded.</param>
   /// <param name="unloadDependencies">Indicates whether dependencies of the plugin should also be unloaded</param>
   /// <returns>A list of plugins that were disposed of, including the specified plugin and its unloaded dependencies, if applicable.</returns>
   private List<IPlugin> DisposePlugin(IPlugin plugin, bool unloadDependants, bool unloadDependencies = false)
   {
      List<IPlugin> disposedPlugins = [];
      if (plugin.Status is PluginStatus.Unloading or PluginStatus.Disabled)
         return disposedPlugins;

      disposedPlugins.Add(plugin);
      if (unloadDependants)
      {
         var dependencies = DependencyManager.GetAllDependentOn(plugin, LoadedPlugins);
         disposedPlugins.AddRange(dependencies.Select(dep => _plugins[dep.Guid]));
         foreach (var dep in dependencies)
            SaveDisposePlugin(_plugins[dep.Guid]);
      }

      if (unloadDependencies)
      {
         var dependants = DependencyManager.GetAllDependentFor(plugin, LoadedPlugins);
         disposedPlugins.AddRange(dependants.Select(dep => _plugins[dep.Guid]));
         foreach (var dep in dependants)
            SaveDisposePlugin(_plugins[dep.Guid]);
      }

      SaveDisposePlugin(plugin);

      return disposedPlugins;
   }

   private void SaveDisposePlugin(IPlugin plugin)
   {
      if (plugin.Status is PluginStatus.Unloading or PluginStatus.Disabled)
         return;

      plugin.Status = PluginStatus.Unloading;
      plugin.IsActive = false;
      if (plugin is IPluginLibrary { AutoUnloadServices: true })
         _cpcService.UnpublishAllServices(plugin);
      plugin.Dispose();
      Log($"Unloaded plugin: {plugin.Name}");
      _plugins.Remove(plugin.Guid);
      plugin.Status = PluginStatus.Unloaded;
   }

   public void Log(string message, LoggingVerbosity verbosity = LoggingVerbosity.Info)
      => Host.Log(PM_LOG_PREFIX, message, verbosity);

   private string AppPath => AppDomain.CurrentDomain.BaseDirectory;

   private string DesensitizePath(string path)
   {
      if (string.IsNullOrEmpty(path))
         return string.Empty;
      if (path.StartsWith(AppPath, StringComparison.OrdinalIgnoreCase))
         return "'Arcanum/" +
                path[AppPath.Length..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                "'";

      return path;
   }
}