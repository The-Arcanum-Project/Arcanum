using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.UI.NUI.Nui2.Nui2Gen.NavHistory;

namespace Arcanum.UI.NUI.Nui2.Nui2Gen;

public static class GridManager
{
   private const int DEFAULT_ROW_HEIGHT = 27;
   private const int DEFAULT_HEADER_ROW_HEIGHT = 35;
   private const int DEFAULT_TOP_MARGIN = 3;
   private const int DEFAULT_BOTTOM_MARGIN = 0;
   private const int DEFAULT_LEFT_MARGIN = 0;
   private const int DEFAULT_RIGHT_MARGIN = 0;

   public static void AddToGrid(Grid mainGrid,
                                FrameworkElement element,
                                int row,
                                int column,
                                int columnSpan = 0,
                                int rowHeight = DEFAULT_ROW_HEIGHT)
   {
      mainGrid.Children.Add(element);

      while (row >= mainGrid.RowDefinitions.Count)
         mainGrid.RowDefinitions.Add(new() { Height = new(rowHeight, GridUnitType.Auto) });

      element.Margin = new(DEFAULT_LEFT_MARGIN, DEFAULT_TOP_MARGIN, DEFAULT_RIGHT_MARGIN, DEFAULT_BOTTOM_MARGIN);

      Grid.SetRow(element, row);
      Grid.SetColumn(element, column);
      if (columnSpan > 0)
         Grid.SetColumnSpan(element, columnSpan);
   }

   private static void SetHeaderInternal(Grid mainGrid,
                                         string headerText,
                                         int row,
                                         int column,
                                         int columnSpan,
                                         int defaultRowHeight,
                                         Action<TextBlock>? configureHeader = null)
   {
      var header = ControlFactory.GetHeaderTextBlock(18, false, headerText);

      configureHeader?.Invoke(header);

      AddToGrid(mainGrid, header, row, column, columnSpan, defaultRowHeight);
   }

   public static void SetPureHeader(Grid mainGrid,
                                    IEu5Object primary,
                                    int targetsCount,
                                    int row,
                                    int column,
                                    int columnSpan = 0)
   {
      var headerText = targetsCount > 1 ? $"{primary.GetType().Name} ({targetsCount})" : primary.UniqueId;
      SetHeaderInternal(mainGrid, headerText, row, column, columnSpan, DEFAULT_HEADER_ROW_HEIGHT);
   }

   public static TextBlock GetNavigationHeader(IEu5Object primary,
                                               NavH navH,
                                               string headerText,
                                               int fontSize,
                                               int height,
                                               bool isNavigation,
                                               HorizontalAlignment alignment = HorizontalAlignment.Center,
                                               bool pureHeader = false)

   {
      var header = pureHeader
                      ? ControlFactory.PureHeaderTextBlock(isNavigation)
                      : ControlFactory.GetHeaderTextBlock(fontSize,
                                                          true,
                                                          headerText,
                                                          height: height,
                                                          alignment: alignment);
      if (isNavigation)
      {
         EventHandlers.SetOnMouseUpHandler(header, navH, primary);
         header.Cursor = System.Windows.Input.Cursors.Hand;
      }

      return header;
   }
}