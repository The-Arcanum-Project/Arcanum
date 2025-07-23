using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Mod;
using Arcanum.Core.Globals;
using Arcanum.Core.Utils.vdfParser;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.UserControls.MainMenuScreen;

namespace Arcanum.UI.Components.Views.MainMenuScreen;

public partial class ArcanumViewModel
{
   public ObservableCollection<BaseModItem> BaseMods { get; set; } = [];

   private readonly MainMenuViewModel _mainMenuViewModel;
   private readonly List<ProjectFileDescriptor> _recentProjectDescriptors = [];

   public ArcanumViewModel(List<ProjectFileDescriptor> descriptors, MainMenuViewModel mainMenuViewModel)
   {
      _mainMenuViewModel = mainMenuViewModel;
      InitializeComponent();

      DataContext = this;
      VanillaFolderTextBox.Text = VdfParser.GetEu5Path();

      descriptors.Sort();

      _recentProjectDescriptors.AddRange(descriptors);
      SetRecentProjects();
   }

   private void SetRecentProjects(int start = 0)
   {
      for (var i = start; i < Math.Min(4, _recentProjectDescriptors.Count); i++)
         RecentProjectsPanel.Children.Add(new RecentProjectCard(_recentProjectDescriptors[i], _mainMenuViewModel));
   }

   private void AddBaseMod(BaseModItem item) => BaseMods.Add(item);

   private void RemoveBaseMod(BaseModItem item) => BaseMods.Remove(item);

   public void RemoveRecentProject(ProjectFileDescriptor descriptor)
   {
      var card = RecentProjectsPanel.Children
         .OfType<RecentProjectCard>()
         .FirstOrDefault(c => Equals(c.Descriptor, descriptor));

      if (card != null)
      {
         RecentProjectsPanel.Children.Remove(card);
         _recentProjectDescriptors.Remove(descriptor);
      }

      AddMoreRecentProjects();
   }

   private void AddMoreRecentProjects()
   {
      var cardCount = RecentProjectsPanel.Children.OfType<RecentProjectCard>().Count();
      SetRecentProjects(cardCount);
   }

   private void VanillaFolderButton_Click(object sender, RoutedEventArgs e)
   {
      var defaultPath = VdfParser.GetEu5Path();
      var path = IO.SelectFolder(defaultPath, "Select the EU5 vanilla folder");
      VanillaFolderTextBox.Text = path ?? string.Empty;
   }

   private void ModFolderButton_Click(object sender, RoutedEventArgs e)
   {
      var modPath = IO.SelectFolder(IO.GetUserModFolderPath, "Select mod folder");
      ModFolderTextBox.Text = modPath ?? string.Empty;
   }

   private void AddBaseModButton_Click(object sender, RoutedEventArgs e)
   {
      var newItemPath = IO.SelectFolder(IO.GetUserModFolderPath, "Select a base mod folder");
      if (string.IsNullOrEmpty(newItemPath) ||
          BaseMods.Any(item => item.Path == newItemPath) ||
          !Directory.Exists(newItemPath))
         return;

      var newItem = new BaseModItem(RemoveBaseMod) { Path = newItemPath };

      AddBaseMod(newItem);
   }

   private BaseModItem? _draggedItem;

   public void ClearUi()
   {
      ModFolderTextBox.Text = string.Empty;
      VanillaFolderTextBox.Text = AppData.MainMenuScreenDescriptor.LastVanillaPath ?? string.Empty;

      BaseMods.Clear();
      BaseModsListBox.ItemsSource = BaseMods;
   }

   public void DescriptorToUi(ProjectFileDescriptor descriptor)
   {
      ModFolderTextBox.Text = descriptor.ModPath;
      VanillaFolderTextBox.Text = descriptor.VanillaPath;

      BaseMods.Clear();
      foreach (var baseMod in descriptor.RequiredMods)
      {
         var item = new BaseModItem(RemoveBaseMod) { Path = baseMod };
         AddBaseMod(item);
      }
   }
}

public class ZeroToVisibilityConverter : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is int count)
         return count == 0 ? Visibility.Visible : Visibility.Collapsed;

      return Visibility.Collapsed;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
      => throw new NotImplementedException();
}