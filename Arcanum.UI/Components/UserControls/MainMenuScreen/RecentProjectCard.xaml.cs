using System.Windows.Input;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Mod;
using Arcanum.UI.Components.Views.MainMenuScreen;
using CommunityToolkit.Mvvm.Input;

namespace Arcanum.UI.Components.UserControls.MainMenuScreen;

public partial class RecentProjectCard
{
   private readonly MainViewModel _mainViewModel;
   private readonly ProjectFileDescriptor _projectFileDescriptor;
   
   // Command to launch Arcanum with the selected project
   public ICommand LaunchArcanumCommand => new RelayCommand(LaunchArcanum);
   
   // Command to load the project in the Arcanum view
   public ICommand LoadProjectCommand => new RelayCommand(() =>
   {
      _mainViewModel.ArcanumVm.DescriptorToUi(_projectFileDescriptor);
   });
   
   public RecentProjectCard(ProjectFileDescriptor projectFileDescriptor, MainViewModel mainViewModel)
   {
      _projectFileDescriptor = projectFileDescriptor;
      _mainViewModel = mainViewModel;
      InitializeComponent();
      
      ModNameTextBlock.Text = projectFileDescriptor.ModName;
      ModThumbnailImage.Source = projectFileDescriptor.ModThumbnailOrDefault();
   }

   private void LaunchArcanum()
   {
      _ = _mainViewModel.LaunchArcanum(_projectFileDescriptor);
   }
}