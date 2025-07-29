using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Arcanum;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Mod;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.Views.MainMenuScreen;
using Arcanum.UI.Components.WindowLinker;
using ArcanumViewModel = Arcanum.UI.Components.Views.MainMenuScreen.ArcanumViewModel;

namespace Arcanum.UI.Components.Windows.MainWindows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainMenuScreen
{
#if DEBUG
   public static bool IsFirstLaunch = true;
#endif
   public enum MainMenuScreenView
   {
      Home = 0,
      Arcanum = 1,
      Feature = 0, // Assuming Feature is the same as Home for now as we have disabled that button
      Modforge = 2,
      AboutUs = 3,
      Attributions = 4,
   }

   public readonly MainMenuViewModel MainMenuViewModel;

   public MainMenuScreen()
   {
      InitializeComponent();
      MainMenuViewModel = new() { MainMenuStackPanel = MenuBarStackPanel, MenuWindow = this };
      DataContext = MainMenuViewModel;

      Closed += OnClosed;

      MainMenuViewModel.PropertyChanged += (_, args) =>
      {
         if (args.PropertyName == nameof(Views.MainMenuScreen.MainMenuViewModel.IsWindowVisible))
            Visibility = MainMenuViewModel.IsWindowVisible;
      };

      Debug.Assert(MainMenuViewModel != null, "MainMenuViewModel should not be null");

#if DEBUG
      Loaded += (_, _) =>
      {
         if (DebugConfig.Settings.SkipMainMenu && IsFirstLaunch)
         {
            LoadLastConfigButton_Click(null!, null!);
            IsFirstLaunch = false;
         }
      };
#endif

      AppData.WindowLinker = new WindowLinkerImpl();
   }

   private void OnClosed(object? sender, EventArgs? e)
   {
      MainMenuScreenDescriptor.SaveData();
   }

   private void CloseButton_Click(object sender, RoutedEventArgs e)
   {
      Close();
   }

   private void CreateNewProjectButton_Click(object sender, RoutedEventArgs e)
   {
      ArcanumTabButton.IsChecked = true;
      MainMenuViewModel.ArcanumVc.Execute(null);
      MainMenuViewModel.ArcanumVm.ClearUi();
   }

   private async void LoadLastConfigButton_Click(object sender, RoutedEventArgs e)
   {
      ProjectFileDescriptor? descriptor;
      // If we are not in the arcanum view model we launch the last project if there is one
      if (MainMenuViewModel.CurrentView is not ArcanumViewModel)
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
      else if (!MainMenuViewModel.GetDescriptorFromInput(out descriptor))
      {
         MessageBox.Show("Could not create a valid 'ProjectDescriptor'.\n" +
                         "Please make sure to have valid paths for the mod- and the vanilla folder.\n\n " +
                         "If you are using base mods make sure that they are valid, too.",
                         "Invalid Project Data",
                         MessageBoxButton.OK,
                         MessageBoxImage.Error);
         return;
      }

      await MainMenuViewModel.LaunchArcanum(descriptor);
   }

   private void LoadLastConfigButton_MouseEnter(object sender, MouseEventArgs e)
   {
      // Set the tooltip to the profile which will be loaded
      if (MainMenuViewModel.CurrentView is not ArcanumViewModel)
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
      MainMenuViewModel.SetCurrentView(MainMenuViewModel.TargetedView);
   }

   public async void LoadAndTransfer()
   {
      try
      {
         var loadingScreen = new LoadingScreen();
         await loadingScreen.ShowLoadingAsync();
         var mw = new MainWindow();
         Application.Current.MainWindow = mw;
         Application.Current.MainWindow.Show();
         mw.Activate();
         Close();
      }
      catch (Exception)
      {
         MessageBox.Show("An error occurred while loading the main window. Please try again.",
                         "Error",
                         MessageBoxButton.OK,
                         MessageBoxImage.Error);
      }
   }
}