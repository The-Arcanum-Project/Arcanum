using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Arcanum.UI.Components.Windows.MinorWindows.PopUpEditors;

public abstract class ViewModelBase : INotifyPropertyChanged
{
   public event PropertyChangedEventHandler? PropertyChanged;

   protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
      PropertyChanged?.Invoke(this, new(propertyName));
   }

   protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
   {
      if (Equals(storage, value))
         return false;

      storage = value;
      OnPropertyChanged(propertyName);
      return true;
   }
}