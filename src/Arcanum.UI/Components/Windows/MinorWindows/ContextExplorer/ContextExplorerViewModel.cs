using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Arcanum.UI.Commands;
using Arcanum.UI.Commands.UIContext;

namespace Arcanum.UI.Components.Windows.MinorWindows.ContextExplorer;

public sealed class ContextExplorerViewModel : INotifyPropertyChanged
{
   private readonly List<IAppCommand> _contextCommands = [];
   private const bool IS_DEEP_SEARCH = true;

   public ObservableCollection<CommandGroup> FilteredGroupedcommands { get; } = [];

   public string SearchText
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
         RefreshFilter();
      }
   } = "";

   public event PropertyChangedEventHandler? PropertyChanged;

   public void Initialize(DependencyObject focusedElement)
   {
      List<string> activeScopes;
      if (focusedElement is Window window)
         activeScopes = ContextDiscovery.GetActiveScopes(window, IS_DEEP_SEARCH);
      else
         activeScopes = ContextDiscovery.GetActiveScopes(focusedElement, IS_DEEP_SEARCH);
      var commands = CommandRegistry.AllCommands
                                    .Where(c => activeScopes.Contains(c.Scope, StringComparer.OrdinalIgnoreCase));

      _contextCommands.Clear();
      _contextCommands.AddRange(commands);
      RefreshFilter();
   }

   private void RefreshFilter()
   {
      FilteredGroupedcommands.Clear();
      var filtered = _contextCommands
        .Where(c => string.IsNullOrEmpty(SearchText) ||
                    c.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    c.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

      var groups = filtered.GroupBy(c => c.Scope)
                           .Select(g => new CommandGroup(g.Key, g.ToList()));

      foreach (var g in groups)
         FilteredGroupedcommands.Add(g);
   }

   private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
      PropertyChanged?.Invoke(this, new(propertyName));
   }
}

public record CommandGroup(string ScopeName, List<IAppCommand> Commands);