using System.Windows;
using Arcanum.Core.FlowControlServices;
using Arcanum.UI;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.Windows.MainWindows;
using Arcanum.UI.Components.Windows.MinorWindows.CrashHandler;

namespace Arcanum.App;

internal static class Program
{
   /// <summary>
   ///  The main entry point for the application.
   /// </summary>
   [STAThread]
   private static void Main()
   {
      try
      {
         InternalApplicationRun();
      }
      catch (Exception e)
      {
         CrashHandler.Show(e);
         Application.Current?.Shutdown();
      }
   }

   private static void InternalApplicationRun()
   {
      var app = new Application { ShutdownMode = ShutdownMode.OnLastWindowClose };
      _ = typeof(BaseWindow);
      const string uiAssemblyName = "Arcanum_UI";

      var resources = new[]
      {
         $"/{uiAssemblyName};component/Assets/ArcanumShared/DefaultPalette.xaml",
         $"/{uiAssemblyName};component/Assets/ArcanumShared/DefaultFonts.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseButton.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseTextBoxStyle.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseComboboxStyle.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Specific/BorderlessComboBox.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseCheckBox.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseScrollbar.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseListBox.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseTextBlock.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseTabControl.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Specific/StackPanelStyle.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseToolTip.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseMenuStyle.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseMenuItemStyle.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseGridSplitter.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseContextMenu.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseDataGrid.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseToggleButton.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseTreeView.xaml",
         $"/{uiAssemblyName};component/Components/Styles/Base/BaseListView.xaml",
         $"/{uiAssemblyName};component/Components/UserControls/BaseControls/AutoCompleteBox/AutoCompleteComboBoxStyle.xaml",
      };

      foreach (var path in resources)
      {
         var uri = new Uri(path, UriKind.RelativeOrAbsolute);
         var dict = new ResourceDictionary { Source = uri };
         app.Resources.MergedDictionaries.Add(dict);
      }

      var pluginHost = new PluginHost.PluginHost();

      UiHandlesInjector.InjectUiHandles();
      LifecycleManager.Instance.RunStartUpSequence(pluginHost);

      var mw = new MainMenuScreen();
      app.MainWindow = mw;

      mw.Show();
      app.Run();

      LifecycleManager.Instance.RunShutdownSequence();
   }
}