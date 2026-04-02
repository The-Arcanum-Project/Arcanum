#region

using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Arcanum.UI.Commands;
using Arcanum.UI.Documentation;
using Arcanum.UI.Documentation.Renderers;
using Common;
using CommunityToolkit.Mvvm.Input;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Syntax;
using Block = System.Windows.Documents.Block;

#endregion

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class DocuPageTest
{
   public static readonly DependencyProperty SelectedPageProperty =
      DependencyProperty.Register(nameof(SelectedPage), typeof(DocuPage), typeof(DocuPageTest), new(default(DocuPage)));

   public static readonly DependencyProperty AllSnippetIdsProperty =
      DependencyProperty.Register(nameof(AllSnippetIds), typeof(string[]), typeof(DocuPageTest), new(default(string[])));

   private ScrollViewer? _internalScrollViewer;

   public DocuPageTest()
   {
      DocuPages = DocuPathResolver.GetAllDocuPages;
      DataContext = this;
      InitializeComponent();

      var uiPipelineBuilder = new MarkdownPipelineBuilder()
                             .UseAdvancedExtensions()
                             .UseAlertBlocks()
                             .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
                             .UseGenericAttributes()
                             .UseCustomContainers();

      uiPipelineBuilder.Extensions.Add(new WpfCustomControlsExtension());
      uiPipelineBuilder.Extensions.Add(new WpfImageResourceExtension());

      Viewer.Pipeline = uiPipelineBuilder.Build();

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
      Viewer.Markdown = DocuPathResolver.ProcessSnippets(page.Content);
   }

   private void OpenHyperlink(object sender, ExecutedRoutedEventArgs e)
   {
      var current = e.OriginalSource as DependencyObject;
      Hyperlink hyperlink = null!;

      while (current != null)
      {
         if (current is Hyperlink hl)
         {
            hyperlink = hl;
            break;
         }

         current = LogicalTreeHelper.GetParent(current);
      }

      var uri = e.Parameter?.ToString() ?? string.Empty;

      if ((string.IsNullOrEmpty(uri) || uri == "#") && hyperlink != null!)
      {
         var linkText = new TextRange(hyperlink.ContentStart, hyperlink.ContentEnd).Text;

         if (!string.IsNullOrEmpty(linkText) && Viewer.Markdown != null)
         {
            var match = Regex.Match(Viewer.Markdown,
                                    $@"\[{Regex.Escape(linkText)}\]\((.*?)\)");
            if (match.Success)
               uri = match.Groups[1].Value;
         }
      }

      if (string.IsNullOrEmpty(uri) || uri == "#")
         return;

      // Internal Tag Links (#Fragment)
      if (uri.StartsWith("#"))
         ScrollToTag(uri.TrimStart('#'));

      // Internal Page Links (id:PageId#Fragment)
      else if (uri.StartsWith("id:"))
      {
         var parts = uri[3..].Split('#');
         var pageId = parts[0];
         var fragment = parts.Length > 1 ? parts[1] : null;

         var page = DocuPathResolver.GetPage(new(pageId));
         if (page != null)
         {
            DisplayPage(page);
            if (!string.IsNullOrEmpty(fragment))
               Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                      () => ScrollToTag(fragment));
         }
      }

      // Commands (cmd:OpenCalc)
      else if (uri.StartsWith("cmd:"))
      {
         var command = uri[4..];
         if (CommandRegistry.TryGetFromString(command, out var cmd))
            if (cmd.CanExecute(null))
               cmd.Execute(null);
      }

      // Standard Web Links
      else if (uri.StartsWith("http"))
         Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });

      e.Handled = true;
   }

   private void ScrollToTag(string tag)
   {
      if (string.IsNullOrEmpty(tag) || Viewer.Markdown == null || Viewer.Document == null)
         return;

      var target = GetTargetInfoFromMarkdown(Viewer.Markdown, tag.ToLower().Trim());

      if (string.IsNullOrEmpty(target.Text))
         return;

      FindElementByText(Viewer.Document, target.Text, target.IsHeading)?.BringIntoView();
   }

   private static (string? Text, bool IsHeading) GetTargetInfoFromMarkdown(string markdown, string slug)
   {
      var customMatch = Regex.Match(markdown, $@"(.*)\{{#{Regex.Escape(slug)}\}}");
      if (customMatch.Success)
         return (customMatch.Groups[1].Value.Trim(), false);

      var headingMatches = Regex.Matches(markdown, @"^(#{1,6})\s+(.*)$", RegexOptions.Multiline);
      foreach (Match match in headingMatches)
      {
         var headingText = match.Groups[2].Value.Trim();

         var headingSlug = headingText.ToLower().Replace(" ", "-");
         headingSlug = Regex.Replace(headingSlug, @"[^a-z0-9\-]", "");
         headingSlug = headingSlug.Trim('-');

         if (headingSlug == slug)
            return (headingText, true);
      }

      return (null, false);
   }

   private static FrameworkContentElement? FindElementByText(FlowDocument doc, string targetText, bool isHeading)
   {
      foreach (var block in doc.Blocks)
      {
         var result = SearchBlockForText(block, targetText, isHeading);
         if (result != null)
            return result;
      }

      return null;
   }

   private static FrameworkContentElement? SearchBlockForText(Block block, string targetText, bool isHeading)
   {
      var blockText = new TextRange(block.ContentStart, block.ContentEnd).Text.Trim();

      var isHeadingLike = block.Tag is HeadingBlock ||
                          block.FontWeight == FontWeights.Bold ||
                          block.FontWeight == FontWeights.SemiBold;

      if (isHeading)
      {
         if (isHeadingLike && blockText.Equals(targetText, StringComparison.OrdinalIgnoreCase))
            return block;
      }
      else
      {
         if (blockText.Contains(targetText, StringComparison.OrdinalIgnoreCase))
            return block;
      }

      switch (block)
      {
         case Section section:
         {
            foreach (var subBlock in section.Blocks)
            {
               var result = SearchBlockForText(subBlock, targetText, isHeading);
               if (result != null)
                  return result;
            }

            break;
         }
         case List list:
         {
            foreach (var listItem in list.ListItems)
            {
               foreach (var subBlock in listItem.Blocks)
               {
                  var result = SearchBlockForText(subBlock, targetText, isHeading);
                  if (result != null)
                     return result;
               }
            }

            break;
         }
      }

      return null;
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

   private void Viewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
   {
      if (_internalScrollViewer == null)
         _internalScrollViewer = GetChildOfType<ScrollViewer>(Viewer);

      if (_internalScrollViewer != null)
      {
         const double speedMultiplier = 2.5;
         var newOffset = _internalScrollViewer.VerticalOffset - e.Delta * speedMultiplier / 3.0;
         _internalScrollViewer.ScrollToVerticalOffset(newOffset);
         e.Handled = true;
      }
   }

   private static T? GetChildOfType<T>(DependencyObject? depObj) where T : DependencyObject
   {
      if (depObj == null)
         return null;

      for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
      {
         var child = VisualTreeHelper.GetChild(depObj, i);
         var result = child as T ?? GetChildOfType<T>(child);
         if (result != null)
            return result;
      }

      return null;
   }
}