using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.Map.ToolTip;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.UI.Util;

/// <summary>
/// A manager class that uses a fluent builder pattern to construct a complex tooltip UI element.
/// </summary>
public static class ToolTipBuilder
{
   private static ToolTipLineSegment GetDefaultLineSegment(Location location)
      => new($"Location: {location.UniqueId} (<MISSING_LOC>)", ToolTipObjectType.Text) { IsBold = true, };

   private static ToolTipLineSegment GetDefaultMapModeLineSegment(string text) => new(text, ToolTipObjectType.Text);

   private static ToolTipLineSegment GetSeparatorLine() => new(string.Empty, ToolTipObjectType.Separator);

   public static List<ToolTipLineSegment> CreateToolTipSegments(Location location)
   {
      var segments = new List<ToolTipLineSegment> { GetDefaultLineSegment(location), GetSeparatorLine() };
      foreach (var mmttl in MapModeManager.GetCurrent().GetTooltip(location))
         segments.Add(GetDefaultMapModeLineSegment(mmttl));
      return segments;
   }

   public static FrameworkElement CreateContent(List<ToolTipLineSegment> segments)
   {
      var mainPanel = new StackPanel
      {
         Orientation = Orientation.Vertical,
         UseLayoutRounding = true,
         SnapsToDevicePixels = true,
      };

      foreach (var segment in segments)
         switch (segment.SegmentType)
         {
            case ToolTipObjectType.Text:
               mainPanel.Children.Add(CreateTextBlock(segment));
               break;
            case ToolTipObjectType.Separator:
               mainPanel.Children.Add(CreateSeparator());
               break;
            case ToolTipObjectType.Icon:
               mainPanel.Children.Add(CreateIconTextLine(segment));
               break;
            default:
               throw new ArgumentOutOfRangeException(nameof(segment.SegmentType), segment.SegmentType, null);
         }

      return mainPanel;
   }

   // Private helpers for creating specific UI elements.
   private static TextBlock CreateTextBlock(ToolTipLineSegment segment)
   {
      var textBlock = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new(0) };
      textBlock.Inlines.Add(segment.ToRun()); // Assuming one segment per line for now
      return textBlock;
   }

   private static Separator CreateSeparator()
   {
      return new() { Margin = new(0, 4, 0, 4) };
   }

   private static StackPanel CreateIconTextLine(ToolTipLineSegment segment)
   {
      var linePanel = new StackPanel
      {
         Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center,
      };
      if (!string.IsNullOrEmpty(segment.IconSource))
      {
         var icon = new Image
         {
            Source = new BitmapImage(new(segment.IconSource, UriKind.RelativeOrAbsolute)),
            Width = 16,
            Height = 16,
            Margin = new(0, 0, 5, 0),
         };

         linePanel.Children.Add(icon);
      }

      var textBlock = new TextBlock();
      textBlock.Inlines.Add(segment.ToRun());

      linePanel.Children.Add(textBlock);
      return linePanel;
   }
}