using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using Arcanum.UI.Components.Windows.MinorWindows.PopUpEditors;
using CommunityToolkit.Mvvm.Input;

namespace Arcanum.UI.Components.UserControls.ValueAllocators;

public class AllocatorViewModel : ViewModelBase
{
   private int _totalLimit;
   private bool? _areAllLocked;

   private readonly Stack<List<AllocationMemento>> _undoStack = new();
   private readonly Stack<int> _totalHistory = new();

   public ICommand UndoCommand { get; }

   public bool AutoDetectLogScale
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
         if (value)
            RunAutoLogScale();
      }
   } = true;

   public bool? AreAllLocked
   {
      get => _areAllLocked;
      set
      {
         // Only react if the USER clicked the toggle (value is distinct)
         if (_areAllLocked != value)
         {
            _areAllLocked = value;
            OnPropertyChanged();

            // If user sets to True/False, apply to all items
            // If it becomes Null (indeterminate), we treat that as a transition to Unlocked or ignore
            if (value.HasValue)
               SetAllLocks(value.Value);
         }
      }
   }

   public int TotalLimit
   {
      get => _totalLimit;
      set
      {
         // Prevent Total from going below the sum of Locked items
         var minRequired = Items.Where(i => i.IsLocked).Sum(i => i.Value);
         if (value < minRequired)
            value = minRequired;

         if (_totalLimit != value)
         {
            var oldTotal = _totalLimit;
            _totalLimit = value;

            foreach (var item in Items)
               if (item.MaxLimit == oldTotal)
                  item.MaxLimit = value;

            OnPropertyChanged();
            // When Total changes, we must resize unlocked items to fit
            ResizeUnlockedItems(value - oldTotal);

            RunAutoLogScale();
         }
      }
   }

   public bool IsLogarithmic
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
         foreach (var item in Items)
            item.RefreshSlider();
      }
   }

   public ObservableCollection<AllocationItem> Items { get; } = [];

   public AllocatorViewModel(int total)
   {
      _totalLimit = total;

      Items.CollectionChanged += Items_CollectionChanged;

      UpdateMasterLockState();
      UndoCommand = new RelayCommand(Undo);
   }

   private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
   {
      if (e.NewItems != null)
         foreach (AllocationItem item in e.NewItems)
            item.PropertyChanged += Item_PropertyChanged;

      if (e.OldItems != null)
         foreach (AllocationItem item in e.OldItems)
            item.PropertyChanged -= Item_PropertyChanged;

      UpdateMasterLockState();
   }

   public void SnapshotState()
   {
      var snapshot = Items.Select(x => new AllocationMemento(x)).ToList();
      _undoStack.Push(snapshot);
      _totalHistory.Push(TotalLimit);

      if (_undoStack.Count > 50)
      {
         var temp = _undoStack.ToList();
         temp.RemoveAt(temp.Count - 1);
      }
   }

   private void Undo()
   {
      if (_undoStack.Count == 0)
         return;

      var oldState = _undoStack.Pop();
      var oldTotal = _totalHistory.Pop();

      _totalLimit = oldTotal;
      OnPropertyChanged(nameof(TotalLimit));

      foreach (var memento in oldState)
         memento.Restore();

      foreach (var item in Items)
         item.RefreshSlider();
      UpdateMasterLockState();
   }

   private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
   {
      // If an item locks/unlocks, check if we need to update the Master Toggle
      if (e.PropertyName == nameof(AllocationItem.IsLocked))
         UpdateMasterLockState();
   }

   private bool _isUpdatingLocks = false;

   private void UpdateMasterLockState()
   {
      if (_isUpdatingLocks)
         return; // Prevent loops

      if (Items.Count == 0)
      {
         _areAllLocked = false;
      }
      else
      {
         var allLocked = Items.All(x => x.IsLocked);
         var allUnlocked = Items.All(x => !x.IsLocked);

         if (allLocked)
            _areAllLocked = true;
         else if (allUnlocked)
            _areAllLocked = false;
         else
            _areAllLocked = null;
      }

      OnPropertyChanged(nameof(AreAllLocked));
   }

   private void SetAllLocks(bool isLocked)
   {
      _isUpdatingLocks = true;
      foreach (var item in Items)
         item.IsLocked = isLocked;

      _isUpdatingLocks = false;

      UpdateMasterLockState();
   }

   public void AddItem(string name, int initialValue, Color color, bool balanceToTotal)
   {
      var item = new AllocationItem(this, name, initialValue, color);
      Items.Add(item);

      // Initial balance check
      if (balanceToTotal && Items.Sum(x => x.Value) != TotalLimit)
         // Force balance to total (ignoring locks for initial setup)
         BalanceToTotal(null, ignoreLocks: true);

      RunAutoLogScale();
   }

   private void RunAutoLogScale()
   {
      if (!AutoDetectLogScale)
         return;

      // Simple heuristic: If any item has value > 10x the smallest non-zero item, use log scale
      var nonZeroItems = Items.Where(x => x.Value > 0).ToList();
      if (nonZeroItems.Count < 2)
      {
         IsLogarithmic = false;
         return;
      }

      var minVal = nonZeroItems.Min(x => x.Value);
      var maxVal = nonZeroItems.Max(x => x.Value);

      if (maxVal >= 10 * minVal)
         IsLogarithmic = true;
      else
         IsLogarithmic = false;
   }

   /// <summary>
   /// Called when the User changes the Total TextBox.
   /// We keep Locked items as-is, and scale Unlocked items to fill the gap.
   /// </summary>
   private void ResizeUnlockedItems(int delta)
   {
      // If delta is positive, we add to unlocked. If negative, we subtract.
      var unlockedItems = Items.Where(x => !x.IsLocked).ToList();

      if (unlockedItems.Count == 0)
      {
         if (Items.Count > 0)
         {
            var last = Items.Last();
            last.SetValueInternal(last.Value + delta);
         }

         return;
      }

      // Reuse the Distribute logic, treating 'delta' as the error to fix
      DistributeError(delta, unlockedItems);

      foreach (var item in Items)
         item.RefreshSlider();
   }

   /// <summary>
   /// Called when a Slider moves.
   /// </summary>
   public void UpdateItem(AllocationItem source, int newValue)
   {
      // Calculate max allowed based on Total - Locked items
      var lockedSumOther = Items.Where(x => x != source && x.IsLocked).Sum(x => x.Value);
      var globalMaxAllowed = TotalLimit - lockedSumOther;

      // The stricter limit wins.
      var actualMax = Math.Min(globalMaxAllowed, source.MaxLimit);
      var actualMin = source.MinLimit;

      if (actualMin > actualMax)
         actualMin = actualMax;

      if (newValue > actualMax)
         newValue = actualMax;
      if (newValue < actualMin)
         newValue = actualMin;

      source.SetValueInternal(newValue);

      BalanceToTotal(source, ignoreLocks: false);
   }

   private void BalanceToTotal(AllocationItem? sourceToIgnore, bool ignoreLocks)
   {
      var currentSum = Items.Sum(x => x.Value);
      var error = TotalLimit - currentSum;

      if (error == 0)
         return;

      var candidates = Items.Where(x => x != sourceToIgnore).ToList();

      if (!ignoreLocks)
         candidates = candidates.Where(x => !x.IsLocked).ToList();

      if (error < 0)
         candidates = candidates.Where(x => x.Value > 0).ToList();

      if (candidates.Count == 0)
      {
         sourceToIgnore?.SetValueInternal(sourceToIgnore.Value + error);
         return;
      }

      var remainingError = DistributeError(error, candidates);

      // If there is still error left, it means everyone else hit a wall.
      // The Source item MUST take back the change.
      if (remainingError != 0 && sourceToIgnore != null)
         // e.g. User added +10, but others could only give -8. 
         // We must give back +2 to the others 
         // Logic: Just apply the remaining error to Source.
         sourceToIgnore.SetValueInternal(sourceToIgnore.Value + remainingError);
   }

   private static int DistributeError(int error, List<AllocationItem> targets)
   {
      const int maxLoops = 20;
      var currentLoop = 0;

      while (error != 0 && currentLoop < maxLoops)
      {
         currentLoop++;

         // If Adding (error > 0): Must be below Max
         // If Subtracting (error < 0): Must be above Min
         var validCandidates = targets.Where(t =>
                                                (error > 0 && t.Value < t.MaxLimit) ||
                                                (error < 0 && t.Value > t.MinLimit))
                                      .ToList();

         if (validCandidates.Count == 0)
            break; // Deadlock: Cannot distribute further

         var sumValues = validCandidates.Sum(t => (long)t.Value);

         var useEvenSplit = sumValues == 0;

         var plannedChanges = new Dictionary<AllocationItem, int>();
         var distributedThisPass = 0;

         foreach (var item in validCandidates)
         {
            int share;
            if (useEvenSplit)
               share = error / validCandidates.Count;
            else
               // Proportional: (ItemValue / Sum) * Error
               share = (int)Math.Round(error * (double)item.Value / sumValues);

            plannedChanges[item] = share;
         }

         foreach (var kvp in plannedChanges)
         {
            // If error is resolved by previous iterations in this loop, stop
            if (error == 0)
               break;

            var item = kvp.Key;
            var change = kvp.Value;

            if (change == 0)
               continue;

            // Cap change to the remaining error (fixes rounding overshoots)
            if (Math.Abs(change) > Math.Abs(error))
               change = error;

            var proposed = item.Value + change;

            // Clamp to Item Limits
            if (proposed > item.MaxLimit)
               proposed = item.MaxLimit;
            if (proposed < item.MinLimit)
               proposed = item.MinLimit;

            var actualChange = proposed - item.Value;

            if (actualChange != 0)
            {
               item.SetValueInternal(proposed);
               error -= actualChange;
               distributedThisPass += actualChange;
            }
         }

         // Anti-Stagnation (Fixing the "Off-By-One" rounding errors)
         // If we still have error but the proportional math resulted in 0 changes 
         // (common with small errors like 1 or -1), force a single unit move.
         if (error != 0 && distributedThisPass == 0)
         {
            // Pick the largest candidate to absorb the remainder (least visual impact)
            var target = validCandidates.OrderByDescending(x => x.Value).FirstOrDefault();

            if (target != null)
            {
               var step = Math.Sign(error); // +1 or -1

               var proposed = target.Value + step;
               if (proposed >= target.MinLimit && proposed <= target.MaxLimit)
               {
                  target.SetValueInternal(proposed);
                  error -= step;
               }
               else
               {
                  // If the largest candidate is stuck, this loop is dead. 
                  break;
               }
            }
         }
      }

      return error;
   }

   public void UpdateLockedItem(AllocationItem source, int newValue)
   {
      if (newValue < 0)
         newValue = 0;

      var delta = newValue - source.Value;
      if (delta == 0)
         return;

      source.SetValueInternal(newValue);

      _totalLimit += delta;

      OnPropertyChanged(nameof(TotalLimit));

      foreach (var item in Items)
         item.RefreshSlider();
   }
}