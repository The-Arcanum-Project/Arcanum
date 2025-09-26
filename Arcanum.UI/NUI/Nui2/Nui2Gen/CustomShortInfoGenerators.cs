using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

   public static StackPanel GenerateEu5ShortInfo(NavH navH,
                                                 Enum nxProperty,
                                                 IEu5Object primary,
                                                 int height,
                                                 int fontSize)
   {
      var stackPanel = new StackPanel
      {
         Orientation = Orientation.Horizontal,
         Margin = new(0),
         MinHeight = height,
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
         GridManager.GetNavigationHeader(primary, navH, nxProperty.ToString(), fontSize, height, true);
      headerBlock.Margin = new(6, 0, 0, 0);

      var dashBlock = ControlFactory.GetDashBlock(fontSize);

      var infoBlock =
         ControlFactory.GetHeaderTextBlock(fontSize, false, sb.ToString().TrimEnd(' ', ';'), height: height);

      stackPanel.Children.Add(headerBlock);
      stackPanel.Children.Add(dashBlock);
      stackPanel.Children.Add(infoBlock);

      return stackPanel;
   }
}