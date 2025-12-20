using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.Registry;
using Arcanum.UI.Components.Windows.PopUp;

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class Renamer
{
   public static Type[] AvailableTypes => [typeof(Location), typeof(Province), typeof(Area), typeof(Region), typeof(SuperRegion), typeof(Continent)];

   // Holds the full list for the selected type
   private List<IEu5Object> _allObjectsOfCurrentType = [];

   public static readonly DependencyProperty ObjectsOfTargetTypeProperty =
      DependencyProperty.Register(nameof(ObjectsOfTargetType),
                                  typeof(ObservableCollection<IEu5Object>),
                                  typeof(Renamer),
                                  new(null));

   public ObservableCollection<IEu5Object> ObjectsOfTargetType
   {
      get => (ObservableCollection<IEu5Object>)GetValue(ObjectsOfTargetTypeProperty);
      set => SetValue(ObjectsOfTargetTypeProperty, value);
   }

   public ObservableCollection<PrendingRename> PendingRenames { get; } = [];

   public Renamer()
   {
      InitializeComponent();
      DataContext = this;
      ObjectsOfTargetType = [];
   }

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
      foreach (var rename in PendingRenames)
      {
         if (rename.Target is not Location loc)
         {
            var success = Core.CoreSystems.RenamingEngine.Renamer.RenameIEu5Object(rename.Target, rename.NewId);
            if (!success)
               MBox.Show($"Failed to rename {rename.Target.UniqueId} to {rename.NewId}.");
            objectsToSave.Add(rename.Target);
         }
         else
         {
            var success = Core.CoreSystems.RenamingEngine.Renamer.RenameIEu5Object(rename.Target, rename.NewId);
            if (!success)
               MBox.Show($"Failed to rename {rename.Target.UniqueId} to {rename.NewId}.");
            objectsToSave.Add(loc);
            objectsToSave.Add(loc.Province);
            objectsToSave.Add(loc.TemplateData);
         }
      }

      var splash = new SavingSplashScreen();
      splash.Show();
      SaveMaster.SaveObjects(objectsToSave, splash.UpdateProgress);
      splash.Close();

      PendingRenames.Clear();
      FilterAndDisplayItems(ItemSearchBox.Text);
   }
}

public record PrendingRename(IEu5Object Target, string NewId);