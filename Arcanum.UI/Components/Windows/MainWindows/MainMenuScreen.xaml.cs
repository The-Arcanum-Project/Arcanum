using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Mod;
using Arcanum.Core.Globals;
using Arcanum.UI.Components.MVVM.Views.MainMenuScreen;
using ArcanumViewModel = Arcanum.UI.Components.MVVM.Views.MainMenuScreen.ArcanumViewModel;

namespace Arcanum.UI.Components.Windows.MainWindows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainMenuScreen
{
   public enum MainMenuScreenView
   {
      Home = 0,
      Arcanum = 1,
      Feature = 2,
      Modforge = 3,
      AboutUs = 4,
      Attributions = 5,
   }

   public readonly MainViewModel MainViewModel;

   public MainMenuScreen()
   {
      InitializeComponent();
      MainViewModel = new() { MainMenuStackPanel = MenuBarStackPanel };
      DataContext = MainViewModel;

      Closed += OnClosed;

      MainViewModel.PropertyChanged += (_, args) =>
      {
         if (args.PropertyName == nameof(MVVM.Views.MainMenuScreen.MainViewModel.IsWindowVisible))
            Visibility = MainViewModel.IsWindowVisible;
      };

      Debug.Assert(MainViewModel != null, "MainViewModel should not be null");

#if DEBUG
      Loaded += (_, _) =>
      {

         if (DebugConfig.Settings.SkipMainMenu)
            LoadLastConfigButton_Click(null!, null!);
      };
#endif
   }

   private void OnClosed(object? sender, EventArgs? e)
   {
      Application.Current.Shutdown();
   }

   private void CloseButton_Click(object sender, RoutedEventArgs e)
   {
      Close();
   }

   private void CreateNewProjectButton_Click(object sender, RoutedEventArgs e)
   {
      ArcanumTabButton.IsChecked = true;
      MainViewModel.ArcanumVc.Execute(null);
      MainViewModel.ArcanumVm.ClearUi();
   }

   private async void LoadLastConfigButton_Click(object sender, RoutedEventArgs e)
   {
      ProjectFileDescriptor? descriptor;
      // If we are not in the arcanum view model we launch the last project if there is one
      if (MainViewModel.CurrentView is not ArcanumViewModel)
      {
         descriptor = AppData.MainMenuScreenDescriptor.GetLastDescriptor();
         if (descriptor is null)
         {
            MessageBox.Show("No last project found. Please create a new project or load an existing one.",
                            "Could not find last Project",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
            return;
         }
      }
      else if (!MainViewModel.GetDescriptorFromInput(out descriptor))
      {
         MessageBox.Show("Could not create a valid 'ProjectDescriptor'.\n" +
                         "Please make sure to have valid paths for the mod- and the vanilla folder.\n\n " +
                         "If you are using base mods make sure that they are valid, too.",
                         "Invalid Project Data",
                         MessageBoxButton.OK,
                         MessageBoxImage.Error);
         return;
      }

      await MainViewModel.LaunchArcanum(descriptor);
   }

   private void LoadLastConfigButton_MouseEnter(object sender, MouseEventArgs e)
   {
      // Set the tooltip to the profile which will be loaded
      if (MainViewModel.CurrentView is not ArcanumViewModel)
      {
         var descriptor = AppData.MainMenuScreenDescriptor.GetLastDescriptor();
         if (descriptor is not null)
         {
            var ttString = $"Load last project: {descriptor.ModName}";
            if (descriptor.IsSubMod)
               ttString += $" (SubMod of: {string.Join(", ", descriptor.RequiredMods)})";
            LoadLastConfigButton.ToolTip = ttString;
         }
         else
            LoadLastConfigButton.ToolTip = "No last project found";
      }
      else
      {
         LoadLastConfigButton.ToolTip = "Load the current project configuration";
      }
   }

   private void MainMenuScreen_OnActivated(object? sender, EventArgs e)
   {
      MainViewModel.SetCurrentView(MainViewModel.TargetedView);
   }
}