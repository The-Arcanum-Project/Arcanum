#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Markdig;
using Markdig.Extensions.CustomContainers;
using Markdig.Renderers;
using Markdig.Renderers.Wpf;
using Markdig.Syntax.Inlines;

#endregion

namespace Arcanum.UI.Documentation.Renderers;

public class TabsWpfRenderer : WpfObjectRenderer<CustomContainer>
{
   private static ResourceDictionary? _tabStyles;

   protected override void Write(WpfRenderer renderer, CustomContainer obj)
   {
      if (obj.Info != "tabs")
      {
         renderer.WriteChildren(obj);
         return;
      }

      if (_tabStyles == null)
      {
         var uri = new Uri("pack://application:,,,/Arcanum_UI;component/Documentation/Renderers/ThinTabbuttonStyle.xaml", UriKind.Absolute);
         _tabStyles = new() { Source = uri };
      }

      var mainGrid = new Grid { Margin = new(0, 15, 0, 15) };
      mainGrid.RowDefinitions.Add(new() { Height = GridLength.Auto }); // Header Row
      mainGrid.RowDefinitions.Add(new() { Height = GridLength.Auto }); // Content Row

      var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
      Grid.SetRow(headerPanel, 0);
      mainGrid.Children.Add(headerPanel);

      var contentGrid = new Grid();
      var contentBorder = new Border
      {
         BorderBrush = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)),
         BorderThickness = new(1),
         Margin = new(0, 5, 0, 0),
         Child = contentGrid,
      };

      Grid.SetRow(contentBorder, 1);
      mainGrid.Children.Add(contentBorder);

      var tabGroupId = Guid.NewGuid().ToString(); // Ensures radio buttons group properly per container
      var tabs = new List<(RadioButton btn, UIElement content)>();
      var isFirst = true;

      foreach (var block in obj)
         if (block is CustomContainer tabContainer && tabContainer.Info == "tab")
         {
            var tabTitle = tabContainer.Arguments ?? "Tab";

            // Tab Header (Styled as a RadioButton)
            var btn = new RadioButton
            {
               Content = tabTitle,
               GroupName = tabGroupId,
               Style = (Style)_tabStyles["TabButtonStyle"]!,
               IsChecked = isFirst,
            };

            // Render the Markdown content
            var section = new Section();
            renderer.Push(section);
            renderer.WriteChildren(tabContainer);
            renderer.Pop();

            // Cleanly detach the section so it doesn't duplicate in the main view
            if (section.Parent is Section parentSection)
               parentSection.Blocks.Remove(section);
            else if (section.Parent is FlowDocument parentDoc)
               parentDoc.Blocks.Remove(section);
            else if (section.Parent is ListItem parentList)
               parentList.Blocks.Remove(section);

            var flowDoc = new FlowDocument { PagePadding = new(0) };
            flowDoc.Blocks.Add(section);

            var viewer = new FlowDocumentScrollViewer
            {
               Document = flowDoc,
               VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
               HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
               Margin = new(0),
               Padding = new(10),
               Visibility = isFirst ? Visibility.Visible : Visibility.Hidden,
            };

            tabs.Add((btn, viewer));
            headerPanel.Children.Add(btn);
            contentGrid.Children.Add(viewer);

            isFirst = false;
         }

      foreach (var tab in tabs)
         tab.btn.Checked += (_, _) =>
         {
            foreach (var t in tabs)
               t.content.Visibility = t.btn.IsChecked == true ? Visibility.Visible : Visibility.Hidden;
         };

      var uiContainer = new BlockUIContainer(mainGrid);
      renderer.Push(uiContainer);
      renderer.Pop();
   }
}

public class WpfCustomControlsExtension : IMarkdownExtension
{
   public void Setup(MarkdownPipelineBuilder pipeline)
   {
   }

   public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
   {
      if (renderer is WpfRenderer wpfRenderer)
      {
         wpfRenderer.ObjectRenderers.Insert(0, new AlertBlockWpfRenderer());
         wpfRenderer.ObjectRenderers.Insert(0, new TabsWpfRenderer());
         var defaultLinkRenderer = wpfRenderer.ObjectRenderers.OfType<WpfObjectRenderer<LinkInline>>().FirstOrDefault();
         if (defaultLinkRenderer != null)
         {
            // Remove the default one
            wpfRenderer.ObjectRenderers.Remove(defaultLinkRenderer);

            // Insert ours, giving it the default one to use as a fallback
            wpfRenderer.ObjectRenderers.Insert(0, new CustomImageWpfRenderer(defaultLinkRenderer));
         }
      }
   }
}