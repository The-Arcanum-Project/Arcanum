using Arcanum.UI.Components.Models;
using Arcanum.UI.Components.Views.LoadingScreen;

namespace Arcanum.UI.Components.Windows.MainWindows;

public partial class LoadingScreen
{
   
   private readonly LoadingScreenView _viewModel;
   public LoadingScreen()
   {
      InitializeComponent();
      var model = new LoadingScreenModel();
      _viewModel = new(model);
      DataContext = _viewModel;
   }
   
   public async Task ShowLoadingAsync()
   {
      Show();
      await _viewModel.StartLoading();
      Close();
   }
}