using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.Windows.DebugWindows.DebugPanel.VMs;
using Arcanum.UI.Components.Windows.MinorWindows;

namespace Arcanum.UI.Components.Windows.DebugWindows;

// ReSharper disable once InconsistentNaming
public partial class Debug_Panel
{
   public Debug_Panel()
   {
      InitializeComponent();
   }

   private void BrowseProvincesButton_Click(object sender, RoutedEventArgs e)
   {
      DebugView.Content = new BrowseProvincesView();
   }

   private void FindNullMarketsButton_Click(object sender, RoutedEventArgs e)
   {
      var count = 0;
      var str = string.Empty;
      foreach (var loc in Globals.Locations.Values)
      {
         if (loc.Market == null!)
         {
            str += loc.Name;
            str += ", ";
            count++;
         }
      }

      DebugView.Content = new TextBlock { Text = $"Found {count} locations with null markets: {str}" };
   }

   private void FindNullLocationInMarketButton_Click(object sender, RoutedEventArgs e)
   {
      var count = 0;
      var str = string.Empty;
      foreach (var loc in Globals.Locations.Values)
      {
         if (loc.Market.Location == null!)
         {
            str += loc.Name;
            str += ", ";
            count++;
         }
      }

      DebugView.Content = new TextBlock { Text = $"Found {count} markets with null locations: {str}" };
   }

   private void OpenSavingWindowButton_Click(object sender, RoutedEventArgs e)
   {
      var sw = new SaveWindow();
      sw.Show();
   }
}