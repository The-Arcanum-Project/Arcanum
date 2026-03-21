using System.ComponentModel;
using System.Runtime.CompilerServices;
using Arcanum.Core.CoreSystems.Nexus;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.InGame.Cultural;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections.SubObjects;
using Arcanum.UI.Components.Windows.DebugWindows;

namespace Arcanum.UI.Components.UserControls.SpecializedEditors.VMs;

public class InstitutionSetterVm : INotifyPropertyChanged
{
   public event PropertyChangedEventHandler? PropertyChanged;

   public event EventHandler? StateChanged;

   public PresenceState State
   {
      get;
      set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
      }
   }

   public string Name => Institution.UniqueId;
   public Institution Institution { get; init; }

   public RelayCommand SetInAllCommand { get; }
   public RelayCommand SetInNoneCommand { get; }

   public InstitutionSetterVm(Institution institution)
   {
      SetInAllCommand = new(SetInAllAction);
      SetInNoneCommand = new(SetInNoneAction);

      Institution = institution;
   }

   private void SetInAllAction(object? _)
   {
      var locations = SelectionManager.GetActiveSelectionLocations();
      var objects = new object[locations.Count];

      for (var i = 0; i < locations.Count; i++)
         objects[i] = new InstitutionPresence()
         {
            Institution = Institution, IsPresent = true,
         };

      Nx.BulkAddToCollection(locations.Cast<IEu5Object>().ToArray(),
                             Location.Field.InstitutionPresences,
                             objects);

      StateChanged?.Invoke(this, EventArgs.Empty);
   }

   private void SetInNoneAction(object? _)
   {
      var locations = SelectionManager.GetActiveSelectionLocations();
      List<object> objects = [];
      List<IEu5Object> locationsToUpdate = [];

      foreach (var loc in locations)
      {
         var presences = loc.InstitutionPresences;
         var presence = presences.FirstOrDefault(p => p.Institution == Institution);
         if (presence is null)
            continue;

         objects.Add(presence);
         locationsToUpdate.Add(loc);
      }

      Nx.BulkRemoveFromCollection(locationsToUpdate.ToArray(), Location.Field.InstitutionPresences, objects.ToArray());

      StateChanged?.Invoke(this, EventArgs.Empty);
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
}

public enum PresenceState
{
   None,
   Mixed,
   All,
}