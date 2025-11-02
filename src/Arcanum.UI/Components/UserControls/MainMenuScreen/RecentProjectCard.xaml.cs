using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Mod;
using Arcanum.UI.Components.Views.MainMenuScreen;
using CommunityToolkit.Mvvm.Input;

namespace Arcanum.UI.Components.UserControls.MainMenuScreen;

public partial class RecentProjectCard
{
   private readonly MainMenuViewModel _mainMenuViewModel;
   public readonly ProjectFileDescriptor Descriptor;

   // Command to launch Arcanum with the selected project
   public ICommand LaunchArcanumCommand => new RelayCommand(LaunchArcanum);

   // Command to load the project in the Arcanum view
   public ICommand LoadProjectCommand => new RelayCommand(() =>
   {
      _mainMenuViewModel.ArcanumVm.DescriptorToUi(Descriptor);
   });

   public RecentProjectCard(ProjectFileDescriptor descriptor, MainMenuViewModel mainMenuViewModel)
   {
      Descriptor = descriptor;
      _mainMenuViewModel = mainMenuViewModel;
      InitializeComponent();

      ModNameTextBlock.Text = descriptor.ModName;
      ModThumbnailImage.Source = descriptor.ModThumbnailOrDefault();
   }

   private void LaunchArcanum()
   {
      _ = _mainMenuViewModel.LaunchArcanum(Descriptor);
   }

   private void MenuItem_OnClick(object sender, RoutedEventArgs e)
   {
      if (sender is not MenuItem)
         return;

      _mainMenuViewModel.ArcanumVm.RemoveRecentProject(Descriptor);
   }
}