using System.IO;
using Arcanum.API;
using Arcanum.API.Console;
using Arcanum.API.Core.IO;
using Arcanum.Core.CoreSystems.ConsoleServices;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.CoreSystems.Parsing.DocsParsing;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Arcanum;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.GlobalStates.BackingClasses;
using Arcanum.Core.PluginServices;

namespace Arcanum.Core.FlowControlServices;

public class LifecycleManager
{
   public static LifecycleManager Instance => LfmInstance;

   /* --- Bootup Sequence ---
    *
    *  0. Initialization of the plugin host
    *  1. Initialization of the core services
    *  2. Plugin host service initialization
    *  3. Loading of configuration settings
    *  4. Plugin discovery and loading, Initialization and enabling of plugins
    *  5. Showing of MainMenu
    *  Here it has several options:
    *    - Load game data -> Default editor
    *    - Map editor
    *    - Plugin manager
    *    - Project files manager
    */

   public PluginManager PluginManager { get; private set; } = null!;
   private static readonly LifecycleManager LfmInstance = new();

   public void RunStartUpSequence(IPluginHost host)
   {
#if DEBUG
      // Step 0: Initialize debug elements
      LoadDebugElements();
#endif

      InitializeApplicationCore();
      // Step 1: Initialize core services
      LoadConfig();
      InitializeCoreServices(host);

      // Step 2: Initialize plugin host services
      host.RegisterDefaultServices();

      // Step 3: Load configuration settings
      //host.LoadConfigurationSettings();

      // Step 4: Discover, load and enable plugins
      PluginManager = new(host);
      PluginManager.LoadAndInitializePlugins();

      // Step 5: Show the main menu or UI
      //host.ShowMainMenu();
   }

#if DEBUG

   private void LoadDebugElements()
   {
      DebugConfig.Settings =
         JsonProcessor.DefaultDeserialize<DebugConfigSettings>(Path.Combine(IO.GetArcanumDataPath,
                                                                            DebugConfig.DEBUG_CONFIG_FILE_PATH)) ??
         new DebugConfigSettings();
   }

   private void SaveDebugElements()
   {
      JsonProcessor.Serialize(Path.Combine(IO.GetArcanumDataPath, DebugConfig.DEBUG_CONFIG_FILE_PATH),
                              DebugConfig.Settings);
   }
#endif

   public void RunShutdownSequence()
   {
      // Step 1: Unload plugins
      PluginManager.UnloadAll();

      // Step 2: Unload core services
      PluginManager.Host.Unload();

      // Step 3: Perform any additional cleanup if necessary
      // This might include saving state, closing files, etc.

      // Shutdown the core application
      ArcanumDataHandler.SaveAllGitData(new());

      MainMenuScreenDescriptor.SaveData();

      // Save configs
      JsonProcessor.Serialize(Path.Combine(IO.GetArcanumDataPath, Config.CONFIG_FILE_PATH), Config.UserKeyBinds);

#if DEBUG
      SaveDebugElements();
#endif
   }

   private void LoadConfig()
   {
      Config.UserKeyBinds =
         JsonProcessor.DefaultDeserialize<UserKeyBinds>(Path.Combine(IO.GetArcanumDataPath,
                                                                     Config.CONFIG_FILE_PATH)) ??
         new UserKeyBinds();
   }

#if DEBUG
   public void InsertPluginForTesting(IPlugin plugin)
   {
      if (PluginManager == null)
         throw new InvalidOperationException("PluginManager is not initialized. Call RunStartUpSequence first.");

      PluginManager.InjectPluginForTesting(plugin);
   }
#endif

   private static void InitializeApplicationCore()
   {
      ArcanumDataHandler.LoadDefaultDescriptor(new());
      MainMenuScreenDescriptor.LoadData();
      EffectsAndTriggersDocsParser.LoadDocs();
   }

   private static void InitializeCoreServices(IPluginHost host)
   {
      // Initialize core services here
      // This might include logging, configuration management, etc.
      host.RegisterService<IFileOperations>(new APIWrapperIO());
      host.RegisterService<IJsonProcessor>(new APIWrapperJsonProcessor());
      host.RegisterService<IConsoleService>(new ConsoleServiceImpl(host, "ArcanumConsole"));
   }
}