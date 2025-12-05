using System.Diagnostics;
using System.Windows;
using Arcanum.Core.CoreSystems.ErrorSystem;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.FlowControlServices;
using Arcanum.Core.GlobalStates;
using Arcanum.UI;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.Windows.MainWindows;
using Common.Logger;
#if !DEBUG
using Arcanum.UI.Components.Windows.MinorWindows.CrashHandler;
#endif

namespace Arcanum.App;

internal static class Program
{
   /// <summary>
   ///  The main entry point for the application.
   /// </summary>
   [STAThread]
   private static void Main(string[] args) // CHANGED: Added args
   {
#if !DEBUG
      try
      {
         InternalApplicationRun(args);
      }
      catch (Exception e)
      {
         // In headless mode, write to console too
         if (args.Contains("--headless") || args.Contains("-batch"))
         {
            ArcLog.WriteLine(CommonLogSource.PRT, LogLevel.INF, $"CRITICAL ERROR: {e.Message}");
            ArcLog.WriteLine(CommonLogSource.PRT, LogLevel.INF, e.StackTrace);
         }

         CrashHandler.Show(e);
         Application.Current?.Shutdown(1);
      }
#else
      InternalApplicationRun(args);
#endif
   }

   private static void InternalApplicationRun(string[] args)
   {
      var app = new Application { ShutdownMode = ShutdownMode.OnLastWindowClose };
      _ = typeof(BaseWindow);
      LoadApplicationResources(app);

      // Initialize Plugin Host and Lifecycle Manager
      var pluginHost = new PluginHost.PluginHost();
      UiHandlesInjector.InjectUiHandles();
      LifecycleManager.Instance.RunStartUpSequence(pluginHost);

      if (args.Contains("--headless") || args.Contains("-h"))
      {
         // --- HEADLESS MODE ---
         try
         {
            ConsoleHelper.InitConsole();
            var config = ArgumentParser.ParseArguments(args);
            if (!config.IsValid)
            {
               ArcLog.WriteLine(CommonLogSource.PRT, LogLevel.INF, "Invalid startup configuration for headless mode.");
               return;
            }

            AppData.IsHeadless = true;
            Debug.Assert(config.ModPath != null);
            ArcLog.WriteLine(CommonLogSource.PRT, LogLevel.INF, "Init FileManager...");
            FileManager.InitHeadlessMode(config.ModPath, config.BaseMods);
            RunHeadlessLogic().GetAwaiter().GetResult();

            ArcLog.WriteLine(CommonLogSource.PRT, LogLevel.INF, "Logic executed successfully.");

            ErrorManager.PrintDiagnosticsToConsole();
         }
         catch (Exception ex)
         {
            ArcLog.WriteLine(CommonLogSource.PRT, LogLevel.INF, $"Error during headless execution: {ex.Message}");
            Environment.ExitCode = 1;
         }
         finally
         {
            ArcLog.WriteLine(CommonLogSource.PRT, LogLevel.INF, "Shutting down...");
            LifecycleManager.Instance.RunShutdownSequence();
            ConsoleHelper.ReleaseConsole();
            app.Shutdown(Environment.ExitCode);
         }
      }
      else
      {
         // --- GUI MODE ---
         app.ShutdownMode = ShutdownMode.OnLastWindowClose;

         var mw = new MainMenuScreen();
         app.MainWindow = mw;

         mw.Show();
         app.Run();

         LifecycleManager.Instance.RunShutdownSequence();
      }
   }

   private static async Task RunHeadlessLogic()
   {
      var success = await ParsingMaster.Instance.ExecuteAllParsingSteps();
      ArcLog.WriteLine(CommonLogSource.PRT, LogLevel.INF, success ? "Parsing completed successfully." : "Parsing failed.");
   }

   private static void LoadApplicationResources(Application app)
   {
      const string uiAssemblyName = "Arcanum_UI";

      var resources = new[]
      {
         $"/{uiAssemblyName};component/Assets/ArcanumShared/DefaultPalette.xaml", $"/{uiAssemblyName};component/Assets/ArcanumShared/DefaultFonts.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseButton.xaml", $"/{uiAssemblyName};component/Components/Styles/Base/BaseTextBoxStyle.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseComboboxStyle.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Specific/BorderlessComboBox.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseCheckBox.xaml", $"/{uiAssemblyName};component/Components/Styles/Base/BaseScrollbar.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseListBox.xaml", $"/{uiAssemblyName};component/Components/Styles/Base/BaseTextBlock.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseTabControl.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Specific/StackPanelStyle.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseToolTip.xaml", $"/{uiAssemblyName};component/Components/Styles/Base/BaseMenuStyle.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseMenuItemStyle.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseGridSplitter.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseContextMenu.xaml", $"/{uiAssemblyName};component/Components/Styles/Base/BaseDataGrid.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseToggleButton.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseTreeView.xaml", $"/{uiAssemblyName};component/Components/Styles/Base/BaseListView.xaml",
         $"/{uiAssemblyName};component/Components/UserControls/BaseControls/AutoCompleteBox/AutoCompleteComboBoxStyle.xaml",
      };

      foreach (var path in resources)
      {
         var uri = new Uri(path, UriKind.RelativeOrAbsolute);
         var dict = new ResourceDictionary { Source = uri };
         app.Resources.MergedDictionaries.Add(dict);
      }
   }
}