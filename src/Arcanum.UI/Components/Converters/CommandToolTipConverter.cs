using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Arcanum.UI.Commands;
using Arcanum.UI.NUI.Generator;

namespace Arcanum.UI.Components.Converters;

public class CommandToolTipConverter : IMultiValueConverter
{
   private static readonly GestureToTextConverter GestureToTextConverter = new();

   public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
   {
      if (values[0] is not IAppCommand cmd)
         return Binding.DoNothing;

      var originalContent = values[1];

      var container = new StackPanel { MaxWidth = 450, Margin = new(2) };
      var headerBlock = new TextBlock { VerticalAlignment = VerticalAlignment.Center };

      headerBlock.Inlines.Add(new Run(cmd.DisplayName)
      {
         FontWeight = FontWeights.Bold, Foreground = ControlFactory.ForegroundBrush,
      });

      if (cmd.Gestures.Count > 0)
      {
         var gestureText = string.Join(", ",
                                       cmd.Gestures.Select(x =>
                                                              GestureToTextConverter.Convert(x, null!, null, CultureInfo.InvariantCulture)));

         headerBlock.Inlines.Add(new Run($"  {gestureText}")
         {
            FontFamily = ControlFactory.MonoFontFamily,
            FontSize = 12,
            Foreground = ControlFactory.LightContrastForeColorBrush,
         });
      }

      container.Children.Add(headerBlock);

      container.Children.Add(new Separator
      {
         Background = ControlFactory.DefaultBorderColorBrush,
         Opacity = 0.3,
         Margin = new(-2, 4, 0, 2),
         SnapsToDevicePixels = true,
      });

      if (originalContent is FrameworkElement fe)
      {
         switch (fe.Parent)
         {
            case ContentControl parent:
               parent.Content = null;
               break;
            case Panel panelParent:
               panelParent.Children.Remove(fe);
               break;
         }

         container.Children.Add(fe);
      }
      else
      {
         var bodyText = originalContent as string ?? cmd.Description;

         container.Children.Add(new TextBlock
         {
            Text = bodyText,
            TextWrapping = TextWrapping.Wrap,
            Foreground = ControlFactory.ForegroundBrush,
            FontSize = 12,
         });
      }

      return container;
   }

   public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
}