using System.Windows;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.Court;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.Windows.DebugWindows.DebugPanel.VMs;
using Arcanum.UI.Components.Windows.MinorWindows;

namespace Arcanum.UI.Components.Windows.DebugWindows.DebugPanel;

public partial class DebugPanelGrid
{
   public DebugPanelGrid()
   {
      InitializeComponent();
   }

   private void BrowseProvincesButton_Click(object sender, RoutedEventArgs e)
   {
      DebugView.Content = new BrowseProvincesView();
   }

   private void FindNullMarketsButton_Click(object sender, RoutedEventArgs e)
   {
   }

   private void FindNullLocationInMarketButton_Click(object sender, RoutedEventArgs e)
   {
   }

   private void OpenSavingWindowButton_Click(object sender, RoutedEventArgs e)
   {
      var climates = Globals.Regencies.Values.Cast<IEu5Object>().Take(2).ToList();
      climates.AddRange(Globals.Climates.Values.Take(2));
      var sw = new Saving.Window.SaveWindow(climates, [..climates]);
      sw.Show();
   }

   private void OpenSavingWindowExporterButton_Click(object sender, RoutedEventArgs e)
   {
      new AgsWindow().Show();
   }

   private void OpenTestWindowButton_Click(object sender, RoutedEventArgs e)
   {
      new ExportFileWindow().Show();
   }

   private void OpenNexusAccessorButton_Click(object sender, RoutedEventArgs e)
   {
      new NexusAccessorWindow().Show();
   }

   private void OpenEu5UiGenButton_Click(object sender, RoutedEventArgs e)
   {
      new Eu5Gen().Show();
   }

   private void ColorPickerViewerButton_Click(object sender, RoutedEventArgs e)
   {
      new ColorPickerWindow().Show();
   }

   private void ObjectCreatorButton_Click(object sender, RoutedEventArgs e)
   {
      Eu5ObjectCreator.ShowDialog(typeof(Character), out _);
   }
}