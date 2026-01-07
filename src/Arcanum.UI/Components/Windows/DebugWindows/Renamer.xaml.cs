using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Registry;
using Arcanum.UI.Components.Windows.PopUp;
using Area = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Area;
using Continent = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Continent;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;
using Province = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Province;
using Region = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Region;
using SuperRegion = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.SuperRegion;

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class Renamer
{
   public static readonly DependencyProperty ObjectsOfTargetTypeProperty =
      DependencyProperty.Register(nameof(ObjectsOfTargetType),
                                  typeof(ObservableCollection<IEu5Object>),
                                  typeof(Renamer),
                                  new(null));

   // Holds the full list for the selected type
   private List<IEu5Object> _allObjectsOfCurrentType = [];

   public Renamer()
   {
      InitializeComponent();
      DataContext = this;
      ObjectsOfTargetType = [];
   }

   public static Type[] AvailableTypes => [typeof(Location), typeof(Province), typeof(Area), typeof(Region), typeof(SuperRegion), typeof(Continent)];

   public ObservableCollection<IEu5Object> ObjectsOfTargetType
   {
      get => (ObservableCollection<IEu5Object>)GetValue(ObjectsOfTargetTypeProperty);
      set => SetValue(ObjectsOfTargetTypeProperty, value);
   }

   public ObservableCollection<PrendingRename> PendingRenames { get; } = [];

   private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (e.AddedItems.Count == 0 || e.AddedItems[0] is not Type selectedType)
         return;

      var items = ((IEu5Object)EmptyRegistry.Empties[selectedType])
                 .GetGlobalItemsNonGeneric()
                 .Values
                 .Cast<IEu5Object>()
                 .OrderBy(x => x.UniqueId)
                 .ToList();

      _allObjectsOfCurrentType = items;

      FilterAndDisplayItems(ItemSearchBox.Text);

      NewIdTextBox.Clear();
   }

   private void OnItemSearchChanged(object sender, TextChangedEventArgs e)
   {
      FilterAndDisplayItems(ItemSearchBox.Text);
   }

   private void FilterAndDisplayItems(string filter = "")
   {
      var filtered = string.IsNullOrWhiteSpace(filter)
                        ? _allObjectsOfCurrentType
                        : _allObjectsOfCurrentType.Where(i => i.UniqueId.Contains(filter, StringComparison.OrdinalIgnoreCase));

      ObjectsOfTargetType = new(filtered);
   }

   private void ApplyRename_OnClick(object sender, RoutedEventArgs e)
   {
      if (ObjectsListView.SelectedItem is not IEu5Object selectedObject)
         return;

      var newId = NewIdTextBox.Text.Trim();

      if (string.IsNullOrWhiteSpace(newId))
      {
         MBox.Show("New Unique ID cannot be empty.");
         return;
      }

      PendingRenames.Add(new(selectedObject, newId));

      NewIdTextBox.Clear();
   }

   private void Save_OnClick(object sender, RoutedEventArgs e)
   {
      List<IEu5Object> objectsToSave = [];
      List<IEu5Object> setupObjectsToSave = [];
      foreach (var rename in PendingRenames)
         if (rename.Target is not Location loc)
         {
            var success = Core.CoreSystems.RenamingEngine.Renamer.RenameIEu5Object(rename.Target, rename.NewId);
            if (!success)
               MBox.Show($"Failed to rename {rename.Target.UniqueId} to {rename.NewId}.");
            objectsToSave.Add(rename.Target);
         }
         else
         {
            loc.TemplateData.UniqueId = rename.NewId;
            var success = Core.CoreSystems.RenamingEngine.Renamer.RenameIEu5Object(rename.Target, rename.NewId);
            if (!success)
               MBox.Show($"Failed to rename {rename.Target.UniqueId} to {rename.NewId}.");
            objectsToSave.Add(loc);
            objectsToSave.Add(loc.Province);
            objectsToSave.Add(loc.TemplateData);
            foreach (var olc in Globals.GameObjectLocators.Values)
               foreach (var nd in olc.NudgeDatas)
                  if (nd.TargetLocation == loc)
                  {
                     objectsToSave.Add(olc);
                     break;
                  }

            foreach (var bdg in Globals.BuildingsManager.BuildingDefinitions)
               if (bdg.Location == loc)
                  objectsToSave.Add(bdg);

            setupObjectsToSave.Add(loc);
         }

      var splash = new SavingSplashScreen { Owner = Application.Current.MainWindow };
      splash.Show();
      SaveMaster.SaveObjects(objectsToSave, splash.UpdateProgress);
      SaveMaster.SaveSetupFolder(setupObjectsToSave, splash.UpdateProgress);
      splash.MarkAsComplete();

      PendingRenames.Clear();
      FilterAndDisplayItems(ItemSearchBox.Text);
   }
}

public record PrendingRename(IEu5Object Target, string NewId);