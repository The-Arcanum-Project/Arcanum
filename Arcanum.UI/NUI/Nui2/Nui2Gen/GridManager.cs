using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.UI.NUI.Nui2.Nui2Gen.NavHistory;

namespace Arcanum.UI.NUI.Nui2.Nui2Gen;

public static class GridManager
{
   private const int DEFAULT_ROW_HEIGHT = 27;
   private const int DEFAULT_HEADER_ROW_HEIGHT = 35;

   public static void AddToGrid(Grid mainGrid,
                                UIElement element,
                                int row,
                                int column,
                                int columnSpan = 0,
                                int rowHeight = DEFAULT_ROW_HEIGHT)
   {
      mainGrid.Children.Add(element);

      while (row >= mainGrid.RowDefinitions.Count)
         mainGrid.RowDefinitions.Add(new() { Height = new(rowHeight, GridUnitType.Auto) });

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
                                               bool isNavigation)
   {
      var header = ControlFactory.GetHeaderTextBlock(fontSize, true, headerText, height: height);
      if (isNavigation)
      {
         EventHandlers.SetOnMouseUpHandler(header, navH, primary);
         header.Cursor = System.Windows.Input.Cursors.Hand;
      }

      return header;
   }
}