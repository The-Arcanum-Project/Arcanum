#region

using System.IO;
using System.Windows;
using Common.Logger;
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

#endregion

namespace Arcanum.UI.Documentation.Renderers;

public class WpfImageResourceExtension : IMarkdownExtension
{
   private const string BASE_RESOURCE_PATH = "pack://application:,,,/Arcanum_UI;component/Documentation/DocuPages/Images/";

   public void Setup(MarkdownPipelineBuilder pipeline)
   {
      pipeline.DocumentProcessed += RewriteImageUrls;
   }

   public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
   {
   }

   private static void RewriteImageUrls(MarkdownDocument document)
   {
      foreach (var node in document.Descendants())
         if (node is LinkInline { IsImage: true } link && !string.IsNullOrEmpty(link.Url))
            if (!link.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
                !link.Url.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
            {
               var targetUriString = $"{BASE_RESOURCE_PATH}{link.Url}";

               if (ResourceExists(targetUriString))
                  link.Url = targetUriString;
               else
               {
                  ArcLog.WriteLine("WRE", LogLevel.ERR, $"Markdown Image Not Found: '{link.Url}'. Expected at: {targetUriString}");
                  link.Url = string.Empty;
               }
            }
   }

   private static bool ResourceExists(string packUriString)
   {
      try
      {
         var uri = new Uri(packUriString, UriKind.Absolute);
         var streamInfo = Application.GetResourceStream(uri);
         return streamInfo != null;
      }
      catch (IOException)
      {
         return false;
      }
      catch (Exception ex)
      {
         ArcLog.WriteLine("WRE", LogLevel.ERR, $"Malformed Image URI: {packUriString}", ex);
         return false;
      }
   }
}