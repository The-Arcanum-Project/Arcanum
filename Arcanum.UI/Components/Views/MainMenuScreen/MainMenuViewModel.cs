using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Mod;
using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GlobalStates;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using static Arcanum.UI.Components.Windows.MainWindows.MainMenuScreen;

namespace Arcanum.UI.Components.Views.MainMenuScreen;

public class MainMenuViewModel : ObservableObject
{
   internal MainMenuScreenView TargetedView { get; set; } = MainMenuScreenView.Home;

   internal StackPanel MainMenuStackPanel { get; set; } = null!;

   public RelayCommand HomeVc { get; set; }
   public RelayCommand ModforgeVc { get; set; }
   public RelayCommand FeatureVc { get; set; }
   public RelayCommand ArcanumVc { get; set; }
   public RelayCommand AboutUsVc { get; set; }
   public RelayCommand AttributionsVc { get; set; }

   public HomeViewModel HomeVm { get; set; }
   public ModforgeViewModel ModforgeVm { get; set; }
   public FeatureViewModel FeatureFm { get; set; }
   public ArcanumViewModel ArcanumVm { get; set; }
   public AboutUsViewModel AboutUsVm { get; set; }
   public AttributionsViewModel AttributionsVm { get; set; }
   private object _currentView = null!;
   private Visibility _isWindowVisible = Visibility.Visible;

   public string LaunchButtonText => CurrentView switch
   {
      ArcanumViewModel => "Current Config:",
      _ => "Last Project:",
   };

   public Visibility IsWindowVisible
   {
      get => _isWindowVisible;
      set
      {
         if (value == _isWindowVisible)
            return;

         _isWindowVisible = value;
         OnPropertyChanged();
      }
   }

   public object CurrentView
   {
      get => _currentView;
      private set
      {
         _currentView = value;
         OnPropertyChanged();
         OnPropertyChanged(nameof(LaunchButtonText));
      }
   }
   public required Windows.MainWindows.MainMenuScreen MenuWindow { get; set; }

   public MainMenuViewModel()
   {
      HomeVm = new();
      ModforgeVm = new();
      FeatureFm = new();
      ArcanumVm = new(AppData.MainMenuScreenDescriptor.ProjectFiles, this);
      AboutUsVm = new();
      AttributionsVm = new();

      CurrentView = HomeVm;

      HomeVc = new(() => { SetCurrentView(MainMenuScreenView.Home); });
      FeatureVc = new(() => { SetCurrentView(MainMenuScreenView.Feature); });
      ModforgeVc = new(() => { SetCurrentView(MainMenuScreenView.Modforge); });
      ArcanumVc = new(() =>
      {
         SetCurrentView(MainMenuScreenView.Arcanum);
         if (string.IsNullOrEmpty(ArcanumVm.ModFolderTextBox.Text))
            ArcanumVm.VanillaFolderTextBox.Text =
               AppData.MainMenuScreenDescriptor.LastVanillaPath?.FullPath ?? string.Empty;
      });
      AboutUsVc = new(() => { SetCurrentView(MainMenuScreenView.AboutUs); });
      AttributionsVc = new(() => { SetCurrentView(MainMenuScreenView.Attributions); });
   }

   internal void SetCurrentView(MainMenuScreenView view)
   {
      Debug.Assert(MainMenuStackPanel != null, "MainMenuStackPanel should not be null");

      switch (view)
      {
         case MainMenuScreenView.Home:
            CurrentView = HomeVm;
            break;
         case MainMenuScreenView.Modforge:
            CurrentView = ModforgeVm;
            break;
         // case MainMenuScreenView.Feature:
         //    CurrentView = FeatureFm;
         //    break;
         case MainMenuScreenView.Arcanum:
            CurrentView = ArcanumVm;
            break;
         case MainMenuScreenView.AboutUs:
            CurrentView = AboutUsVm;
            break;
         case MainMenuScreenView.Attributions:
            CurrentView = AttributionsVm;
            break;
         default:
            throw new ArgumentOutOfRangeException(nameof(view), view, null);
      }

      // Update the button state of the MainMenuStackPanel
      var buttons = MainMenuStackPanel.Children.OfType<RadioButton>().ToList();
      buttons.ForEach(button => button.IsChecked = false);

      if (buttons.Count > (int)view)
         ((RadioButton)MainMenuStackPanel.Children[(int)view]).IsChecked = true;

      TargetedView = view;
   }

   internal bool GetDescriptorFromInput(out ProjectFileDescriptor descriptor)
   {
      var modPath = ArcanumVm.ModFolderTextBox.Text.TrimEnd(Path.DirectorySeparatorChar)
                             .Split(Path.DirectorySeparatorChar);
      var modDataSpace = new DataSpace(Path.GetDirectoryName(ArcanumVm.ModFolderTextBox.Text) ?? string.Empty,
                                       modPath,
                                       DataSpace.AccessType.ReadWrite);

      var vanillaPath = ArcanumVm.VanillaFolderTextBox.Text.TrimEnd(Path.DirectorySeparatorChar)
                                 .Split(Path.DirectorySeparatorChar);
      var vanillaDataSpace = new DataSpace(Path.GetDirectoryName(ArcanumVm.VanillaFolderTextBox.Text) ?? string.Empty,
                                           vanillaPath,
                                           DataSpace.AccessType.ReadOnly);

      descriptor = new(Path.GetFileName(ArcanumVm.ModFolderTextBox.Text.TrimEnd(Path.DirectorySeparatorChar)),
                       modDataSpace,
                       ArcanumVm.BaseMods.Select(mod => mod.DataSpace).ToList(),
                       vanillaDataSpace);

      return descriptor.IsValid();
   }

   // This is the main entry point for the Arcanum application from the main menu.
   // When creating a new project, this method will be called.
   // It validates the project file and launches into the main window of Arcanum
   // if all requirements are met.
   internal Task LaunchArcanum(ProjectFileDescriptor descriptor)
   {
      if (!descriptor.IsValid())
      {
         MessageBox.Show("Could not create a valid 'ProjectDescriptor'.\n" +
                         "Please make sure to have valid paths for the mod- and the vanilla folder.\n\n " +
                         "If you are using base mods make sure that they are valid, too.",
                         "Invalid Project Data",
                         MessageBoxButton.OK,
                         MessageBoxImage.Error);
         return Task.CompletedTask;
      }

      descriptor.LoadToApplication();
      // Save the paths to the MainMenuScreenDescriptor
      AppData.MainMenuScreenDescriptor.LastVanillaPath = descriptor.VanillaPath;
      AppData.MainMenuScreenDescriptor.LastProjectFile = descriptor.ModName;

      if (AppData.MainMenuScreenDescriptor.ProjectFiles
                 .Any(x => x.ModName.Equals(descriptor.ModName, StringComparison.OrdinalIgnoreCase)))
      {
         AppData.MainMenuScreenDescriptor.ProjectFiles.RemoveAll(x => x.ModName.Equals(descriptor.ModName,
                                                                  StringComparison.OrdinalIgnoreCase));
      }

      AppData.MainMenuScreenDescriptor.ProjectFiles.Add(descriptor);

      MenuWindow.LoadAndTransfer();
      return Task.CompletedTask;
   }
}