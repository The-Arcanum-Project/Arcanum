// Filename: MultiSelectPropertyViewModel.cs

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Arcanum.Core.CoreSystems.CommandSystem;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.BaseTypes;
using Nexus.Core;

namespace Arcanum.UI.NUI.Generator;

/// <summary>
/// A ViewModel that represents a single property across multiple selected INUI objects.
/// It handles checking for common values and applying changes to all objects.
/// This implementation uses the Weak Event Pattern to automatically unsubscribe from
/// model events, preventing memory leaks without needing to be manually disposed.
/// </summary>
public sealed class MultiSelectPropertyViewModel : INotifyPropertyChanged
{
   private readonly IReadOnlyList<INUI> _targets;
   private readonly Enum _property;
   private readonly bool _allowReadonlyWrite;
   private readonly object? _defaultValue;

   private INotifyCollectionChanged? _observableCollection;
   public event EventHandler? CollectionContentChanged;

   public bool IsNonDefaultValue { get; private set; }
   public int CollectionCount { get; private set; }
   public IEu5Object[] Targets => _targets.OfType<IEu5Object>().ToArray();
   public IEu5Object[] TargetPropObjects
   {
      get
      {
         if (_targets.Count == 0)
            return [];

         return _targets[0].GetNxPropType(_property).IsAssignableTo(typeof(IEu5Object))
                   ? _targets.Select(t => (IEu5Object)t._getValue(_property)).ToArray()
                   : _targets.OfType<IEu5Object>().ToArray();
      }
   }

   public MultiSelectPropertyViewModel(IReadOnlyList<INUI> targets, Enum property, bool allowReadonlyWrite = false)
   {
      _targets = targets;
      _property = property;
      _allowReadonlyWrite = allowReadonlyWrite;

      if (_targets.Count > 0)
         _defaultValue = _targets[0].GetDefaultValue(_property);

      // Subscribe to the PropertyChanged event of each target using a weak listener.
      foreach (var target in _targets)
         if (target is INotifyPropertyChanged inpc)
         {
            // The WeakEventListener ensures that the 'inpc' (model) object does not
            // hold a strong reference back to 'this' (the ViewModel).
            var listener = new WeakEventListener<MultiSelectPropertyViewModel, object, PropertyChangedEventArgs>(this,
                (instance, _, e) => instance.OnModelPropertyChanged(e),
                weakListener => inpc.PropertyChanged -= weakListener.OnEvent);

            inpc.PropertyChanged += listener.OnEvent;
         }

      if (Value is INotifyCollectionChanged newCollection)
      {
         _observableCollection = newCollection;
         _observableCollection.CollectionChanged += OnModelCollectionChanged;
      }

      UpdateIsNonDefaultState();
      UpdateCollectionCount();
   }

   private void OnModelCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
   {
      UpdateIsNonDefaultState();
      UpdateCollectionCount();
      CollectionContentChanged?.Invoke(this, EventArgs.Empty);
   }

   /// <summary>
   /// This method is called weakly from the models.
   /// </summary>
   private void OnModelPropertyChanged(PropertyChangedEventArgs e)
   {
      if (e.PropertyName != _property.ToString())
         return;

      if (_observableCollection != null)
         _observableCollection.CollectionChanged -= OnModelCollectionChanged;

      if (Value is INotifyCollectionChanged newCollection)
      {
         _observableCollection = newCollection;
         _observableCollection.CollectionChanged += OnModelCollectionChanged;
      }

      Refresh();
   }

   public void Refresh()
   {
      OnPropertyChanged(nameof(Value));
      UpdateIsNonDefaultState();
      UpdateCollectionCount();
      CollectionContentChanged?.Invoke(this, EventArgs.Empty);
   }

   private void UpdateCollectionCount()
   {
      var newCount = 0;
      if (Value is ICollection collection)
         newCount = collection.Count;

      if (CollectionCount == newCount)
         return;

      CollectionCount = newCount;
      OnPropertyChanged(nameof(CollectionCount));
   }

   private void UpdateIsNonDefaultState()
   {
      if (_targets.Count == 0)
      {
         SetIsNonDefaultValue(false);
         return;
      }

      // The logic: if ANY target's value is not the default, the marker is "on".
      // This correctly handles the multi-select case where values might be mixed.
      var anyNonDefault = false;
      foreach (var target in _targets)
         if (!AreValuesEqual(GetValue(target), _defaultValue))
         {
            anyNonDefault = true;
            break; //
         }

      SetIsNonDefaultValue(anyNonDefault);
   }

   private void SetIsNonDefaultValue(bool newValue)
   {
      if (IsNonDefaultValue == newValue)
         return;

      IsNonDefaultValue = newValue;
      OnPropertyChanged(nameof(IsNonDefaultValue));
   }

   /// <summary>
   /// Gets or sets the value for the property.
   /// The GETTER returns the common value, or null if values are mixed.
   /// The SETTER applies the new value to ALL target objects.
   /// </summary>
   public object? Value
   {
      get
      {
         if (_targets.Count == 0)
            return null;

         var firstValue = GetValue(_targets[0]);

         for (var i = 1; i < _targets.Count; i++)
         {
            var otherValue = GetValue(_targets[i]);
            if (!AreValuesEqual(firstValue, otherValue))
               // Values are mixed, return an "indeterminate" state.
               // For CheckBoxes, this is null. For other controls, it might be an empty string or null.
               return null;
         }

         return firstValue;
      }
      set
      {
         if (_targets.Count == 0 || (_targets[0].IsPropertyReadOnly(_property) && !_allowReadonlyWrite))
            return;

         // Apply the new value to every selected object.
         foreach (var target in _targets)
            Nx.ForceSet(value, target, _property);
         OnPropertyChanged();
      }
   }

   private object GetValue(INUI target)
   {
      object val = null!;
      Nx.ForceGet(target, _property, ref val);
      return val;
   }

   /// <summary>
   /// A helper to correctly compare values, including collections which require sequence equality.
   /// </summary>
   private bool AreValuesEqual(object? a, object? b)
   {
      if (a is ICollection ^ b is ICollection)
         return false;

      // If both are collections, compare their contents in order.
      if (a is ICollection firstColl && b is ICollection secondColl)
         return firstColl.Cast<object>().SequenceEqual(secondColl.Cast<object>());

      return Equals(a, b);
   }

   public event PropertyChangedEventHandler? PropertyChanged;

   private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
      PropertyChanged?.Invoke(this, new(propertyName));
   }
}