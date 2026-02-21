using System.Diagnostics;
using System.Windows.Controls;
using Arcanum.Core.CoreSystems.Nexus;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.UserControls.BaseControls;
using Arcanum.UI.Components.UserControls.BaseControls.AutoCompleteBox;
using Arcanum.UI.SpecializedEditors.EditorControls.ViewModels;

namespace Arcanum.UI.SpecializedEditors.EditorControls;

public partial class PoliticalEditor
{
   private static readonly Lazy<PoliticalEditor> LazyInstance = new(() => new());
   public static PoliticalEditor Instance => LazyInstance.Value;

   public PoliticalEditor()
   {
      ViewModel = new();
      DataContext = this;
      InitializeComponent();

      if (SelectionManager.EditableObjects.Count == 1 && SelectionManager.EditableObjects[0] is Country country)
         ViewModel.UpdateViewModel(country);
   }

   public PoliticalEditorViewModel ViewModel { get; }

   public static IEnumerable<Country> Countries => Globals.Countries.Values;
   public static IEnumerable<Location> Locations => Globals.Locations.Values;

   public void AddLocations(EntitySelector selector)
   {
      var target = ViewModel.SelectedCountry;
      if (target is null)
         return;

      var locations = Selection.GetSelectedLocations;

      if (selector.IsUniqueInAll)
         EnforceUniqueInAll(locations, target);
      else if (selector.IsUniqueInProperty)
         EnforceUniqueInProperty(locations, target, selector.TargetProperty);

      Nx.AddRangeToCollection(target, selector.TargetProperty, locations);
   }

   public void RemoveLocations(EntitySelector selector)
   {
      var target = ViewModel.SelectedCountry;
      if (target is null)
         return;

      RemoveFromCollection(Selection.GetSelectedLocations, selector.TargetProperty, target);
   }

   private static void EnforceUniqueInProperty(List<Location> locations, Country target, Enum property)
   {
      foreach (var country in Countries)
      {
         if (country == target)
            continue;

         RemoveFromCollection(locations, property, country);
      }
   }

   private static void EnforceUniqueInAll(List<Location> locations, Country target)
   {
      foreach (var country in Countries)
      {
         if (country == target)
            continue;

         RemoveFromCollection(locations, Country.Field.OwnControlCores, country);
         RemoveFromCollection(locations, Country.Field.ControlCores, country);
         RemoveFromCollection(locations, Country.Field.OwnControlIntegrated, country);
         RemoveFromCollection(locations, Country.Field.OwnControlConquered, country);
         RemoveFromCollection(locations, Country.Field.OwnControlColony, country);
         RemoveFromCollection(locations, Country.Field.OwnCores, country);
         RemoveFromCollection(locations, Country.Field.OwnConquered, country);
         RemoveFromCollection(locations, Country.Field.OwnIntegrated, country);
         RemoveFromCollection(locations, Country.Field.OwnColony, country);
         RemoveFromCollection(locations, Country.Field.Control, country);
         RemoveFromCollection(locations, Country.Field.OurCoresConqueredByOthers, country);
      }
   }

   private static void RemoveFromCollection(List<Location> locations, Enum field, Country target)
   {
      List<Location> toRemove = [];
      var collection = target[field] as ObservableRangeCollection<Location>;

      Debug.Assert(collection != null);

      foreach (var location in locations)
         if (collection.Contains(location))
            toRemove.Add(location);

      Nx.RemoveRangeFromCollection(target, field, toRemove);
   }

   private void CountrySelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is not AutoCompleteComboBox { SelectedItem: Country country })
         return;

      ViewModel.UpdateViewModel(country);
   }
}