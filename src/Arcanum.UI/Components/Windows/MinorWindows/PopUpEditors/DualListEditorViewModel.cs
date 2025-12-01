using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.Input;

namespace Arcanum.UI.Components.Windows.MinorWindows.PopUpEditors;

public interface ICollectionEditorViewModel
{
   object GetResult();
}

public enum LastActiveList
{
   Selection,
   Source,
}

public class DualListEditorViewModel<T> : ViewModelBase, ICollectionEditorViewModel where T : notnull
{
   private LastActiveList _lastActiveList = LastActiveList.Source;
   private IList? _lastSelectedAppliedItems;
   private IList? _lastSelectedSourceItems;

   private readonly ObservableCollection<ListItemViewModel<T>> _appliedItemsInternal;
   private readonly ObservableCollection<ListItemViewModel<T>> _availableItemsInternal;
   public ICollectionView AppliedItems { get; }
   public ICollectionView AvailableItems { get; }

   public string AppliedFilterText
   {
      get;
      set
      {
         if (SetProperty(ref field, value))
            AppliedItems.Refresh();
      }
   } = string.Empty;

   public string AvailableFilterText
   {
      get;
      set
      {
         if (SetProperty(ref field, value))
            AvailableItems.Refresh();
      }
   } = string.Empty;

   public bool CanCreatePrimitives { get; }

   public string NewPrimitiveValue
   {
      get;
      set => SetProperty(ref field, value);
   } = string.Empty;

   private readonly HashSet<T> _itemsToAddToAll = [];
   private readonly HashSet<T> _itemsToRemoveFromAll = [];
   public List<ICollection<T>> Collections { get; }

   // --- REVISED COMMANDS ---
   public IRelayCommand RightArrowCommand { get; } // Replaces AddItemCommand
   public IRelayCommand<IList> LeftArrowCommand { get; } // Replaces RemoveItemCommand
   public IRelayCommand<IList> SelectionListActivatedCommand { get; }
   public IRelayCommand<IList> SourceListActivatedCommand { get; }
   public IRelayCommand CreatePrimitiveCommand { get; }

   public DualListEditorViewModel(IEnumerable<ICollection<T>> sourceCollections, IEnumerable<T>? globalItemPool = null)
   {
      Collections = sourceCollections.ToList();
      var sourceObjectCount = Collections.Count;
      var itemCounts = new Dictionary<T, int>();
      foreach (var item in Collections.SelectMany(c => c).Distinct())
         itemCounts[item] = Collections.Count(c => c.Contains(item));
      _appliedItemsInternal = new(itemCounts.Select(kvp => new ListItemViewModel<T>(kvp.Key,
                                                     kvp.Value == sourceObjectCount
                                                        ? EditState.InAll
                                                        : EditState.InSome)));
      var availablePool = (globalItemPool ?? []).Except(itemCounts.Keys).ToList();
      _availableItemsInternal = new(availablePool.Select(item => new ListItemViewModel<T>(item, EditState.NotPresent)));
      AppliedItems = CollectionViewSource.GetDefaultView(_appliedItemsInternal);
      AppliedItems.Filter = o => FilterItem(o, AppliedFilterText);
      AvailableItems = CollectionViewSource.GetDefaultView(_availableItemsInternal);
      AvailableItems.Filter = o => FilterItem(o, AvailableFilterText);
      var typeOfT = typeof(T);
      CanCreatePrimitives = typeOfT == typeof(string) || typeOfT.IsPrimitive;

      LeftArrowCommand = new RelayCommand<IList>(items => RemoveItems(items?.Cast<ListItemViewModel<T>>()));
      RightArrowCommand = new RelayCommand(ExecuteRightArrowAction);

      SelectionListActivatedCommand = new RelayCommand<IList>(items =>
      {
         _lastActiveList = LastActiveList.Selection;
         _lastSelectedAppliedItems = items;
      });
      SourceListActivatedCommand = new RelayCommand<IList>(items =>
      {
         _lastActiveList = LastActiveList.Source;
         _lastSelectedSourceItems = items;
      });

      CreatePrimitiveCommand = new RelayCommand(CreatePrimitive, () => !string.IsNullOrWhiteSpace(NewPrimitiveValue));
   }

   private void ExecuteRightArrowAction()
   {
      if (_lastActiveList == LastActiveList.Source)
         AddItems(_lastSelectedSourceItems?.Cast<ListItemViewModel<T>>());
      else
         PromoteToAll(_lastSelectedAppliedItems?.Cast<ListItemViewModel<T>>());
   }

   private void AddItems(IEnumerable<ListItemViewModel<T>>? items)
   {
      if (items == null)
         return;

      foreach (var item in items.ToList())
      {
         _itemsToRemoveFromAll.Remove(item.Value);
         if (item.InitialState == EditState.NotPresent)
            _itemsToAddToAll.Add(item.Value);

         _availableItemsInternal.Remove(item);
         item.State = EditState.MarkedForAddition;
         _appliedItemsInternal.Add(item);
      }
   }

   private void RemoveItems(IEnumerable<ListItemViewModel<T>>? items)
   {
      if (items == null)
         return;

      foreach (var item in items.ToList())
      {
         _itemsToAddToAll.Remove(item.Value);
         if (item.InitialState is EditState.InAll or EditState.InSome)
            _itemsToRemoveFromAll.Add(item.Value);

         _appliedItemsInternal.Remove(item);
         item.State = EditState.MarkedForRemoval;
         if (item.InitialState == EditState.NotPresent)
            _availableItemsInternal.Add(item);
      }
   }

   private void PromoteToAll(IEnumerable<ListItemViewModel<T>>? items)
   {
      if (items == null)
         return;

      foreach (var item in items)
         if (item.State == EditState.InSome)
         {
            _itemsToRemoveFromAll.Remove(item.Value);
            _itemsToAddToAll.Add(item.Value);
            item.State = EditState.InAll;
         }
   }

   public object GetResult()
   {
      var toAdd = new T[Collections.Count][];
      var toRemove = new T[Collections.Count][];

      for (var i = 0; i < Collections.Count; i++)
      {
         toAdd[i] = _itemsToAddToAll.Except(Collections[i]).ToArray();
         toRemove[i] = _itemsToRemoveFromAll.Intersect(Collections[i]).ToArray();
      }

      return new CollectionEditResult<T>
      {
         Canceled = false,
         ToAddPerCollection = toAdd,
         ToRemovePerCollection = toRemove,
      };
   }

   private static bool FilterItem(object o, string filter)
   {
      if (string.IsNullOrWhiteSpace(filter))
         return true;

      return (o as ListItemViewModel<T>)?.DisplayText.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false;
   }

   private void CreatePrimitive()
   {
      if (TypeDescriptor.GetConverter(typeof(T)) is not { } converter || !converter.IsValid(NewPrimitiveValue))
         return;

      var newItem = (T)converter.ConvertFromString(NewPrimitiveValue)!;
      if (_appliedItemsInternal.Any(i => i.Value.Equals(newItem)) ||
          _availableItemsInternal.Any(i => i.Value.Equals(newItem)))
         return;

      _availableItemsInternal.Add(new(newItem, EditState.NotPresent));
      NewPrimitiveValue = string.Empty;
   }
}