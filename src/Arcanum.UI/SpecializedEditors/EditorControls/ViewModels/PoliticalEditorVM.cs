using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Arcanum.Core.ApplicationContext.Contexts.SpecializedEditors;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Commands;

namespace Arcanum.UI.SpecializedEditors.EditorControls.ViewModels;

public class PoliticalEditorViewModel : IPoliticalEditor, INotifyPropertyChanged
{
   public PoliticalEditorViewModel()
   {
      SyncWithSearchCommand = CommandRegistry.Get(CommandIds.Editor.SpecializedEditors.PoliticalEditor.SyncWithSelection);
      SubscribeToSelectionChanges();
   }

   public IAppCommand SyncWithSearchCommand { get; }
   public static IEnumerable<Country> Countries => Globals.Countries.Values;
   public static IEnumerable<Location> Locations => Globals.Locations.Values;
   public void SubscribeToSelectionChanges() => SelectionManager.EditableObjectsChanged += SyncWithSelectionEffect;
   public void UnsubscribeFromSelectionChanges() => SelectionManager.EditableObjectsChanged -= SyncWithSelectionEffect;

   public ObservableCollection<Location> OwnControlCoreLocations
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   } = [];
   public ObservableCollection<Location> OwnControlIntegratedLocations
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   } = [];
   public ObservableCollection<Location> OwnControlConqueredLocations
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   } = [];
   public ObservableCollection<Location> OwnControlColonyLocations
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   } = [];
   public ObservableCollection<Location> OwnCoreLocations
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   } = [];
   public ObservableCollection<Location> OwnConqueredLocations
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   } = [];
   public ObservableCollection<Location> OwnIntegratedLocations
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   } = [];
   public ObservableCollection<Location> OwnColonyLocations
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   } = [];
   public ObservableCollection<Location> ControlCoreLocations
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   } = [];
   public ObservableCollection<Location> ControlLocations
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   } = [];
   public ObservableCollection<Location> OurCoresConqueredByOthersLocations
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   } = [];
   public Country? SelectedCountry
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   }
   public bool SyncWithSearch
   {
      get;
      set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = true;

   public event PropertyChangedEventHandler? PropertyChanged;

   public void Clear()
   {
      SelectedCountry = null;

      OwnControlCoreLocations = [];
      OwnControlIntegratedLocations = [];
      OwnControlConqueredLocations = [];
      OwnControlColonyLocations = [];
      OwnCoreLocations = [];
      OwnConqueredLocations = [];
      OwnIntegratedLocations = [];
      OwnColonyLocations = [];
      ControlCoreLocations = [];
      ControlLocations = [];
      OurCoresConqueredByOthersLocations = [];
   }

   public void UpdateViewModel(Country country)
   {
      SelectedCountry = country;

      OwnControlCoreLocations = country.OwnControlCores;
      OwnControlIntegratedLocations = country.OwnControlIntegrated;
      OwnControlConqueredLocations = country.OwnControlConquered;
      OwnControlColonyLocations = country.OwnControlColony;
      OwnCoreLocations = country.OwnCores;
      OwnConqueredLocations = country.OwnConquered;
      OwnIntegratedLocations = country.OwnIntegrated;
      OwnColonyLocations = country.OwnColony;
      ControlCoreLocations = country.ControlCores;
      ControlLocations = country.Control;
      OurCoresConqueredByOthersLocations = country.OurCoresConqueredByOthers;
   }

   protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
      PropertyChanged?.Invoke(this, new(propertyName));
   }

   protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
   {
      if (EqualityComparer<T>.Default.Equals(field, value))
         return false;

      field = value;
      OnPropertyChanged(propertyName);
      return true;
   }

   public void SyncWithSelectionEffect()
   {
      if (SelectionManager.EditableObjects.Count == 1 && SyncWithSearch && SelectionManager.EditableObjects[0] is Country country)
         UpdateViewModel(country);
   }

   public void ToggleSyncState()
   {
      SyncWithSearch = !SyncWithSearch;
      if (SyncWithSearch)
         SyncWithSelectionEffect();
   }
}