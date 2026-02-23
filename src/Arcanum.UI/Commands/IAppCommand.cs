using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Arcanum.UI.Commands;

public interface IAppCommand : ICommand, INotifyPropertyChanged
{
   public CommandId Id { get; }
   public string DisplayName { get; }
   public string Description { get; }
   public ObservableCollection<InputGesture> Gestures { get; }
   public ReadOnlyObservableCollection<InputGesture> DefaultGestures { get; }
   public string Tooltip { get; }
   public string Scope { get; }
   public void ResetToDefault();
   public bool HasUserDefinedGestures
   {
      get { return !(Gestures.Count == DefaultGestures.Count && Gestures.SequenceEqual(DefaultGestures, new KeyGestureComparer())); }
   }

   public bool IsHiddenInPalette { get; }
   public Predicate<object?>? PaletteVisibilityPredicate { get; set; }
   public bool ShouldShowInPalette(object? context) => !IsHiddenInPalette && (PaletteVisibilityPredicate?.Invoke(context) ?? true);
}