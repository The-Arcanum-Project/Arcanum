using System.Windows;
using Arcanum.API.Settings;
using Arcanum.Core;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.CoreSystems.ParsingSystem;
using Arcanum.Core.FlowControlServices;
using Arcanum.Core.Utils.vdfParser;
using Arcanum.UI.Components.MVVM.Converters;
using Arcanum.UI.Components.Windows.MainWindows;
using Arcanum.UI.HostUIServices.SettingsGUI;
using Arcanum.UI.WpfTesting;
using Nexus.Core;

namespace Arcanum.UI;

internal static class Program
{
   /// <summary>
   ///  The main entry point for the application.
   /// </summary>
   [STAThread]
   private static void Main()
   {
      var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };

      var resources = new[]
      {
         "Assets/ArcanumShared/DefaultPalette.xaml", "Assets/ArcanumShared/DefaultFonts.xaml",
         "Components/Base/Styles/BaseDarkButton.xaml", "Components/Base/Styles/BaseTextBoxStyle.xaml",
         "Components/Base/Styles/BaseComboboxStyle.xaml", "Components/Base/Styles/BorderlessComboBox.xaml",
         "Components/Base/Styles/BaseCheckBox.xaml", "Components/Base/Styles/BaseDarkScrollbar.xaml",
         "Components/Base/Styles/BaseListBox.xaml", "Components/Base/Styles/BaseTextBlock.xaml",
         "Components/Base/Styles/DarkTabControl.xaml", "Components/Base/Styles/StackPanelStyle.xaml",
         "Components/Base/Styles/BaseToolTip.xaml", "Components/Base/Styles/BaseMenuStyle.xaml",
         "Components/Base/Styles/BaseMenuItemStyle.xaml", 
      };

      foreach (var path in resources)
      {
         var dict = new ResourceDictionary { Source = new(path, UriKind.Relative) };
         app.Resources.MergedDictionaries.Add(dict);
      }

      var pluginHost = new PluginHost.PluginHost();
      LifecycleManager.Instance.RunStartUpSequence(pluginHost);

      var mw = new MainMenuScreen();
      mw.Closing += (_, _) => { LifecycleManager.Instance.RunShutdownSequence(); };
      app.MainWindow = mw;

      mw.Show();
      app.Run();
   }
}