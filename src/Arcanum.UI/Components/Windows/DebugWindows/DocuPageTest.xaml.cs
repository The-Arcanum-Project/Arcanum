#region

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Arcanum.UI.AppFeatures;
using Arcanum.UI.Documentation;
using Arcanum.UI.Documentation.Renderers;
using Markdig;

#endregion

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class DocuPageTest
{
   public DocuPage[] DocuPages { get; }

   public DocuPageTest()
   {
      DocuPages = DocuPathResolver.GetAllDocuPages;
      DataContext = this;
      InitializeComponent();

      var uiPipelineBuilder = new MarkdownPipelineBuilder()
                             .UseAdvancedExtensions()
                             .UseAlertBlocks()
                             .UseAutoIdentifiers()
                             .UseGenericAttributes()
                             .UseCustomContainers();

      uiPipelineBuilder.Extensions.Add(new WpfCustomControlsExtension());
      uiPipelineBuilder.Extensions.Add(new WpfImageResourceExtension());

      Viewer.Pipeline = uiPipelineBuilder.Build();
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

   private void OnLinkClick(object sender, RequestNavigateEventArgs e)
   {
   }

   private void OpenHyperlink(object sender, ExecutedRoutedEventArgs e)
   {
      string uri = e.Parameter.ToString();

      // 1. Cross-Reference (id:Editor.Map)
      if (uri.StartsWith("id:"))
      {
         var id = new FeatureId(uri.Substring(3));
         var page = DocuPathResolver.GetPage(id);
         if (page != null)
            DisplayPage(page);
      }

      // 2. Commands (cmd:OpenCalc)
      else if (uri.StartsWith("cmd:"))
      {
         string command = uri.Substring(4);
         // ExecuteAppCommand(command);
      }

      // 3. Custom Tooltip Link [Text](tooltip:"Content")
      else if (uri.StartsWith("tooltip:"))
      {
         // For tooltips as links, you might want to show a Popup or a Message
         string message = Uri.UnescapeDataString(uri.Substring(8)).Trim('"');
         MessageBox.Show(message, "Quick Info");
      }

      // 4. Standard Web Links
      else if (uri.StartsWith("http"))
      {
         Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
      }

      e.Handled = true; // Prevents the system from trying to "launch" the URI
   }
}