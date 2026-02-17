using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Arcanum.UI.Commands;

namespace Arcanum.UI.Components.Views.Settings;

public class ShortcutTreeItem : INotifyPropertyChanged
{
   public string Name { get; set; } = string.Empty;
   public IAppCommand? Command { get; set; }
   public ObservableCollection<ShortcutTreeItem> Children { get; } = new();
   public bool IsExpanded
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   } = true;
   public bool IsVisible
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   } = true;

   public bool IsCommand => Command != null;
   public ObservableCollection<InputGesture>? Gestures => Command?.Gestures;

   public event PropertyChangedEventHandler? PropertyChanged;
   protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}