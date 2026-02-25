using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Arcanum.UI.Components.Windows.HelpWindow.ViewModels;

public abstract class HelpPageViewModelBase : INotifyPropertyChanged
{
   public abstract string Title { get; }
   public event PropertyChangedEventHandler? PropertyChanged;
   protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}

// Sidebar item definition
public record NavMenuItem(string Name, string IconKey, HelpPageViewModelBase ViewModel);