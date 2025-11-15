using System.Diagnostics;
using System.Windows;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.Windows.DebugWindows.DebugPanel.VMs;
using Arcanum.UI.Components.Windows.MinorWindows;
using Arcanum.UI.Components.Windows.PopUp;
using Arcanum.UI.Saving.Window;
using MultiCollectionEditor = Arcanum.UI.Components.Windows.MinorWindows.PopUpEditors.MultiCollectionEditor;

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
      var numModded = 0;
      foreach (var climate in Globals.Climates.Values)
         if (climate.Source.IsModded)
            numModded++;

      MessageBox.Show($"Found {numModded} modded climates.\nFound {Globals.Climates.Values.Count - numModded} vanilla climates.");
      //MessageBox.Show($"Num of objects to save: {SaveMaster.GetNeedsToBeSaveCount}");
   }

   private void OpenSavingWindowButton_Click(object sender, RoutedEventArgs e)
   {
      var sw = new SaveWindow();
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
      Eu5ObjectCreator.ShowDialog(typeof(Location), out _);
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
      var targets = Globals.Provinces.Values.Take(2);
      var ownerWindow = Window.GetWindow(this);

      var result = MultiCollectionEditor.ShowDialogN(ownerWindow!,
                                                     "TestEditing",
                                                     typeof(Location),
                                                     targets.Select(x => x.LocationChildren),
                                                     Globals.Locations.Values);
      Debug.WriteLine($"Result: {result}");
   }
}