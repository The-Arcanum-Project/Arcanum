using System.Windows;
using Arcanum.Core.FlowControlServices;
using Arcanum.UI.Components.Windows.MainWindows;

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
      };

      foreach (var path in resources)
      {
         var dict = new ResourceDictionary { Source = new(path, UriKind.Relative) };
         app.Resources.MergedDictionaries.Add(dict);
      }

      var pluginHost = new PluginHost.PluginHost();
      LifecycleManager.Instance.RunStartUpSequence(pluginHost);

      var mw = new MainMenuScreen();
      app.MainWindow = mw;

      mw.Show();
      app.Run();
      
      LifecycleManager.Instance.RunShutdownSequence();
   }
}