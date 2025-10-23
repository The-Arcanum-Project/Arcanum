using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.CommandSystem;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.UI.Components.UserControls.BaseControls;
using Arcanum.UI.NUI.Nui2.Nui2Gen.NavHistory;
using Nexus.Core;

namespace Arcanum.UI.NUI.Nui2.Nui2Gen;

public static class CustomShortInfoGenerators
{
   public static FrameworkElement GetModValInstanceShortInfo(object value, Enum nxProp, int height, int fontSize)
   {
      if (value is not ModValInstance instance)
         return new TextBlock
         {
            Text = "??",
            Height = height,
            FontSize = fontSize,
            Margin = new(0),
            FontFamily = (FontFamily)Application.Current.FindResource("DefaultMonospacedFont")!,
         };

      ModValViewer viewer = new(instance)
      {
         Height = height,
         FontSize = fontSize,
         Margin = new(0),
         FontFamily = (FontFamily)Application.Current.FindResource("DefaultMonospacedFont")!,
      };
      return viewer;
   }

   public static DockPanel GenerateEu5ShortInfo(NavH navH,
                                                IEu5Object primary,
                                                Enum sNxProp,
                                                int height,
                                                int fontSize,
                                                int leftMargin,
                                                int topMargin)
   {
      var stackPanel = new DockPanel
      {
         Margin = new(0),
         MinHeight = height,
         LastChildFill = true,
      };

      var sb = new System.Text.StringBuilder();

      foreach (var nxProp in primary.NUISettings.ShortInfoFields)
      {
         var itemType = primary.GetNxItemType(nxProp);
         object value = null!;
         Nx.ForceGet(primary, nxProp, ref value);

         if (itemType == null)
            sb.Append(value);
         else if (value is IEnumerable enumerable and not string)
         {
            var count = enumerable.Cast<object>().Count();
            if (count == 0)
               continue;

            sb.Append(nxProp).Append(": ").Append(count);
         }

         sb.Append("; ");
      }

      var headerBlock =
         GridManager.GetNavigationHeader(primary,
                                         navH,
                                         $"({sNxProp.ToString()}) {primary.UniqueId}",
                                         fontSize,
                                         height,
                                         true);
      headerBlock.Margin = new(6, 0, 0, 0);

      var dashBlock = ControlFactory.GetDashBlock(fontSize);

      var infoBlock =
         ControlFactory.GetHeaderTextBlock(fontSize, false, sb.ToString().TrimEnd(' ', ';'), height: height);
      infoBlock.TextTrimming = TextTrimming.CharacterEllipsis;
      infoBlock.TextWrapping = TextWrapping.NoWrap;
      infoBlock.HorizontalAlignment = HorizontalAlignment.Left;

      Thickness margin = new(leftMargin, topMargin, 0, 0);
      headerBlock.Margin = margin;
      dashBlock.Margin = margin;
      infoBlock.Margin = margin;

      stackPanel.Children.Add(headerBlock);
      stackPanel.Children.Add(dashBlock);
      DockPanel.SetDock(headerBlock, Dock.Left);
      DockPanel.SetDock(dashBlock, Dock.Left);
      stackPanel.Children.Add(infoBlock);

      return stackPanel;
   }
}