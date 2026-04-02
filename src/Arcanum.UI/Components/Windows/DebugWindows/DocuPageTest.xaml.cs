#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.UI.Documentation;
using Common;
using CommunityToolkit.Mvvm.Input;

#endregion

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class DocuPageTest
{
   public static readonly DependencyProperty SelectedPageProperty =
      DependencyProperty.Register(nameof(SelectedPage), typeof(DocuPage), typeof(DocuPageTest), new(default(DocuPage)));

   public static readonly DependencyProperty AllSnippetIdsProperty =
      DependencyProperty.Register(nameof(AllSnippetIds), typeof(string[]), typeof(DocuPageTest), new(default(string[])));

   public DocuPageTest()
   {
      DocuPages = DocuPathResolver.GetAllDocuPages;
      DataContext = this;
      InitializeComponent();

      DocuPathResolver.OnDocumentationReloaded += HandleDocumentationReloaded;
      Closed += (_, _) => DocuPathResolver.OnDocumentationReloaded -= HandleDocumentationReloaded;

      CopySnippetCommand = new RelayCommand<string>(id =>
      {
         if (string.IsNullOrEmpty(id))
            return;

         var clipboardText = $"{{{{snippet:{id}}}}}";
         Clipboard.SetText(clipboardText);
      });
      AllSnippetIds = DocuPathResolver.GetAllSnippetIds;
   }

   public DocuPage[] DocuPages { get; }

   public string[] AllSnippetIds
   {
      get => (string[])GetValue(AllSnippetIdsProperty);
      set => SetValue(AllSnippetIdsProperty, value);
   }

   public DocuPage SelectedPage
   {
      get => (DocuPage)GetValue(SelectedPageProperty);
      set => SetValue(SelectedPageProperty, value);
   }
   public ICommand CopySnippetCommand { get; set; }

   private void HandleDocumentationReloaded()
   {
      Dispatcher.BeginInvoke(() =>
      {
         var selectedId = (DocuPageListView.SelectedItem as DocuPage)?.Id;

         DocuPageListView.ItemsSource = null;
         DocuPageListView.ItemsSource = DocuPathResolver.GetAllDocuPages;

         AllSnippetIds = DocuPathResolver.GetAllSnippetIds;

         if (selectedId == null)
            return;

         var updatedPage = DocuPathResolver.GetPage(selectedId);
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

   private void DisplayPage(DocuPage page)
   {
      Viewer.Markdown = page.Content;
   }

   private void OnForceReload(object sender, RoutedEventArgs e)
   {
      DocuPathResolver.ReloadAll();
   }

   private void OnOpenInEditor(object sender, RoutedEventArgs e)
   {
      if (DocuPageListView.SelectedItem is DocuPage page)
         ProcessHelper.OpenVsCodeAtLineOfFile(page.SourcePath, 0, 0);
   }
}