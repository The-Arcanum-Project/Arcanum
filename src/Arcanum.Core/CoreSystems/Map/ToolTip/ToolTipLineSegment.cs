using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;

namespace Arcanum.Core.CoreSystems.Map.ToolTip;

public class ToolTipLineSegment(string text, ToolTipObjectType segmentType)
{
   public ToolTipObjectType SegmentType { get; set; } = segmentType;
   public string Text { get; set; } = text;
   public Brush? TextBrush { get; set; }
   public Color? BackgroundColor { get; set; }
   public Font? Font { get; set; }
   public int FontSize { get; set; } = 12;
   public bool IsBold { get; set; }
   public bool IsUnderline { get; set; }
   public bool IsItalic { get; set; }
   public string? HyperlinkTarget { get; set; }
   public string? IconSource { get; set; }

   // This method converts the ToolTipLineSegment into a Run for WPF TextBlock
   public System.Windows.Documents.Run ToRun()
   {
      var run = new System.Windows.Documents.Run(Text) { FontSize = FontSize };
      if (TextBrush != null)
         run.Foreground = TextBrush;
      if (Font != null)
      {
         run.FontFamily = new(Font.Name);
         run.FontSize = Font.Size;
      }

      if (IsBold)
         run.FontWeight = System.Windows.FontWeights.Bold;
      if (IsItalic)
         run.FontStyle = System.Windows.FontStyles.Italic;
      if (IsUnderline)
      {
         var textDecoration = new System.Windows.TextDecoration
         {
            Location = System.Windows.TextDecorationLocation.Underline,
         };
         var textDecorationCollection = new System.Windows.TextDecorationCollection { textDecoration };
         run.TextDecorations = textDecorationCollection;
      }

      return run;
   }
}