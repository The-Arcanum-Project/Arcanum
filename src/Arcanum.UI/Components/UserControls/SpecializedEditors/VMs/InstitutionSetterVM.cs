using System.ComponentModel;
using System.Runtime.CompilerServices;
using Arcanum.Core.CoreSystems.Nexus;
using Arcanum.Core.CoreSystems.Selection;
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

      foreach (var loc in locations)
         Nx.AddToCollection(loc,
                            Location.Field.InstitutionPresences,
                            new InstitutionPresence
                            {
                               Institution = Institution, IsPresent = true,
                            });

      StateChanged?.Invoke(this, EventArgs.Empty);
   }

   private void SetInNoneAction(object? _)
   {
      var locations = SelectionManager.GetActiveSelectionLocations();

      foreach (var loc in locations)
      {
         var presences = loc.InstitutionPresences;
         var presence = presences.FirstOrDefault(p => p.Institution == Institution);
         if (presence is null)
            continue;

         Nx.RemoveFromCollection(loc, Location.Field.InstitutionPresences, presence);
      }

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