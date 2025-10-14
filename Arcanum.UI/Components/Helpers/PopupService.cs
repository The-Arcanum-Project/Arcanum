using System.Windows;
using System.Windows.Media;
using Arcanum.UI.Components.UserControls.BaseControls;
using Arcanum.UI.Components.Windows.MinorWindows;

namespace Arcanum.UI.Components.Helpers;

public struct PopupResult<T>
{
   public bool Confirmed;
   public T Value;
}

public static class PopupService
{
   public static PopupResult<Color> ShowColorPicker(Color initialColor, Point position)
   {
      var colorPicker = new ColorPicker { SelectedColor = initialColor };

      var shell = new PopUpShell
      {
         DialogTitle = "Select a Color",
         HostedContent = colorPicker,
         WindowStartupLocation = WindowStartupLocation.Manual,
         Left = position.X,
         Top = position.Y,
         SizeToContent = SizeToContent.WidthAndHeight,
         ResizeMode = ResizeMode.NoResize,
         WindowStyle = WindowStyle.None,
      };

      var dialogResult = shell.ShowDialog();

      if (dialogResult == true)
         return new() { Confirmed = true, Value = colorPicker.SelectedColor };

      return new()
      {
         Confirmed = false, Value = initialColor,
      };
   }
}