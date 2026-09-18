#region

using System.Windows;
using Arcanum.Core.CoreSystems.Nexus;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.InGame.Cultural;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.Core.GameObjects.InGame.Pops;
using Arcanum.Core.GameObjects.InGame.Religious;
using Arcanum.UI.Components.Windows.MinorWindows.PopUpEditors;
using Arcanum.UI.Components.Windows.PopUp;
using Common.UI.MBox;

#endregion

namespace Arcanum.UI.Components.UserControls.ValueAllocators;

public sealed class MassPopPainterViewModel : ViewModelBase
{
   private const double POP_PRECISION_EPSILON = 0.001; // 0.001 = 1 Person (if 1.0 = 1k)
   private const int DECIMALS = 3;

   public event Action? UIResetRequested;
   private static PopPainterState? _lastState;

   public MassPopPainterViewModel(Location[] selectedLocations)
   {
      ResetFor(selectedLocations);
      OnRequestRefreshPreview();
   }

   public bool UseTotalRatioMode
   {
      get;
      set
      {
         if (SetField(ref field, value))
            OnRequestRefreshPreview();
      }
   } = false;

   // Toggles for active transformation
   public bool ModifyCulture
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
   public bool ModifyReligion
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
   public bool ModifyPopType
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

   public double GlobalTransferPercent
   {
      get;
      set
      {
         if (SetField(ref field, value))
            RefreshPreview(Selection.GetSelectedLocations.ToArray());
      }
   } = 100;

   // Preview Properties
   public int AffectedPopsCount { get; private set; }
   public int AffectedLocationsCount { get; private set; }

   public void Undo()
   {
   }

   public void ApplyChanges()
   {
      var selectedLocations = Selection.GetSelectedLocations;
      ApplyCombinedTransformation(selectedLocations.ToList());

      // Save state after a successful apply
      SaveState();
   }

   private void ApplyCombinedTransformation(List<Location> locations)
   {
      var ratio = Math.Clamp(GlobalTransferPercent / 100.0, 0.0, 1.0);

      if (ratio < 0.0001)
      {
         MBox.Show("Transfer percentage is too low. Please set a higher percentage before applying.",
                   "Invalid Configuration",
                   MBoxButton.OK,
                   MessageBoxImage.Warning);
         return;
      }

      // We have nothing to set.
      var hasTarget = (ModifyCulture && TargetCulture != Culture.Empty) ||
                      (ModifyReligion && TargetReligion != Religion.Empty) ||
                      (ModifyPopType && TargetPopType != PopType.Empty);

      if (!hasTarget)
      {
         MBox.Show("No target identity specified. Please set at least one target before applying.",
                   "Invalid Configuration",
                   MBoxButton.OK,
                   MessageBoxImage.Warning);
         return;
      }

      if (UseTotalRatioMode && (TargetCulture == Culture.Empty || TargetReligion == Religion.Empty || TargetPopType == PopType.Empty))
      {
         MBox.Show("In Total Ratio Mode, you must specify a Target Culture, Religion, and PopType. Please set all targets before applying.",
                   "Invalid Configuration",
                   MBoxButton.OK,
                   MessageBoxImage.Warning);
         return;
      }

      foreach (var loc in locations)
      {
         var eligiblePops = GetEligiblePops(loc);
         if (eligiblePops.Length == 0)
            continue;

         double pooledAmount = 0;

         // Path A: Total Ratio Mode (Pooling everyone into one target identity)
         if (UseTotalRatioMode)
         {
            // 1. Calculate how much we are taking from everyone
            foreach (var pop in eligiblePops)
            {
               var amountToTake = Math.Round(pop.Size * ratio, DECIMALS);
               if (amountToTake < POP_PRECISION_EPSILON)
                  continue;

               pooledAmount += amountToTake;

               // Reduce the source pop
               var remaining = Math.Round(pop.Size - amountToTake, DECIMALS);
               if (remaining < POP_PRECISION_EPSILON)
                  Nx.RemoveFromCollection(loc, Location.Field.Pops, pop);
               else
                  Nx.Set(pop, PopDefinition.Field.Size, remaining);
            }

            if (pooledAmount < POP_PRECISION_EPSILON)
               continue;

            // 2. Define the Target Identity (In pooling mode, we use UI targets)
            // If UI target is empty, we have to fallback to a sensible default or the first pop's traits
            var finalC = TargetCulture != Culture.Empty ? TargetCulture : eligiblePops[0].Culture;
            var finalR = TargetReligion != Religion.Empty ? TargetReligion : eligiblePops[0].Religion;
            var finalT = TargetPopType != PopType.Empty ? TargetPopType : eligiblePops[0].PopType;

            // 3. Create or Merge the pooled result
            MergeOrCreate(loc, pooledAmount, finalC, finalR, finalT);
         }
         // Path B: Individual Mode (Original Logic)
         else
         {
            foreach (var pop in eligiblePops)
            {
               var amountToMove = Math.Round(pop.Size * ratio, DECIMALS);
               if (amountToMove < POP_PRECISION_EPSILON)
                  continue;

               var targetC = ModifyCulture && TargetCulture != Culture.Empty ? TargetCulture : pop.Culture;
               var targetR = ModifyReligion && TargetReligion != Religion.Empty ? TargetReligion : pop.Religion;
               var targetT = ModifyPopType && TargetPopType != PopType.Empty ? TargetPopType : pop.PopType;

               MergeOrCreate(loc, amountToMove, targetC, targetR, targetT, pop);

               var remaining = Math.Round(pop.Size - amountToMove, DECIMALS);
               if (remaining < POP_PRECISION_EPSILON)
                  Nx.RemoveFromCollection(loc, Location.Field.Pops, pop);
               else
                  Nx.Set(pop, PopDefinition.Field.Size, remaining);
            }
         }
      }
   }

