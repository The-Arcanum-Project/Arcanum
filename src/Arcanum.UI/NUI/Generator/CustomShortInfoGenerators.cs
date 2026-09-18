#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.UI.Components.UserControls.BaseControls;
using Arcanum.UI.NUI.Generator.StructConverters;

#endregion

namespace Arcanum.UI.NUI.Generator;

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

      var multiBinding = new MultiBinding
      {
         Mode = BindingMode.OneWay,
      };
      multiBinding.Bindings.Add(new Binding { Source = primary });

      multiBinding.Bindings.Add(new Binding(sNxProp.ToString())
      {
         Source = primary, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
      });
      multiBinding.Converter = new Eu5ShortInfoConverter();

      var headerBlock =
         GridManager.GetNavigationHeader(navH,
                                         sNxProp,
                                         $"({sNxProp.ToString()}) {primary.UniqueId}",
                                         fontSize,
                                         height,
                                         true);
      headerBlock.SetBinding(FrameworkElement.TagProperty, new Binding { Source = primary });
      headerBlock.Margin = new(6, 0, 0, 0);

      var dashBlock = ControlFactory.GetDashBlock(fontSize);

      var infoBlock = ControlFactory.GetHeaderTextBlock(fontSize, false, string.Empty, height: height);
      infoBlock.SetBinding(TextBlock.TextProperty, multiBinding);
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