using System.Diagnostics;
using System.Windows;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.UserControls.ValueAllocators;
using Arcanum.UI.Components.Windows.DebugWindows.DebugPanel.VMs;
using Arcanum.UI.Components.Windows.MinorWindows;
using Arcanum.UI.Components.Windows.PopUp;
using Arcanum.UI.Saving.Window;
using Arcanum.UI.Util.WindowManagement;
using Common.Logger;

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
      new LogWindow().Show();
   }

   private void FindNullLocationInMarketButton_Click(object sender, RoutedEventArgs e)
   {
      var foundAny = false;
      foreach (var country in Globals.Countries.Values)
      {
         if (country.GovernmentState.SocietalValues.Count > 0)
         {
            Debug.WriteLine($"Country {country.UniqueId} has societal values:");
            foreach (var svEntry in country.GovernmentState.SocietalValues)
               Debug.WriteLine($" - {svEntry.SocientalValue} with intensity {svEntry.Value}");
            foundAny = true;
         }
      }

      if (!foundAny)
         Debug.WriteLine("No countries with societal values found.");
   }

   private void OpenSavingWindowButton_Click(object sender, RoutedEventArgs e)
   {
      WindowManager.OpenWindow<SaveWindow>(true);
   }

   private void OpenSavingWindowExporterButton_Click(object sender, RoutedEventArgs e)
   {
      WindowManager.OpenWindow<AgsWindow>(true);
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
      Eu5ObjectCreator.ShowDialog(typeof(Location));
   }

   private void ModifierCreatorButton_Click(object sender, RoutedEventArgs e)
   {
      var modCreator = new ModifierCreator();
      modCreator.ShowDialog();

      if (modCreator.CreatedInstance != null)
         Console.WriteLine($"Created modifier instance: {modCreator.CreatedInstance}");
   }

   private void GraphViewerButton_Click(object sender, RoutedEventArgs e)
   {
      new GraphViewer().Show();
   }

   private void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
   {
      Selection.ClearAll();
   }

   private void FileWatcherTest_Click(object sender, RoutedEventArgs e)
   {
      new FileChange().Show();
   }

   private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
   {
   }

   private void InsertLogSpacerButton_Click(object sender, RoutedEventArgs e)
   {
      ArcLog.WriteLine("DBP", LogLevel.INF, "----------------------------------------");
   }

   private void PopsEditorTestButton_Click(object sender, RoutedEventArgs e)
   {
      var bwindow = new BaseWindow { Title = "Pops Editor Test" };
      var allocator = new PopsEditor();
      var locs = Globals.Locations.Values.ToArray();
      var loc = locs[0]; //Random.Shared.Next(0, locs.Length)
      var allocatroVm = new AllocatorViewModel(loc);
      allocator.DataContext = allocatroVm;
      bwindow.Content = allocator;
      bwindow.Width = 500;
      bwindow.Height = 1000;
      bwindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      bwindow.Show();
   }
}