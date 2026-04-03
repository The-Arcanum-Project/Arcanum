#region

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.UI.AppFeatures;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.Windows.PopUp;
using Arcanum.UI.Documentation.Implementation;
using Common;
using Common.UI.MBox;
using CommunityToolkit.Mvvm.Input;

#endregion

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class DocuPageTest
{
   private enum CopiedType
   {
      None,
      CommandScope,
      Status,
      Level,
      Category,
      Location,
      Scale,
   }
   
   public static readonly DependencyProperty SelectedPageProperty =
      DependencyProperty.Register(nameof(SelectedPage), typeof(FeatureDoc), typeof(DocuPageTest), new(default(FeatureDoc)));

   public static readonly DependencyProperty AllSnippetIdsProperty =
      DependencyProperty.Register(nameof(AllSnippetIds), typeof(string[]), typeof(DocuPageTest), new(default(string[])));

   public DocuPageTest()
   {
      DocuPages = DocuRegistry.GetAllDocuPages;
      DataContext = this;
      InitializeComponent();

      DocuRegistry.OnDocumentationReloaded += HandleDocumentationReloaded;
      Closed += (_, _) => DocuRegistry.OnDocumentationReloaded -= HandleDocumentationReloaded;

      CopySnippetCommand = new RelayCommand<string>(id =>
      {
         if (string.IsNullOrEmpty(id))
            return;

         var clipboardText = $"{{{{snippet:{id}}}}}";
         Clipboard.SetText(clipboardText);
      });
      AllSnippetIds = DocuRegistry.GetAllSnippetIds;
   }

   public FeatureDoc[] DocuPages { get; set; }
   private CopiedType _lastCopiedType = CopiedType.None;

   public string[] AllSnippetIds
   {
      get => (string[])GetValue(AllSnippetIdsProperty);
      set => SetValue(AllSnippetIdsProperty, value);
   }
   public string[] AllStatus => Enum.GetNames<FeatureStatus>();
   public string[] AllFeatureScale => Enum.GetNames<FeatureScale>();
   public string[] AllFeatureLevel => Enum.GetNames<FeatureLevel>();
   public string[] AllFeatureCategory => Enum.GetNames<FeatureCategory>();
   public string[] AllFeatureLocation => Enum.GetNames<FeatureLocation>();

   public FeatureDoc SelectedPage
   {
      get => (FeatureDoc)GetValue(SelectedPageProperty);
      set => SetValue(SelectedPageProperty, value);
   }
   public ICommand CopySnippetCommand { get; set; }

   private void HandleDocumentationReloaded()
   {
      Dispatcher.BeginInvoke(() =>
      {
         var selectedId = (DocuPageListView.SelectedItem as FeatureDoc)?.Id;

         DocuPageListView.ItemsSource = null;
         DocuPageListView.ItemsSource = DocuRegistry.GetAllDocuPages;

         AllSnippetIds = DocuRegistry.GetAllSnippetIds;

         if (selectedId == null)
            return;

         var updatedPage = DocuRegistry.GetPage(selectedId);
         if (updatedPage != null)
            DisplayPage(updatedPage);

         SelectedPage = updatedPage ?? SelectedPage;
      });
   }

   private void DocuPageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is not ListView listView)
         return;

      var index = listView.SelectedIndex;
      if (index < 0 || index >= DocuPages.Length)
      {
         Viewer.Markdown = string.Empty;
         return;
      }

      DisplayPage(DocuPages[index]);
   }

   private void DisplayPage(FeatureDoc page)
   {
      Viewer.Markdown = page.Content;
   }

   private void OnForceReload(object sender, RoutedEventArgs e)
   {
      DocuRegistry.ReloadAll();
   }

   private void OnOpenInEditor(object sender, RoutedEventArgs e)
   {
      if (DocuPageListView.SelectedItem is FeatureDoc page)
         ProcessHelper.OpenVsCodeAtLineOfFile(page.SourcePath, 0, 0);
   }

   private static void AppendToClipboard(string text, string separator)
   {
      if (string.IsNullOrEmpty(text))
         return;

      var current = Clipboard.GetText();
      if (!string.IsNullOrEmpty(current))
         text = current + separator + text;

      Clipboard.Clear();
      SetClipBoardText(text);
   }

   private static void SetClipBoardText(string text)
   {
      if (string.IsNullOrEmpty(text))
         return;

      Clipboard.SetText(text);
   }

   private void HandleClipboardOperation(object sender, CopiedType type)
   {
      if (sender is not BaseButton { Content: string scope })
         return;

      if (_lastCopiedType == type && Keyboard.Modifiers == ModifierKeys.Shift)
         AppendToClipboard(scope, ", ");
      else
         SetClipBoardText(scope);

      _lastCopiedType = type;
   }

   private void AvailableScopes_OnClick(object sender, RoutedEventArgs e)
   {
      HandleClipboardOperation(sender, CopiedType.CommandScope);
   }

   private void AvailableStatus_OnClick(object sender, RoutedEventArgs e)
   {
      HandleClipboardOperation(sender, CopiedType.Status);
   }

   private void AvailableLevels_OnClick(object sender, RoutedEventArgs e)
   {
      HandleClipboardOperation(sender, CopiedType.Level);
   }

   private void AvailableLocations_OnClick(object sender, RoutedEventArgs e)
   {
      HandleClipboardOperation(sender, CopiedType.Location);
   }

   private void AvailableCategory_OnClick(object sender, RoutedEventArgs e)
   {
      HandleClipboardOperation(sender, CopiedType.Category);
   }

   private void AvailableScale_OnClick(object sender, RoutedEventArgs e)
   {
      HandleClipboardOperation(sender, CopiedType.Scale);
   }

   private void OnOpenFolder(object sender, RoutedEventArgs e)
   {
      if (DocuPageListView.SelectedItem is FeatureDoc page)
         ProcessHelper.OpenExplorerAndSelectFile(page.SourcePath);
   }

   private void OnNewSnippet(object sender, RoutedEventArgs e)
   {
      if (DocuRegistry.ExternalSnippetsPath == null)
      {
         MBox.Show("External snippets path is not set. Please set it in the settings first.", "Error", MBoxButton.OK, MessageBoxImage.Error);
         return;
      }

      var path = ProcessHelper.OpenFileCreationDialog(DocuRegistry.ExternalSnippetsPath, "*.md");
      ProcessHelper.OpenVsCodeAtLineOfFile(path, 0, 0);

      AllSnippetIds = DocuRegistry.GetAllSnippetIds;
   }

   private void OnNewPage(object sender, RoutedEventArgs e)
   {
      if (DocuRegistry.ExternalPath == null)
      {
         MBox.Show("External documentation path is not set. Please set it in the settings first.", "Error", MBoxButton.OK, MessageBoxImage.Error);
         return;
      }

      var path = ProcessHelper.OpenFileCreationDialog(DocuRegistry.ExternalPath, "*.md");
      var templateContent = IO.ReadAllTextUtf8(Path.Combine(DocuRegistry.ExternalPath, "Template.md"))?.Replace('#', ' ');
      IO.WriteAllTextUtf8(path, templateContent ?? string.Empty);
      ProcessHelper.OpenVsCodeAtLineOfFile(path, 0, 0);

      DocuPages = DocuRegistry.GetAllDocuPages;
   }
}