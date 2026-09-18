#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Common.Logger;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Wpf;
using Markdig.Syntax.Inlines;

#endregion

namespace Arcanum.UI.Documentation.Renderers;

public class CustomImageWpfRenderer(WpfObjectRenderer<LinkInline> defaultRenderer) : WpfObjectRenderer<LinkInline>
{
   protected override void Write(WpfRenderer renderer, LinkInline link)
   {
      if (!link.IsImage)
      {
         defaultRenderer.Write(renderer, link);
         return;
      }

      var image = new Image();

      try
      {
         if (link.Url != null)
            image.Source = new BitmapImage(new(link.Url, UriKind.Absolute));
      }
      catch
      {
         ArcLog.WriteLine("CIR", LogLevel.ERR, $"Failed to load image from URL: {link.Url}");
         return;
      }

      //![alt](url){.center width=200 height=100 scale=Uniform}
      var attributes = link.TryGetAttributes();
      if (attributes != null)
      {
         // Alignment
         if (attributes.Classes != null && attributes.Classes.Contains("center"))
            image.HorizontalAlignment = HorizontalAlignment.Center;
         else if (attributes.Classes != null && attributes.Classes.Contains("right"))
            image.HorizontalAlignment = HorizontalAlignment.Right;
         else if (attributes.Classes != null && attributes.Classes.Contains("left"))
            image.HorizontalAlignment = HorizontalAlignment.Left;
         else if (attributes.Classes != null && attributes.Classes.Contains("stretch"))
            image.HorizontalAlignment = HorizontalAlignment.Stretch;

         // Size & Scale
         if (attributes.Properties != null)
         {
            // Width
            var widthProp = attributes.Properties.FirstOrDefault(p => p.Key.Equals("width", StringComparison.OrdinalIgnoreCase));
            if (widthProp.Key != null && double.TryParse(widthProp.Value, out var w))
               image.Width = w;

            // Height
            var heightProp = attributes.Properties.FirstOrDefault(p => p.Key.Equals("height", StringComparison.OrdinalIgnoreCase));
            if (heightProp.Key != null && double.TryParse(heightProp.Value, out var h))
               image.Height = h;

            // Scale (Maps to WPF's System.Windows.Media.Stretch)
            var scaleProp = attributes.Properties.FirstOrDefault(p => p.Key.Equals("scale", StringComparison.OrdinalIgnoreCase));
            if (scaleProp.Key != null && Enum.TryParse<Stretch>(scaleProp.Value, true, out var stretchValue))
               image.Stretch = stretchValue;
            else
               image.Stretch = Stretch.Uniform;
         }
      }

      renderer.Push(new InlineUIContainer(image));
      renderer.Pop();
   }
}