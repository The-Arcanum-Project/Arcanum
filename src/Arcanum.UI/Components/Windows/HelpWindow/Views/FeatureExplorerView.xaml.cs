using System.Windows;
using System.Windows.Input;
using Arcanum.UI.Commands;
using Arcanum.UI.Components.Windows.HelpWindow.ViewModels;

namespace Arcanum.UI.Components.Windows.HelpWindow.Views;

public partial class FeatureExplorerView
{
   private readonly FeatureExplorerViewModel _viewModel = new();

   public FeatureExplorerView()
   {
      DataContext = _viewModel;
      InitializeComponent();

      _viewModel.SelectedItem = _viewModel.FeatureTree[0];
   }

   private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
   {
      if (e.ClickCount != 2)
         return;

      if (sender is FrameworkElement { DataContext: IAppCommand cmd } && cmd.CanExecute(null))
      {
         cmd.Execute(null);
         Window.GetWindow(this)?.Close();
      }

      e.Handled = true;
   }
}