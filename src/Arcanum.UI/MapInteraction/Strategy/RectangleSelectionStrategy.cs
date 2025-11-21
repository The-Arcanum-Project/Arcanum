using System.Drawing;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.UI.Components.UserControls;
using Point = System.Windows.Point;

namespace Arcanum.UI.MapInteraction.Strategy;

public class RectangleSelectionStrategy(MapInteractionManager mapInteractionManager) : IMapInteractionStrategy
{
   private MapControl _map = null!;
   private bool _hasMoved;
   private Point _lastMousePosition;

   public void Enter(MapControl map)
   {
      _map = map;
      _hasMoved = false;
      _lastMousePosition = new();
   }

   public void Exit(MapControl map)
   {
      if (!_hasMoved)
         return;

      Selection.DragArea = RectangleF.Empty;
      Selection.DragPath.Clear();
      if (_map.LocationRenderer.ClearSelectionOutline())
         _map.LocationRenderer.Render();
   }

   public void OnMouseDown(MapControl map, MouseButtonEventArgs e)
   {
      if (e.ChangedButton != MouseButton.Left)
         return;

      _lastMousePosition = e.GetPosition(map.HwndHostContainer);
   }

   public void OnMouseMove(MapControl map, MouseEventArgs e)
   {
      var currentPos = e.GetPosition(map.HwndHostContainer);
      var deltaX = currentPos.X - _lastMousePosition.X;
      var deltaY = currentPos.Y - _lastMousePosition.Y;

      if (!_hasMoved)
      {
         if (Math.Abs(deltaX) < SystemParameters.MinimumHorizontalDragDistance &&
             Math.Abs(deltaY) < SystemParameters.MinimumVerticalDragDistance)
            return;

         _hasMoved = true;
         Selection.StartRectangleSelection(_map.Coords.CurrentPosition.Map);
      }

      Selection.UpdateDragSelection(_map.Coords.CurrentPosition.Map, true, false);
      SetSelectionRectangle();
   }

   public void OnMouseUp(MapControl map, MouseButtonEventArgs e)
   {
      var isControlPressed = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
      var isShiftPressed = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

      if (!_hasMoved)
      {
         if (!Selection.GetLocation(_map.Coords.CurrentPosition.Map, out var location1))
            return;

         // TODO: @MelCo: In the future make Control only deselect
         if (isShiftPressed)
            Selection.Modify(SelectionTarget.Selection,
                             SelectionMethod.Simple,
                             [location1],
                             true);
         else if (isControlPressed)
            Selection.Modify(SelectionTarget.Selection,
                             SelectionMethod.Simple,
                             [location1],
                             false,
                             false);
         else // TODO: @MelCo: In the future make a select if there are multiple selected not inverted -> not intuitive
            Selection.Modify(SelectionTarget.Selection,
                             SelectionMethod.Simple,
                             [location1],
                             true,
                             false,
                             true);
         return;
      }

      if (_map.LocationRenderer.ClearSelectionOutline())
         _map.LocationRenderer.Render();
      Selection.EndRectangleSelection(_map.CurrentPos, isControlPressed, !isControlPressed && !isShiftPressed);
   }

   public void OnMouseWheel(MapControl map, MouseWheelEventArgs e)
   {
   }

   private void SetSelectionRectangle()
   {
      // In map coordinates
      var upperLeft = Selection.DragPath.First();
      var lowerRight = Selection.DragPath.Last();

      //TODO: @Melco Optimize this to cache the data and do not instantiate new arrays every frame
      var topLeftNdc = _map.Coords.MapToNdc(new(upperLeft.X, upperLeft.Y));
      var bottomRightNdc = _map.Coords.MapToNdc(new(lowerRight.X, lowerRight.Y));

      Vector2[] rectangleNdc =
      [
         new(topLeftNdc.X, topLeftNdc.Y), new(bottomRightNdc.X, topLeftNdc.Y),
         new(bottomRightNdc.X, bottomRightNdc.Y), new(topLeftNdc.X, bottomRightNdc.Y),
         new(topLeftNdc.X, topLeftNdc.Y)
      ];

      _map.LocationRenderer.UpdateSelectionOutline(rectangleNdc, false);
      _map.LocationRenderer.Render();
      // Convert to NDC coordinates
   }
}