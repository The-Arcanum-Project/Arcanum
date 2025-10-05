// Filename: MultiSelectPropertyViewModel.cs

using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Arcanum.Core.CoreSystems.NUI;
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

   public MultiSelectPropertyViewModel(IReadOnlyList<INUI> targets, Enum property, bool allowReadonlyWrite = false)
   {
      _targets = targets;
      _property = property;
      _allowReadonlyWrite = allowReadonlyWrite;

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
   }

   /// <summary>
   /// This method is called weakly from the models.
   /// </summary>
   private void OnModelPropertyChanged(PropertyChangedEventArgs e)
   {
      if (e.PropertyName == _property.ToString())
         OnPropertyChanged(nameof(Value));
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