#region

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Markdig;
using Markdig.Extensions.Alerts;
using Markdig.Renderers;
using Markdig.Renderers.Wpf;

#endregion

namespace Arcanum.UI.Documentation.Renderers;

public class AlertBlockWpfRenderer : WpfObjectRenderer<AlertBlock>
{
   protected override void Write(WpfRenderer renderer, AlertBlock obj)
   {
      var section = new Section();
      var kind = obj.Kind.ToString().ToUpperInvariant();

      var (hexBorder, hexBg, icon) = kind switch
      {
         "NOTE" => ("#2F81F7", "#0D192B", "ℹ️ "),
         "TIP" => ("#3FB950", "#122B19", "💡 "),
         "IMPORTANT" => ("#A371F7", "#211736", "💬 "),
         "WARNING" => ("#D29922", "#312613", "⚠️ "),
         "CAUTION" => ("#F85149", "#361618", "🛑 "),
         _ => ("#8b949e", "#161b22", "📝 "),
      };

      section.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexBorder));
      section.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexBg));
      section.BorderThickness = new(4, 0, 0, 0);
      section.Padding = new(10, 8, 10, 8);
      section.Margin = new(0, 10, 0, 10);

      var headerPar = new Paragraph(new Run(icon + kind))
      {
         Foreground = section.BorderBrush,
         FontWeight = FontWeights.Bold,
         Margin = new(0, 0, 0, 5),
      };
      section.Blocks.Add(headerPar);

      renderer.Push(section);
      renderer.WriteChildren(obj);
      renderer.Pop();
   }
}

public class WpfAlertExtension : IMarkdownExtension
{
   public void Setup(MarkdownPipelineBuilder pipeline)
   {
   }

   public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
   {
      if (renderer is WpfRenderer wpfRenderer)
         wpfRenderer.ObjectRenderers.Insert(0, new AlertBlockWpfRenderer());
   }
}