using System.Windows;
using System.Windows.Input;
using Arcanum.UI.Commands;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Arcanum.UI.Components.Windows.MinorWindows.ContextExplorer;

public partial class ContextExplorerWindow
{
   private readonly CommandVisualTracker _tracker = new();

   public ContextExplorerWindow(Window owner)
   {
      Owner = Application.Current.MainWindow;
      var vm = new ContextExplorerViewModel();
      DataContext = vm;
      InitializeComponent();
      vm.Initialize(owner);
      Owner = owner;

      Loaded += (_, _) => _tracker.Scan(owner);
      Closed += (_, _) => _tracker.Clear();

      PartSearchBox.Focus();
   }

   private void CommandItem_MouseEnter(object sender, MouseEventArgs mouseEventArgs)
   {
      if (sender is FrameworkElement { DataContext: IAppCommand cmd })
         _tracker.ShowHighlights(cmd);
   }

   private void CommandItem_MouseLeave(object sender, MouseEventArgs mouseEventArgs)
   {
      _tracker.Clear();
   }

   private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
   {
      if (e.ClickCount != 2)
         return;

      if (sender is FrameworkElement { DataContext: IAppCommand cmd } && cmd.CanExecute(null))
      {
         cmd.Execute(null);
         Close();
      }

      e.Handled = true;
   }
}