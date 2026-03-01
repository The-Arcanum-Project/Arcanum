using System.Windows;
using System.Windows.Controls;
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

   private void Close_Click(object sender, RoutedEventArgs e) => Close();
}