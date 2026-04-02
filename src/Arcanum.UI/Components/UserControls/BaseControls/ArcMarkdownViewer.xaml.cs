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
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Syntax;
using Block = System.Windows.Documents.Block;

#endregion

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class ArcMarkdownViewer
{
   public static readonly DependencyProperty MarkdownProperty = DependencyProperty.Register(nameof(Markdown),
                                                                                            typeof(string),
                                                                                            typeof(ArcMarkdownViewer),
                                                                                            new(string.Empty, OnMarkdownChanged));

   private ScrollViewer? _internalScrollViewer;

   public ArcMarkdownViewer()
   {
      InitializeComponent();

      var uiPipelineBuilder = new MarkdownPipelineBuilder()
                             .UseAdvancedExtensions()
                             .UseAlertBlocks()
                             .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
                             .UseGenericAttributes()
                             .UseCustomContainers();

      uiPipelineBuilder.Extensions.Add(new WpfCustomControlsExtension());
      uiPipelineBuilder.Extensions.Add(new WpfImageResourceExtension());

      InternalViewer.Pipeline = uiPipelineBuilder.Build();
   }

   public string Markdown
   {
      get => (string)GetValue(MarkdownProperty);
      set => SetValue(MarkdownProperty, value);
   }

   private static void OnMarkdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      if (d is ArcMarkdownViewer control)
         control.InternalViewer.Markdown = DocuPathResolver.ProcessSnippets(e.NewValue?.ToString() ?? string.Empty);
   }

   private void OpenHyperlink(object sender, ExecutedRoutedEventArgs e)
   {
      var current = e.OriginalSource as DependencyObject;
      Hyperlink? hyperlink = null;

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

      if ((string.IsNullOrEmpty(uri) || uri == "#") && hyperlink != null)
      {
         var linkText = new TextRange(hyperlink.ContentStart, hyperlink.ContentEnd).Text;
         if (!string.IsNullOrEmpty(linkText) && InternalViewer.Markdown != null)
         {
            var match = Regex.Match(InternalViewer.Markdown, $@"\[{Regex.Escape(linkText)}\]\((.*?)\)");
            if (match.Success)
               uri = match.Groups[1].Value;
         }
      }

      if (string.IsNullOrEmpty(uri) || uri == "#")
         return;

      if (uri.StartsWith("#"))
         ScrollToTag(uri.TrimStart('#'));
      else if (uri.StartsWith("id:"))
      {
         var parts = uri[3..].Split('#');
         var page = DocuPathResolver.GetPage(new(parts[0]));
         if (page != null)
         {
            Markdown = DocuPathResolver.ProcessSnippets(page.Content);
            if (parts.Length > 1)
               Dispatcher.BeginInvoke(DispatcherPriority.Background, () => ScrollToTag(parts[1]));
         }
      }
      else if (uri.StartsWith("cmd:"))
      {
         if (CommandRegistry.TryGetFromString(uri[4..], out var cmd) && cmd.CanExecute(null))
            cmd.Execute(null);
      }
      else if (uri.StartsWith("http"))
         Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });

      e.Handled = true;
   }

   private void ScrollToTag(string tag)
   {
      if (string.IsNullOrEmpty(tag) || InternalViewer.Markdown == null || InternalViewer.Document == null)
         return;

      var target = GetTargetInfoFromMarkdown(InternalViewer.Markdown, tag.ToLower().Trim());
      if (!string.IsNullOrEmpty(target.Text))
         FindElementByText(InternalViewer.Document, target.Text, target.IsHeading)?.BringIntoView();
   }

   private static (string? Text, bool IsHeading) GetTargetInfoFromMarkdown(string markdown, string slug)
   {
      var customMatch = Regex.Match(markdown, $@"(.*)\{{#{Regex.Escape(slug)}\}}");
      if (customMatch.Success)
         return (customMatch.Groups[1].Value.Trim(), false);

      var headingMatches = Regex.Matches(markdown, @"^(#{1,6})\s+(.*)$", RegexOptions.Multiline);
      foreach (Match match in headingMatches)
      {
         var text = match.Groups[2].Value.Trim();
         var headingSlug = Regex.Replace(text.ToLower().Replace(" ", "-"), @"[^a-z0-9\-]", "").Trim('-');
         if (headingSlug == slug)
            return (text, true);
      }

      return (null, false);
   }

   private static FrameworkContentElement? FindElementByText(FlowDocument doc, string text, bool isHeading)
   {
      foreach (var block in doc.Blocks)
      {
         var result = SearchBlockForText(block, text, isHeading);
         if (result != null)
            return result;
      }

      return null;
   }

   private static FrameworkContentElement? SearchBlockForText(Block block, string targetText, bool isHeading)
   {
      var blockText = new TextRange(block.ContentStart, block.ContentEnd).Text.Trim();
      var isHeadingLike = block.Tag is HeadingBlock || block.FontWeight == FontWeights.Bold || block.FontWeight == FontWeights.SemiBold;

      if (isHeading
             ? isHeadingLike && blockText.Equals(targetText, StringComparison.OrdinalIgnoreCase)
             : blockText.Contains(targetText, StringComparison.OrdinalIgnoreCase))
         return block;

      IEnumerable<Block>? children = block switch
      {
         Section s => s.Blocks,
         List l => l.ListItems.SelectMany(li => li.Blocks),
         _ => null,
      };
      if (children != null)
         foreach (var child in children)
         {
            var res = SearchBlockForText(child, targetText, isHeading);
            if (res != null)
               return res;
         }

      return null;
   }

   private void Viewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
   {
      _internalScrollViewer ??= GetChildOfType<ScrollViewer>(InternalViewer);
      if (_internalScrollViewer != null)
      {
         _internalScrollViewer.ScrollToVerticalOffset(_internalScrollViewer.VerticalOffset - e.Delta * 2.5 / 3.0);
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