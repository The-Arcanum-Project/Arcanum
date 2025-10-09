using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.NUI.Nui2.Nui2Gen.NavHistory;
using Arcanum.UI.NUI.UserControls.BaseControls;

namespace Arcanum.UI.NUI.Nui2.Nui2Gen;

public static class ControlFactory
{
   public const int SHORT_INFO_ROW_HEIGHT = 20;
   public const int SHORT_INFO_FONT_SIZE = 11;
   public const int EMBEDDED_VIEW_HEIGHT = 25;

   public static readonly Brush BlueBrush = (Brush)Application.Current.FindResource("BlueAccentColorBrush")!;
   private static readonly Brush ForegroundBrush = (Brush)Application.Current.FindResource("DefaultForeColorBrush")!;
   public static readonly Brush AccentBrush = (Brush)Application.Current.FindResource("LightAccentBackColorBrush")!;
   public static readonly Brush MarkedBrush = (Brush)Application.Current.FindResource("MarkedColorBrush")!;

   public static BaseView GetBaseView()
   {
      return new() { };
   }

   /// <summary>
   /// Has 3 Columns, with the 2nd one being a splitter.
   /// </summary>
   public static Grid GetMainGrid()
   {
      var grid = new Grid
      {
         ColumnDefinitions =
         {
            new() { Width = new(4, GridUnitType.Star) }, new() { Width = new(5, GridUnitType.Star) },
         },
      };

      return grid;
   }

   public static TextBlock GetHeaderTextBlock(int fontSize,
                                              bool isNavigationHeader,
                                              string headerText,
                                              FontWeight? fontWeight = null,
                                              int height = 35,
                                              HorizontalAlignment alignment = HorizontalAlignment.Center,
                                              int leftMargin = 4)
   {
      var header = new TextBlock
      {
         TextWrapping = TextWrapping.NoWrap,
         HorizontalAlignment = alignment,
         VerticalAlignment = VerticalAlignment.Center,
         TextAlignment = TextAlignment.Center,
         FontWeight = fontWeight ?? FontWeights.Bold,
         Margin = new(leftMargin, 0, 0, 0),
         FontSize = fontSize,
         MaxHeight = height,
         Height = double.NaN,
         Foreground = isNavigationHeader ? BlueBrush : ForegroundBrush,
         Text = headerText,
         TextTrimming = TextTrimming.CharacterEllipsis,
         SnapsToDevicePixels = true,
      };
      return header;
   }

   public static TextBlock PureHeaderTextBlock(bool isNavigationHeader)
   {
      return new()
      {
         FontSize = 12,
         Foreground = isNavigationHeader ? BlueBrush : ForegroundBrush,
         TextWrapping = TextWrapping.Wrap,
      };
   }

   public static ContextMenu GetContextMenu(INUINavigation?[] navs, NavH navH)
   {
      var contextMenu = new ContextMenu();

      foreach (var nav in navs)
      {
         if (nav == null)
         {
            contextMenu.Items.Add(new Separator());
            continue;
         }

         var menuItem = new MenuItem
         {
            Header = nav.ToolStripString, IsEnabled = nav.IsEnabled,
         };
         menuItem.Click +=
            (_, _) => navH.NavigateTo((IEu5Object)nav
                                        .Target
                                      !); // TODO: @Minnator remove this once we are using this new gen and have changed the interface 
         contextMenu.Items.Add(menuItem);
      }

      return contextMenu;
   }

   public static Grid GetCollectionPreviewGrid()
   {
      var grid = new Grid();

      for (var i = 0; i < Config.Settings.NUIConfig.MaxCollectionItemsPreviewed + 1; i++)
         grid.RowDefinitions.Add(new() { Height = new(SHORT_INFO_ROW_HEIGHT, GridUnitType.Pixel) });

      return grid;
   }

   public static TextBlock GetDashBlock(int fontSize)
   {
      var dashBlock = new TextBlock
      {
         Text = " — ",
         VerticalAlignment = VerticalAlignment.Top,
         FontSize = fontSize,
         Height = fontSize + 4,
      };
      return dashBlock;
   }

   public static Grid GetCollectionGrid()
   {
      var grid = new Grid { Margin = new(0), };

      grid.RowDefinitions.Add(new() { Height = new(30, GridUnitType.Pixel) });

      return grid;
   }
}