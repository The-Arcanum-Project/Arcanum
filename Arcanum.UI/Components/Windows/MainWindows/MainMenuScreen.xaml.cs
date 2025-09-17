using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Arcanum;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Mod;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.UIHandles;
using Arcanum.UI.Components.Views.MainMenuScreen;
using Arcanum.UI.TutorialSystem.Core;
using Arcanum.UI.TutorialSystem.Data;
using Common.UI;
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
        UIHandle.Instance.UIUtils = new UIUtilsImpl();
        UIHandle.Instance.PopUpHandle = new PopUpHandleImpl();
        UIHandle.Instance.MainWindowsHandle = new MainWindowHandleImpl();
    }

    private void OnClosed(object? sender, EventArgs? e)
    {
        MainMenuScreenDescriptor.SaveData();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        //Close();
        DefineTutorialSequence();
        var tutorialManager = new TutorialManager(this, this.LayoutRoot);
        tutorialManager.Start(_sequence);
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
            MessageBox.Show($"An error occurred while loading: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            // On error, close the loading screen and show this window again
            if (loadingScreen.IsLoaded)
                loadingScreen.Close();
            Show();
        }
    }

    //TODO remove this later:

    private Chapter _sequence;

    private void DefineTutorialSequence()
    {
        _sequence = new StructureChapter(
            "Arcanum Welcome Tour",
            "Welcome to Arcanum, thank you for giving the tool a try. Let's get you up to the basics of the launcher view." +
            "If you want to skip to any chapter just click it in the chapter selection.",[],
            [
                new StructureChapter([
                        new InteractiveChapter("Press this button", "Just do it.", [
                            new ButtonStep("Pressing the close button", "You can do it!", () => [new ElementGeometryProvider(TopBarBorder, new(10))], () => CreatNewProjectButton)
                        ])
                    ],
                    new("Home Screen",
                        "This is the default launch screen of the app where you can see current releases, feature spotlights and Socials, where you can ask questions about Arcanum and get support.",
                        () => [new ElementGeometryProvider(ContentControl, new(5,0,-50,0)), new ElementGeometryProvider(TopBarBorder)]))
            ]
        );
    }
}