   private static void MergeOrCreate(Location loc, double size, Culture c, Religion r, PopType t, PopDefinition? original = null)
   {
      var match = loc.Pops.FirstOrDefault(lp =>
                                             lp != original &&
                                             lp.Culture == c &&
                                             lp.Religion == r &&
                                             lp.PopType == t);

      if (match != null)
         Nx.Set(match, PopDefinition.Field.Size, Math.Round(match.Size + size, DECIMALS));
      else
      {
         PopDefinition newPop;
         if (loc.Pops.Count > 0)
            newPop = (PopDefinition)(original?.DeepClone() ?? loc.Pops[0].DeepClone());
         else
            newPop = new() { UniqueId = loc.UniqueId };
         Nx.Set(newPop, PopDefinition.Field.Size, size);
         Nx.Set(newPop, PopDefinition.Field.Culture, c);
         Nx.Set(newPop, PopDefinition.Field.Religion, r);
         Nx.Set(newPop, PopDefinition.Field.PopType, t);
         Nx.AddToCollection(loc, Location.Field.Pops, newPop);
      }
   }

   private PopDefinition[] GetEligiblePops(Location loc)
   {
      return loc.Pops.Where(pop =>
                 {
                    var cMatch = !ModifyCulture || SourceCulture == Culture.Empty || pop.Culture == SourceCulture;
                    var rMatch = !ModifyReligion || SourceReligion == Religion.Empty || pop.Religion == SourceReligion;
                    var tMatch = !ModifyPopType || SourcePopType == PopType.Empty || pop.PopType == SourcePopType;
                    return cMatch && rMatch && tMatch;
                 })
                .ToArray();
   }

   public void OnRequestRefreshPreview()
   {
      RefreshPreview(Selection.GetSelectedLocations.ToArray());
   }

