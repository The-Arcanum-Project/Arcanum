using System.Collections.ObjectModel;
using System.Windows.Input;
using Arcanum.UI.Components.StyleClasses;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class SearchWindow
{
   public SearchWindow()
   {
      InitializeComponent();

      var collection = new ObservableCollection<SearchResultItem>
      {
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "SearchWindow.xaml",
            Description = @"Arcanum.UI\TestingAndDebugging",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "Program",
            Description = "Arcanum.UI",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "PluginSettingsWindow.Designer",
            Description = @"Arcanum.UI\HostUIServices\SettingsGUI",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "OutputRichTextBox",
            Description = @"Arcanum.UI\ConsoleUi",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "SearchWindow.xaml",
            Description = @"Arcanum.UI\TestingAndDebugging",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "Program",
            Description = @"Arcanum.UI",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "PluginSettingsWindow.Designer",
            Description = @"Arcanum.UI\HostUIServices\SettingsGUI",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "OutputRichTextBox",
            Description = @"Arcanum.UI\ConsoleUi",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "SearchWindow.xaml",
            Description = @"Arcanum.UI\TestingAndDebugging",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "Program",
            Description = @"Arcanum.UI",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "PluginSettingsWindow.Designer",
            Description = @"Arcanum.UI\HostUIServices\SettingsGUI",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "OutputRichTextBox",
            Description = @"Arcanum.UI\ConsoleUi",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "SearchWindow.xaml",
            Description = @"Arcanum.UI\TestingAndDebugging",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "Program",
            Description = @"Arcanum.UI",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "PluginSettingsWindow.Designer",
            Description = @"Arcanum.UI\HostUIServices\SettingsGUI",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "OutputRichTextBox",
            Description = @"Arcanum.UI\ConsoleUi",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "SearchWindow.xaml",
            Description = @"Arcanum.UI\TestingAndDebugging",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "Program",
            Description = @"Arcanum.UI",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "PluginSettingsWindow.Designer",
            Description = @"Arcanum.UI\HostUIServices\SettingsGUI",
         },
         new()
         {
            IconPath = "/Assets/Icons/Missing16x16.png",
            MainText = "OutputRichTextBox",
            Description = @"Arcanum.UI\ConsoleUi",
         },
      };

      SearchResultsListBox.ItemsSource = collection;
   }

   protected override void OnPreviewKeyDown(KeyEventArgs e)
   {
      if (e.Key == Key.Escape)
         Close();
      base.OnPreviewKeyDown(e);
   }
}