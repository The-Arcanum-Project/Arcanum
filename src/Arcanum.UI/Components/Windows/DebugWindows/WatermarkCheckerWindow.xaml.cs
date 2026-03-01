using System.Drawing;
using System.IO;
using System.Windows;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.Utils.Imagery;
using Arcanum.UI.Components.Windows.PopUp;
using Brushes = System.Windows.Media.Brushes;

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class WatermarkCheckerWindow
{
   public WatermarkCheckerWindow()
   {
      InitializeComponent();
   }

   private void OnBrowseClick(object sender, RoutedEventArgs e)
   {
      var file = IO.SelectFile(IO.GetArcanumDataPath, "Lossless Images|*.png;*.bmp;*.tiff|PNG Files|*.png|All Files|*.*");
      if (file != null)
      {
         FilePathTxt.Text = file;
         ResultTxt.Text = "Ready to check.";
         ResultTxt.Foreground = Brushes.Gray;
      }
   }

   private void OnCheckClick(object sender, RoutedEventArgs e)
   {
      var path = FilePathTxt.Text;
      if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
      {
         MBox.Show("Please select a valid file first.");
         return;
      }

      try
      {
         using var bmp = new Bitmap(path);
         var tag = ImageTagger.ReadTag(bmp);
         if (!string.IsNullOrEmpty(tag))
         {
            ResultTxt.Text = $"FOUND: \"{tag}\"";
            ResultTxt.Foreground = Brushes.LimeGreen;
         }
         else
         {
            ResultTxt.Text = "No watermark detected.";
            ResultTxt.Foreground = Brushes.OrangeRed;
         }
      }
      catch (Exception ex)
      {
         ResultTxt.Text = "Error reading file.";
         MBox.Show($"Could not read image data.\n{ex.Message}");
      }
   }
}