using System.Diagnostics;
using System.IO;
using System.Windows;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Utils;
using Arcanum.UI.Commands;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.UserControls.ValueAllocators;
using Arcanum.UI.Components.Windows.DebugWindows.DebugPanel.VMs;
using Arcanum.UI.Components.Windows.MinorWindows;
using Arcanum.UI.Components.Windows.MinorWindows.ContextExplorer;
using Arcanum.UI.Components.Windows.PopUp;
using Arcanum.UI.Saving.Window;
using Arcanum.UI.Util.WindowManagement;
using Common.Logger;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

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
         if (country.GovernmentState.SocietalValues.Count > 0)
         {
            Debug.WriteLine($"Country {country.UniqueId} has societal values:");
            foreach (var svEntry in country.GovernmentState.SocietalValues)
               Debug.WriteLine($" - {svEntry.SocientalValue} with intensity {svEntry.Value}");
            foundAny = true;
         }

      if (!foundAny)
         Debug.WriteLine("No countries with societal values found.");
   }

   private void OpenSavingWindowButton_Click(object sender, RoutedEventArgs e)
   {
      WindowManager.OpenWindow<SaveWindow>(true);
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
         Debug.WriteLine($"Created modifier instance: {modCreator.CreatedInstance}");
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

   private async void SplashDeterminedButton_Click(object sender, RoutedEventArgs e)
   {
      await SaveWithKnownCount();
   }

   private async Task SaveWithKnownCount()
   {
      var filesToSave = new List<string>();
      for (var i = 0; i < 10; i++)
         filesToSave.Add($"mod_texture_{i}.png");

      var splash = new SavingSplashScreen(filesToSave.Count) { Owner = Application.Current.MainWindow };

      splash.Show();

      try
      {
         await Task.Run(async () =>
         {
            foreach (var file in filesToSave)
            {
               await Task.Delay(500);
               splash.UpdateProgress(file);
            }
         });
      }
      finally
      {
         splash.MarkAsComplete();
      }
   }

   private async Task SaveWithUnknownCount()
   {
      var splash = new SavingSplashScreen { Owner = Application.Current.MainWindow };
      splash.Show();

      try
      {
         await Task.Run(async () =>
         {
            for (var i = 0; i < 15; i++)
            {
               await Task.Delay(300);
               splash.UpdateProgress($"dynamic_file_{i}.dat");
            }
         });
      }
      finally
      {
         splash.MarkAsComplete();
      }
   }

   private async void SplashIntermediateButton_Click(object sender, RoutedEventArgs e)
   {
      await SaveWithUnknownCount();
   }

   private void ExperimentalRenameButton_Click(object sender, RoutedEventArgs e)
   {
      var renamer = new Renamer();
      renamer.Show();
   }

   private void CommandGenerator_OnClick(object sender, RoutedEventArgs e)
   {
      var cmdGen = new CommandGenerator();
      cmdGen.Show();
   }

   private void TemplateToPops_OnClick(object sender, RoutedEventArgs e)
   {
      var templateToPops = new TemplateToPopsWindow();
      templateToPops.Show();
   }

   private void WordGenerator_OnClick(object sender, RoutedEventArgs e)
   {
      var wordGen = new NameGenerator();
      wordGen.Show();
   }

   private void ASTBuilderViewer_OnClick(object sender, RoutedEventArgs e)
   {
      new AstBuilderTest().Show();
   }

   private void ImageTagDecode_OnClick(object sender, RoutedEventArgs e)
   {
      new WatermarkCheckerWindow().Show();
   }

   private void BindingTest_OnClick(object sender, RoutedEventArgs e)
   {
      new CommandBindingTest().ShowDialog();
   }

   private void ExportAllCommands_OnClick(object sender, RoutedEventArgs e)
   {
      IO.WriteAllTextUtf8(Path.Combine(IO.GetArcanumDataPath, "CMD_docs.md"), CommandDocEngine.ExportMarkdown());
   }

   private void ExportSelectedCommands_OnClick(object sender, RoutedEventArgs e)
   {
      IO.WriteAllTextUtf8(Path.Combine(IO.GetArcanumDataPath, "CMD_docs.json"), CommandDocEngine.ExportJson());
   }

   private void ExportToHtml_OnClick(object sender, RoutedEventArgs e)
   {
      IO.WriteAllTextUtf8(Path.Combine(IO.GetArcanumDataPath, "CMD_docs.html"), CommandDocEngine.ExportHtml());
   }

   private void ContextExplorer_OnClick(object sender, RoutedEventArgs e)
   {
      var ownerWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
      new ContextExplorerWindow(ownerWindow!).Show();
   }

   private void CustomNumberParser_OnClick(object sender, RoutedEventArgs e)
   {
      var data = new Dictionary<string, double> { { "x", 123.456 }, { "y", 123.456 } };

      // @formatter:off
      var tests = new List<string>
      {
         "{x}",                  // Default
         "{x#.#}",               // Rounding Up
         "{x#.##}",              // Rounding Down
         "{x#.###}",             // Exact
         "{x#.#####}",           // Trailing Zeros
         "{y:F}",                // .NET Standard
         "{y:F0}",               // .NET No Decimals
         "{y:P}",                // .NET Percent
         "{z}",                  // Missing Variable
         "Value: {x#.##} units", // Mixed String
         "{x#.#.#}",             // Invalid Syntax
      };
      // @formatter:on

      ArcLog.WritePure($"{"Input Pattern",-20} | Result");
      ArcLog.WritePure(new('-', 40));

      foreach (var t in tests)
      {
         var result = CustomNumberParser.Format(t, data);
         ArcLog.WritePure($"{t,-20} | {result}");
      }
   }

   private void DocuPageTest_OnClick(object sender, RoutedEventArgs e)
   {
      new DocuPageTest().Show();
   }
}