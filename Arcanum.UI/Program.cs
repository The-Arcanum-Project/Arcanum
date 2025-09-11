using System.Windows;
using Arcanum.Core.CoreSystems.Parsing.MapParsing;
using Arcanum.Core.FlowControlServices;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.WindowLinker;
using Arcanum.UI.Components.Windows.MainWindows;
using Arcanum.UI.WpfTesting;

namespace Arcanum.UI;

internal static class Program
{
   /// <summary>
   ///  The main entry point for the application.
   /// </summary>
   [STAThread]
   private static void Main()
   {
      var app = new Application { ShutdownMode = ShutdownMode.OnLastWindowClose };

      var resources = new[]
      {
         "Assets/ArcanumShared/DefaultPalette.xaml", "Assets/ArcanumShared/DefaultFonts.xaml",
         "Components/Styles/Base/BaseButton.xaml", "Components/Styles/Base/BaseTextBoxStyle.xaml",
         "Components/Styles/Base/BaseComboboxStyle.xaml", "Components/Styles/Specific/BorderlessComboBox.xaml",
         "Components/Styles/Base/BaseCheckBox.xaml", "Components/Styles/Base/BaseScrollbar.xaml",
         "Components/Styles/Base/BaseListBox.xaml", "Components/Styles/Base/BaseTextBlock.xaml",
         "Components/Styles/Base/BaseTabControl.xaml", "Components/Styles/Specific/StackPanelStyle.xaml",
         "Components/Styles/Base/BaseToolTip.xaml", "Components/Styles/Base/BaseMenuStyle.xaml",
         "Components/Styles/Base/BaseMenuItemStyle.xaml", "Components/Styles/Base/BaseGridSplitter.xaml",
         "Components/Styles/Base/BaseContextMenu.xaml", "Components/Styles/Base/BaseDataGrid.xaml",
         "Components/Styles/Base/BaseToggleButton.xaml", "Components/Styles/Base/BaseTreeView.xaml",
         "Components/Styles/Base/BaseListView.xaml",
      };

      foreach (var path in resources)
      {
         var dict = new ResourceDictionary { Source = new(path, UriKind.Relative) };
         app.Resources.MergedDictionaries.Add(dict);
      }


      AppData.WindowLinker = new WindowLinkerImpl();
      var pluginHost = new PluginHost.PluginHost();
      LifecycleManager.Instance.RunStartUpSequence(pluginHost);

      var mw = new ExampleWindow();
      //var mw = new MainMenuScreen();
      app.MainWindow = mw;
      
      var tracer = new MapTracing();
      //Task.Run(() => tracer.LoadLocations("D:\\SteamLibrary\\steamapps\\common\\Project Caesar Review\\game\\in_game\\map_data\\provinces_small.bmp", mw));
      Task.Run(() => tracer.LoadLocations("D:\\SteamLibrary\\steamapps\\common\\Project Caesar Review\\game\\in_game\\map_data\\locations.png", mw));
      /*tracer.LoadLocations(
         "D:\\SteamLibrary\\steamapps\\common\\Project Caesar Review\\game\\in_game\\map_data\\provinces_small.bmp",
         mw);*/
      //tracer.LoadLocations("D:\\SteamLibrary\\steamapps\\common\\Project Caesar Review\\game\\in_game\\map_data\\locations.png", mw);
      
      mw.Show();
      app.Run();

      
      LifecycleManager.Instance.RunShutdownSequence();
   }
}