   private void RefreshPreview(Location[] locations)
   {
      var totalPops = 0;
      var totalLocs = 0;

      foreach (var loc in locations)
      {
         var matches = GetEligiblePops(loc);

         if (matches.Any())
         {
            totalPops += matches.Length;
            totalLocs++;
         }
      }

      AffectedPopsCount = totalPops;
      AffectedLocationsCount = totalLocs;
      OnPropertyChanged(nameof(AffectedPopsCount));
      OnPropertyChanged(nameof(AffectedLocationsCount));
   }

   public void ResetFor(Location[] selectedLocations)
   {
      HashSet<Culture> cultures = [];
      HashSet<Religion> religions = [];
      HashSet<PopType> popTypes = [];

      foreach (var location in selectedLocations)
      {
         foreach (var pop in location.Pops)
         {
            cultures.Add(pop.Culture);
            religions.Add(pop.Religion);
            popTypes.Add(pop.PopType);
         }
      }

      AvailableCultures.ClearAndAdd(cultures);
      AvailableReligions.ClearAndAdd(religions);
      AvailablePopTypes.ClearAndAdd(popTypes);

      RefreshPreview(selectedLocations);
      LoadState();
      UIResetRequested?.Invoke();
   }

   public void SaveState()
   {
      _lastState = new PopPainterState(ModifyCulture,
                                       SourceCulture,
                                       TargetCulture,
                                       ModifyReligion,
                                       SourceReligion,
                                       TargetReligion,
                                       ModifyPopType,
                                       SourcePopType,
                                       TargetPopType,
                                       GlobalTransferPercent,
                                       UseTotalRatioMode);
   }

   public void LoadState()
   {
      if (_lastState == null)
         return;

      var state = _lastState.Value;

      ModifyCulture = state.ModifyCulture;
      SourceCulture = state.SourceCulture;
      TargetCulture = state.TargetCulture;

      ModifyReligion = state.ModifyReligion;
      SourceReligion = state.SourceReligion;
      TargetReligion = state.TargetReligion;

      ModifyPopType = state.ModifyPopType;
      SourcePopType = state.SourcePopType;
      TargetPopType = state.TargetPopType;

      GlobalTransferPercent = state.GlobalTransferPercent;

      UseTotalRatioMode = state.UseTotalRatioMode;
   }

   private record struct PopPainterState(
      bool ModifyCulture,
      Culture SourceCulture,
      Culture TargetCulture,
      bool ModifyReligion,
      Religion SourceReligion,
      Religion TargetReligion,
      bool ModifyPopType,
      PopType SourcePopType,
      PopType TargetPopType,
      double GlobalTransferPercent,
      bool UseTotalRatioMode);

   #region Culture Properties

   public Culture SourceCulture
   {
      get;
      set => SetField(ref field, value);
   } = Culture.Empty;

   public Culture TargetCulture
   {
      get;
      set => SetField(ref field, value);
   } = Culture.Empty;

   public double CultureTransferPercent
   {
      get;
      set => SetField(ref field, value);
   }

   public ObservableRangeCollection<Culture> AvailableCultures { get; } = [];

   #endregion

   #region Religion Properties

   public Religion SourceReligion
   {
      get;
      set => SetField(ref field, value);
   } = Religion.Empty;

   public Religion TargetReligion
   {
      get;
      set => SetField(ref field, value);
   } = Religion.Empty;

   public double ReligionTransferPercent
   {
      get;
      set => SetField(ref field, value);
   }

   public ObservableRangeCollection<Religion> AvailableReligions { get; } = [];

   #endregion

   #region PopType Properties

   public PopType SourcePopType
   {
      get;
      set => SetField(ref field, value);
   } = PopType.Empty;

   public PopType TargetPopType
   {
      get;
      set => SetField(ref field, value);
   } = PopType.Empty;

   public double TypeTransferPercent
   {
      get;
      set => SetField(ref field, value);
   }

   public ObservableRangeCollection<PopType> AvailablePopTypes { get; } = [];

   #endregion
}