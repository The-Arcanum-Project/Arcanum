using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Arcanum.Core.CoreSystems.NUI;

/// <summary>
/// Defines a base class for NUI roots that can be used to manage the view context of NUI components.
/// </summary>
public abstract class NUIRoot : INotifyPropertyChanged
{
   private object? _viewContext;
   public object? ViewContext
   {
      get => _viewContext;
      protected set
      {
         if (Equals(value, _viewContext))
            return;

         _viewContext = value;
         OnPropertyChanged();
      }
   }

   /// <summary>
   /// Sets the view for this NUIRoot.
   /// </summary>
   /// <param name="view"></param>
   /// <exception cref="InvalidOperationException"></exception>
   /// <exception cref="ArgumentNullException"></exception>
   public virtual void SetView(NUIUserControl view)
   {
      if (_viewContext == null)
         throw new InvalidOperationException("ViewContext must be set before calling SetView.");

      ViewContext = view ?? throw new ArgumentNullException(nameof(view), "View cannot be null.");
   }

   public event PropertyChangedEventHandler? PropertyChanged;

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