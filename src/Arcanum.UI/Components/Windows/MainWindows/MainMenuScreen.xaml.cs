using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Arcanum;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Mod;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Utils.ScreenManagement;
using Arcanum.UI.Components.Views.MainMenuScreen;
using Arcanum.UI.Components.Windows.MinorWindows;
using Arcanum.UI.Components.Windows.PopUp;
using Common.UI.MBox;
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
      Feature = 2, // Assuming Feature is the same as Home for now as we have disabled that button
      Modforge = 3,
      AboutUs = 4,
      Attributions = 5,
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
      ScreenManager.GetScreenFrom(this);
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
      else
      {
         MainMenuScreenDescriptor.SaveData();
         
         var path = MainMenuViewModel.ArcanumVm.VanillaFolderTextBox.Text;
         if (!path.EndsWith("game", StringComparison.InvariantCultureIgnoreCase)){
            MBox.Show("The selected folder must be the game folder. (./Europa Universalis V/game)", "Invalid folder");
            var combine = Path.Combine(path, "game");
            if (path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).EndsWith("common\\Europa Universalis V", StringComparison.InvariantCultureIgnoreCase) && Path.Exists(combine))
               MainMenuViewModel.ArcanumVm.VanillaFolderTextBox.Text = combine;
            else
               return;
         }
         
         if (!MainMenuViewModel.GetDescriptorFromInput(out descriptor))
         {
            MBox.Show("Could not create a valid 'ProjectDescriptor'.\n" +
                            "Please make sure to have valid paths for the mod- and the vanilla folder.\n\n " +
                            "If you are using base mods make sure that they are valid, too.",
               "Invalid Project Data",
               MBoxButton.OK,
               MessageBoxImage.Error);
            return;
         }
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

   // Create a new method that returns a Task for proper async/await
   public async Task LoadAndTransferAsync()
   {
      var loadingScreen = new LoadingScreen();

      try
      {
         // 1. Hide this window and show the loading screen.
         //    Both Show() and Hide() are non-blocking.
         Hide();
         loadingScreen.Show();

         // 2. Await the loading logic directly.
         //    Because we are awaiting, the UI thread is NOW FREE. It can process
         //    the Dispatcher messages from the loading task and update the text.
         var value = await loadingScreen.StartLoading();
         if (value == false)
         {
            loadingScreen.Close();
            return;
         }

         // 3. Once loading is done, create and show the new MainWindow.
         var mw = new MainWindow();
         Application.Current.MainWindow = mw; // Set the new main window
         mw.Show();

         // 4. Close the loading screen and this initial window.
         loadingScreen.Close();
         Close(); // 'this' refers to the now-hidden LoginWindow
      }
      catch (Exception ex)
      {
         MBox.Show($"An error occurred while loading: {ex.Message}",
                   "Error",
                   MBoxButton.OK,
                   MessageBoxImage.Error);

         // On error, close the loading screen and show this window again
         if (loadingScreen.IsLoaded)
            loadingScreen.Close();
         Show();
      }
   }

   private void SettingsButton_OnClickButton_Click(object sender, RoutedEventArgs e)
   {
      SettingsWindow.ShowSettingsWindow();
   }
}