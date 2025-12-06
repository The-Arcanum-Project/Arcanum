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
   private bool _isLogarithmic;
   private bool? _areAllLocked;

   private readonly Stack<List<AllocationMemento>> _undoStack = new();
   private readonly Stack<int> _totalHistory = new();

   public ICommand UndoCommand { get; }

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
            // If it becomes Null (indeterminate), we usually treat that as a transition to Unlocked or ignore
            if (value.HasValue)
            {
               SetAllLocks(value.Value);
            }
         }
      }
   }

   public int TotalLimit
   {
      get => _totalLimit;
      set
      {
         // Prevent Total from going below the sum of Locked items
         int minRequired = Items.Where(i => i.IsLocked).Sum(i => i.Value);
         if (value < minRequired)
            value = minRequired;

         if (_totalLimit != value)
         {
            int oldTotal = _totalLimit;
            _totalLimit = value;
            OnPropertyChanged();
            // When Total changes, we must resize unlocked items to fit
            ResizeUnlockedItems(value - oldTotal);
         }
      }
   }

   public bool IsLogarithmic
   {
      get => _isLogarithmic;
      set
      {
         _isLogarithmic = value;
         OnPropertyChanged();
         foreach (var item in Items)
            item.RefreshSlider();
      }
   }

   public ObservableCollection<AllocationItem> Items { get; } = new ObservableCollection<AllocationItem>();

   public AllocatorViewModel(int total)
   {
      _totalLimit = total;

      // Listen for new items to hook up events
      Items.CollectionChanged += Items_CollectionChanged;

      // Initial state check
      UpdateMasterLockState();
      UndoCommand = new RelayCommand(Undo);
   }

   private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
   {
      if (e.NewItems != null)
      {
         foreach (AllocationItem item in e.NewItems)
            item.PropertyChanged += Item_PropertyChanged;
      }

      if (e.OldItems != null)
      {
         foreach (AllocationItem item in e.OldItems)
            item.PropertyChanged -= Item_PropertyChanged;
      }

      UpdateMasterLockState();
   }

   public void SnapshotState()
   {
      // Capture current values of all items
      var snapshot = Items.Select(x => new AllocationMemento(x)).ToList();
      _undoStack.Push(snapshot);
      _totalHistory.Push(TotalLimit);

      // Optional: Limit stack size
      if (_undoStack.Count > 50)
      {
         var temp = _undoStack.ToList();
         temp.RemoveAt(temp.Count - 1); // Remove oldest
         // ... (requires rebuilding stack, inefficient but safe for simple lists)
      }
   }

   private void Undo()
   {
      if (_undoStack.Count == 0)
         return;

      var oldState = _undoStack.Pop();
      var oldTotal = _totalHistory.Pop();

      // Restore Total first (quietly)
      _totalLimit = oldTotal;
      OnPropertyChanged(nameof(TotalLimit));

      // Restore Items
      foreach (var memento in oldState)
      {
         memento.Restore();
      }

      // Refresh UI
      foreach (var item in Items)
         item.RefreshSlider();
      UpdateMasterLockState();
   }

   private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
   {
      // If an item locks/unlocks, check if we need to update the Master Toggle
      if (e.PropertyName == nameof(AllocationItem.IsLocked))
      {
         UpdateMasterLockState();
      }
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
         bool allLocked = Items.All(x => x.IsLocked);
         bool allUnlocked = Items.All(x => !x.IsLocked);

         if (allLocked)
            _areAllLocked = true;
         else if (allUnlocked)
            _areAllLocked = false;
         else
            _areAllLocked = null; // Mixed state
      }

      OnPropertyChanged(nameof(AreAllLocked));
   }

   private void SetAllLocks(bool isLocked)
   {
      _isUpdatingLocks = true;
      foreach (var item in Items)
      {
         item.IsLocked = isLocked;
      }

      _isUpdatingLocks = false;

      // Refresh visuals just in case
      UpdateMasterLockState();
   }

   public void AddItem(string name, int initialValue, Color color)
   {
      var item = new AllocationItem(this, name, initialValue, color);
      Items.Add(item);

      // Initial balance check
      if (Items.Sum(x => x.Value) != TotalLimit)
         // Force balance to total (ignoring locks for initial setup)
         BalanceToTotal(null, ignoreLocks: true);
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
         // If everything is locked, we can't really resize the Total cleanly 
         // without violating locks. In this implementation, we force the 
         // last item to take the hit/gain to maintain mathematical validity.
         if (Items.Count > 0)
         {
            var last = Items.Last();
            last.SetValueInternal(last.Value + delta);
         }

         return;
      }

      // Reuse the Distribute logic, treating 'delta' as the error to fix
      DistributeError(delta, unlockedItems);

      // Refresh visuals for everyone (percentages change even if value doesn't)
      foreach (var item in Items)
         item.RefreshSlider();
   }

   /// <summary>
   /// Called when a Slider moves.
   /// </summary>
   public void UpdateItem(AllocationItem source, int newValue)
   {
      // 1. Calculate the Maximum this item is allowed to be.
      // Max = Total - (Sum of ALL other Locked items)
      // It cannot grow so large that it forces a locked item to shrink.
      int lockedSumOther = Items.Where(x => x != source && x.IsLocked).Sum(x => x.Value);
      int maxAllowed = TotalLimit - lockedSumOther;

      if (newValue > maxAllowed)
         newValue = maxAllowed;
      if (newValue < 0)
         newValue = 0;

      // 2. Set Value
      source.SetValueInternal(newValue);

      // 3. Balance others
      BalanceToTotal(source, ignoreLocks: false);
   }

   private void BalanceToTotal(AllocationItem sourceToIgnore, bool ignoreLocks)
   {
      int currentSum = Items.Sum(x => x.Value);
      int error = TotalLimit - currentSum; // + means we need to add, - means subtract

      if (error == 0)
         return;

      // Which items are allowed to move?
      var candidates = Items.Where(x => x != sourceToIgnore).ToList();

      if (!ignoreLocks)
         candidates = candidates.Where(x => !x.IsLocked).ToList();

      // Edge case: If we need to Subtract, we can only take from items > 0
      if (error < 0)
         candidates = candidates.Where(x => x.Value > 0).ToList();

      if (candidates.Count == 0)
      {
         // Deadlock: Source moved, but nothing else can move to compensate.
         // Revert source.
         if (sourceToIgnore != null)
            sourceToIgnore.SetValueInternal(sourceToIgnore.Value + error);
         return;
      }

      DistributeError(error, candidates);
   }

   private void DistributeError(int error, List<AllocationItem> targets)
   {
      int sumTargets = targets.Sum(x => x.Value);
      var changes = targets.ToDictionary(x => x, x => 0);

      // 1. Proportional Distribution
      if (sumTargets > 0)
      {
         foreach (var item in targets)
         {
            double ratio = (double)item.Value / sumTargets;
            int change = (int)Math.Round(error * ratio);
            changes[item] = change;
         }
      }
      else
      {
         // If all targets are 0, split evenly
         int split = error / targets.Count;
         foreach (var item in targets)
            changes[item] = split;
      }

      // Apply provisional changes
      foreach (var kvp in changes)
         kvp.Key.SetValueInternal(kvp.Key.Value + kvp.Value);

      // 2. Fix Off-By-One rounding errors
      int finalError = TotalLimit - Items.Sum(x => x.Value);
      while (finalError != 0)
      {
         // Give/Take from largest target to hide jitter
         var target = targets.OrderByDescending(x => x.Value).FirstOrDefault();

         // Fallback for negative error where largest is 0
         if (target == null || (finalError < 0 && target.Value <= 0))
            target = targets.FirstOrDefault();

         if (target == null)
            break;

         int step = Math.Sign(finalError);
         target.SetValueInternal(target.Value + step);
         finalError -= step;
      }
   }

   public void UpdateLockedItem(AllocationItem source, int newValue)
   {
      // 1. Clamp to 0 (cannot be negative)
      if (newValue < 0)
         newValue = 0;

      // 2. Calculate the difference
      int delta = newValue - source.Value;
      if (delta == 0)
         return;

      // 3. Update the Item directly
      source.SetValueInternal(newValue);

      // 4. Update the Total directly (BYPASSING the Setter logic)
      // We modify the backing field to avoid triggering 'ResizeUnlockedItems'
      _totalLimit += delta;

      // Notify UI that Total changed
      OnPropertyChanged(nameof(TotalLimit));

      // 5. Because Total changed, ALL percentages are now wrong. Refresh everyone.
      foreach (var item in Items)
         item.RefreshSlider();
   }
}