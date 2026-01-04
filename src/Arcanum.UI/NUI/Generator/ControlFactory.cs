using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.Clipboard;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.NUI.UserControls.BaseControls;

namespace Arcanum.UI.NUI.Generator;

public static class ControlFactory
{
   public const int SHORT_INFO_ROW_HEIGHT = 20;
   public const int SHORT_INFO_FONT_SIZE = 11;
   public const int EMBEDDED_VIEW_HEIGHT = 25;

   public static readonly Brush BlueBrush = (Brush)Application.Current.FindResource("BlueAccentColorBrush")!;
   public static readonly Brush ForegroundBrush = (Brush)Application.Current.FindResource("DefaultForeColorBrush")!;
   public static readonly Brush AccentBrush = (Brush)Application.Current.FindResource("LightAccentBackColorBrush")!;
   public static readonly Brush MarkedBrush = (Brush)Application.Current.FindResource("MarkedColorBrush")!;
   public static readonly Brush BackColorBrush = (Brush)Application.Current.FindResource("DefaultBackColorBrush")!;

   public static readonly FontFamily MonoFontFamily = (FontFamily)Application.Current.FindResource("DefaultMonospacedFont")!;

   public static BaseView GetBaseView()
   {
      return new();
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

   public static ContextMenu GetContextMenu(INUINavigation?[] navs, NavH navH, Enum nxProp)
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
         // TODO: @Minnator remove this once we are using this new gen and have changed the interface 
         RoutedEventHandler navigationHandler = (_, _) => navH.NavigateTo((IEu5Object)nav.Target!);
         menuItem.Click += navigationHandler;

         menuItem.Unloaded += OnMenuItemOnUnloaded;
         contextMenu.Items.Add(menuItem);
         continue;

         void OnMenuItemOnUnloaded(object o, RoutedEventArgs routedEventArgs)
         {
            menuItem.Click -= navigationHandler;
            menuItem.Unloaded -= OnMenuItemOnUnloaded;
         }
      }

      // We add the copy and paste options here as well
      contextMenu.Items.Add(new Separator());
      var copyItem = new MenuItem { Header = "Copy", };
      RoutedEventHandler copyHandler = (_, _) =>
      {
         if (navH.Targets is [{ } eu5Object])
            ArcClipboard.Copy(eu5Object, nxProp);
      };

      var pasteItem = new MenuItem
      {
         Header = "Paste", IsEnabled = navH.Targets is [{ } target] && ArcClipboard.CanPaste(target),
      };
      RoutedEventHandler pasteHandler = (_, _) =>
      {
         foreach (var iEu5Object in navH.Targets)
         {
            if (ArcClipboard.CanPaste(iEu5Object, nxProp))
            {
               ArcClipboard.Paste(iEu5Object, nxProp);
               NUINavigation.Instance.ForceInvalidateUi();
            }
         }
      };

      copyItem.Click += copyHandler;
      pasteItem.Click += pasteHandler;
      copyItem.Unloaded += OnCopyItemOnUnloaded;
      pasteItem.Unloaded += OnPasteItemOnUnloaded;
      contextMenu.Items.Add(copyItem);
      contextMenu.Items.Add(pasteItem);

      return contextMenu;

      void OnCopyItemOnUnloaded(object o, RoutedEventArgs routedEventArgs)
      {
         copyItem.Click -= copyHandler;
         copyItem.Unloaded -= OnCopyItemOnUnloaded;
      }

      void OnPasteItemOnUnloaded(object o, RoutedEventArgs routedEventArgs)
      {
         pasteItem.Click -= pasteHandler;
         pasteItem.Unloaded -= OnPasteItemOnUnloaded;
      }
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