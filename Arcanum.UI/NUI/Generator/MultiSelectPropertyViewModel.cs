using System.ComponentModel;
using System.Runtime.CompilerServices;
using Arcanum.Core.CoreSystems.NUI;
using Nexus.Core;

namespace Arcanum.UI.NUI.Generator;

/// <summary>
/// A ViewModel that represents a single property across multiple selected INUI objects.
/// It handles checking for common values and applying changes to all objects.
/// </summary>
public class MultiSelectPropertyViewModel(IReadOnlyList<INUI> targets, Enum property) : INotifyPropertyChanged
{
   /// <summary>
   /// Gets or sets the value for the property.
   /// The GETTER returns the common value, or a special value (like null) if values are mixed.
   /// The SETTER applies the new value to ALL target objects.
   /// </summary>
   public object? Value
   {
      get
      {
         var firstValue = GetValue(targets[0]);

         for (var i = 1; i < targets.Count; i++)
         {
            var otherValue = GetValue(targets[i]);
            // Values are mixed, return a "indeterminate" state.
            if (!Equals(firstValue, otherValue))
               return null;
         }

         return firstValue;
      }
      set
      {
         if (targets[0].IsPropertyReadOnly(property))
            return;

         // Apply the new value to every selected object.
         foreach (var target in targets)
            Nx.ForceSet(value, target, property);
         OnPropertyChanged();
      }
   }

   private object GetValue(INUI target)
   {
      object val = null!;
      Nx.ForceGet(target, property, ref val);
      return val;
   }

   public event PropertyChangedEventHandler? PropertyChanged;

   protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
      PropertyChanged?.Invoke(this, new(propertyName));
   }
}