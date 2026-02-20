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
}