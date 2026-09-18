#region

using System.Globalization;
using System.Windows.Data;
using Arcanum.UI.AppFeatures;
using Arcanum.UI.Documentation.Implementation;

#endregion

namespace Arcanum.UI.Components.Windows.HelpWindow.ViewModels;

public class FeatureDocToLocationStringConverter : IValueConverter
{
   public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is not FeatureDoc feature)
         return "Undefined reference in Converter";

      var scaleText = feature.Scale switch
      {
         FeatureScale.Compact => "A compact component",
         FeatureScale.Standard => "A standard-sized panel",
         FeatureScale.Major => "A large feature",
         FeatureScale.Full => "A full-screen view",
         _ => "A feature",
      };

      var locationText = feature.Location switch
      {
         FeatureLocation.Center => "in the center of the screen",
         FeatureLocation.Top => "at the top",
         FeatureLocation.TopRight => "in the top-right corner",
         FeatureLocation.Right => "on the right side",
         FeatureLocation.BottomRight => "in the bottom-right corner",
         FeatureLocation.Bottom => "at the bottom",
         FeatureLocation.BottomLeft => "in the bottom-left corner",
         FeatureLocation.Left => "on the left side",
         FeatureLocation.TopLeft => "in the top-left corner",
         FeatureLocation.Floating => "appearing as a floating popup",
         FeatureLocation.Contextual => "appearing contextually where needed",
         _ => "on the screen",
      };

      return $"{scaleText} {locationText}.";
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}