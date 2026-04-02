#region

using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using Arcanum.UI.Commands;
using Arcanum.UI.Documentation;
using Arcanum.UI.Documentation.Renderers;
using Common;
using CommunityToolkit.Mvvm.Input;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Block = System.Windows.Documents.Block;

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

      var targetText = GetTargetTextFromMarkdown(Viewer.Markdown, tag.ToLower().Trim());

      if (string.IsNullOrEmpty(targetText))
         return;

      FindElementByText(Viewer.Document, targetText)?.BringIntoView();
   }

   private static string? GetTargetTextFromMarkdown(string markdown, string slug)
   {
      if (string.IsNullOrEmpty(markdown))
         return null;

      // Custom Anchor like `{#my-anchor}`
      var pattern = $@"(.*)\{{#{Regex.Escape(slug)}\}}";
      var customIdMatch = Regex.Match(markdown, pattern);
      if (customIdMatch.Success)
         return customIdMatch.Groups[1].Value.Trim();

      // Auto-generated Heading like `# Alerts Demonstration`
      var headingMatches = Regex.Matches(markdown, @"^(#{1,6})\s+(.*)$", RegexOptions.Multiline);
      foreach (Match match in headingMatches)
      {
         var headingText = match.Groups[2].Value.Trim();

         var headingSlug = headingText.ToLower().Replace(" ", "-");
         headingSlug = Regex.Replace(headingSlug, @"[^a-z0-9\-]", "");

         if (headingSlug == slug)
            return headingText;
      }

      return null;
   }

   private static FrameworkContentElement? FindElementByText(FlowDocument doc, string targetText)
   {
      if (string.IsNullOrEmpty(targetText))
         return null;

      foreach (var block in doc.Blocks)
      {
         var result = SearchBlockForText(block, targetText);
         if (result != null)
            return result;
      }

      return null;
   }

   private static FrameworkContentElement? SearchBlockForText(Block block, string targetText)
   {
      var blockText = new TextRange(block.ContentStart, block.ContentEnd).Text.Trim();

      // Prioritize an exact match (like a standalone heading)
      if (blockText.Equals(targetText, StringComparison.OrdinalIgnoreCase))
         return block;

      // Fallback to a partial match (if it's text inside a larger paragraph)
      if (blockText.Contains(targetText, StringComparison.OrdinalIgnoreCase))
         return block;

      switch (block)
      {
         // Drill down into Sections (Tabs, Alerts, Custom Containers)
         case Section section:
         {
            foreach (var subBlock in section.Blocks)
            {
               var result = SearchBlockForText(subBlock, targetText);
               if (result != null)
                  return result;
            }

            break;
         }
         // Drill down into Lists
         case List list:
         {
            foreach (var listItem in list.ListItems)
            {
               foreach (var subBlock in listItem.Blocks)
               {
                  var result = SearchBlockForText(subBlock, targetText);
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
}