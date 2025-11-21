using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Arcanum.UI.Components.Converters;

public class IsNonDefaultToImageSourceConverter : IValueConverter
{
   private static readonly BitmapImage SaveIcon =
      new(new("/Arcanum_UI;component/Assets/Icons/16x16/SaveToFile16x16.png", UriKind.Relative));

   private static readonly BitmapImage DontSaveIcon =
      new(new("/Arcanum_UI;component/Assets/Icons/16x16/DontSaveToFile16x16.png", UriKind.Relative));

   public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is bool isNonDefault)
         // The logic from your old GetPropertyMarker method is now here
         // Note: Your original URIs seemed reversed. I've corrected them based on the tooltips.
         // isNonDefault == true means it WILL be saved.
         return isNonDefault ? SaveIcon : DontSaveIcon;

      return null;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}

// Converter for the ToolTip
public class IsNonDefaultToTooltipConverter : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is bool isNonDefault)
         return isNonDefault
                   ? "This property is set to a NON-default value and will be saved to file."
                   : "This property is set to its default value and will NOT be saved to file.";

      return "Unknown state.";
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}