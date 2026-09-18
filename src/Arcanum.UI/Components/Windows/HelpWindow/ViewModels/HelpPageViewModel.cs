#region

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Arcanum.UI.Documentation.Implementation;

#endregion

namespace Arcanum.UI.Components.Windows.HelpWindow.ViewModels;

public class HelpWindowViewModel : INotifyPropertyChanged, IHelpPageViewModelWrapper
{
   private HelpPageViewModelBase _currentPage;
   private NavMenuItem? _selectedMenuItem;

   public HelpWindowViewModel()
   {
      // Initialize pages (we keep them in memory to preserve search/scroll states)
      Dashboard = new();
      Explorer = new();
      Shortcuts = new();
      Tutorial = new();

      MenuItems.Add(new("Dashboard", "Icon.Home", Dashboard));
      MenuItems.Add(new("Features", "Icon.Explorer", Explorer));
      MenuItems.Add(new("Shortcuts", "Icon.Keyboard", Shortcuts));
      MenuItems.Add(new("Tutorial", "Icon.Tutorial", Tutorial));

      // Set default
      _selectedMenuItem = MenuItems[0];
      _currentPage = _selectedMenuItem.ViewModel;
   }

   public DashboardViewModel Dashboard { get; }
   public FeatureExplorerViewModel Explorer { get; }
   public CommandMapViewModel Shortcuts { get; }
   public TutorialViewModel Tutorial { get; }

   public ObservableCollection<NavMenuItem> MenuItems { get; } = [];

   public HelpPageViewModelBase CurrentPage
   {
      get => _currentPage;
      set
      {
         _currentPage = value;
         OnPropertyChanged();
      }
   }

   public NavMenuItem? SelectedMenuItem
   {
      get => _selectedMenuItem;
      set
      {
         _selectedMenuItem = value;
         OnPropertyChanged();
      }
   }

   public event PropertyChangedEventHandler? PropertyChanged;

   public void Navigate(NavMenuItem item)
   {
      CurrentPage = item.ViewModel;
      SelectedMenuItem = item;
   }

   protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));

   public void ActivateFeatureTabFor(FeatureDoc feature)
   {
      // Navigate to the explorer page and activate the tab for the given feature
      Navigate(MenuItems.First(m => m.ViewModel == Explorer));
      Explorer.SelectFeature(feature);
   }

   public void ShowNextTip()
   {
      Dashboard.ShowNextTip();
   }

   public void ShowPreviousTip()
   {
      Dashboard.ShowPreviousTip();
   }
}