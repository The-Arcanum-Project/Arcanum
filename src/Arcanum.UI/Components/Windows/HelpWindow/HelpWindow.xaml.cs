using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.Windows.HelpWindow.ViewModels;

namespace Arcanum.UI.Components.Windows.HelpWindow;

public partial class HelpWindow
{
   public HelpWindow()
   {
      DataContext = new HelpWindowViewModel();
      InitializeComponent();
   }

   private void Sidebar_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (DataContext is HelpWindowViewModel vm && e.AddedItems.Count > 0)
         vm.Navigate((NavMenuItem)e.AddedItems[0]!);
   }

   private void HelpWindow_OnClosing(object? sender, CancelEventArgs e)
   {
      JsonProcessor.Serialize(Path.Combine(IO.GetArcanumDataPath, Config.CONFIG_FILE_NAME), Config.Settings);
   }
}