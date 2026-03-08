using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.InGame.Cultural;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.AppFeatures.Contexts.SpecializedEditors;
using Arcanum.UI.Components.UserControls.SpecializedEditors.VMs;
#if DEBUG
using System.Diagnostics;
using Common.Logger;
#endif

namespace Arcanum.UI.SpecializedEditors.EditorControls.ViewModels;

public sealed class InstitutionViewModel : IInstitutionEditor, INotifyPropertyChanged
{
   private static readonly Dictionary<Institution, int> InstitutionToIndex = new(Globals.Institutions.Count);

   public InstitutionViewModel()
   {
      SubscribeToSelectionChanges();

      foreach (var institution in Globals.Institutions.Values)
      {
         var vm = new InstitutionSetterVm(institution);
         vm.StateChanged += (_, _) => InternalUpdate();
         InstitutionSetters.Add(vm);
         InstitutionToIndex[institution] = 0;
      }
   }

   public string HeaderText
   {
      get;
      set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = "Institutions for selected location(s)";

   public ObservableCollection<InstitutionSetterVm> InstitutionSetters { get; } = [];

   public event PropertyChangedEventHandler? PropertyChanged;

   public void SubscribeToSelectionChanges() => SelectionManager.EditableObjectsChanged += UpdateHeaderText;
   public void UnsubscribeFromSelectionChanges() => SelectionManager.EditableObjectsChanged -= UpdateHeaderText;

   private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
      PropertyChanged?.Invoke(this, new(propertyName));
   }

   public void Reset()
   {
      UpdateHeaderText();
   }

   private void InternalUpdate()
   {
      UpdateHeaderText();
      var locations = SelectionManager.GetActiveSelectionLocations();
      SetForLocations(locations.ToArray());
   }

   public void SetForLocations(Location[] locations)
   {
#if DEBUG
      var sw = Stopwatch.StartNew();
#endif
      ResetIndexes();

      foreach (var loc in locations)
         foreach (var presence in loc.InstitutionPresences)
            InstitutionToIndex[presence.Institution] += 1;

      var count = locations.Length;
      foreach (var setter in InstitutionSetters)
      {
         var indexValue = InstitutionToIndex[setter.Institution];
         if (indexValue == count)
            setter.State = PresenceState.All;
         else if (indexValue == 0)
            setter.State = PresenceState.None;
         else
            setter.State = PresenceState.Mixed;
      }
#if DEBUG
      sw.Stop();
      ArcLog.WriteLine("IVM", LogLevel.DBG, $"Institution VM update took {sw.ElapsedMilliseconds} ms");
#endif
   }

   private static void ResetIndexes()
   {
      foreach (var institution in InstitutionToIndex)
         InstitutionToIndex[institution.Key] = 0;
   }

   private void UpdateHeaderText()
   {
      var locations = SelectionManager.GetActiveSelectionLocations();
      if (locations.Count == 0)
      {
         HeaderText = "Institutions for selected location(s)";
         return;
      }

      if (locations.Count == 1)
      {
         var loc = locations[0];
         HeaderText = $"Institutions for {loc.UniqueId}";
         return;
      }

      HeaderText = $"Institutions for {locations.Count} selected locations";
   }
}