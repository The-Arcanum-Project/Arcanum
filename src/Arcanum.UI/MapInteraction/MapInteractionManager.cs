using System.Windows.Input;
using Arcanum.UI.Components.UserControls.Map;
using Arcanum.UI.MapInteraction.Strategy;
using Common.Logger;

namespace Arcanum.UI.MapInteraction;

public class MapInteractionManager
{
   private readonly MapControl _map;
   public readonly MapNavigationStrategy NavigationStrategy;
   public readonly RectangleSelectionStrategy RectangleSelectionStrategy;
   public readonly LassoSelectionStrategy LassoSelectionStrategy = new ();
   public readonly BrushSelectionStrategy BrushSelectionStrategy = new ();

   private IMapInteractionStrategy _activeStrategy;

   public MapInteractionManager(MapControl map)
   {
      _map = map;
      NavigationStrategy = new (this);
      RectangleSelectionStrategy = new ();
      _activeStrategy = NavigationStrategy;
   }

   public void SwitchToStrategy(IMapInteractionStrategy newStrategy)
   {
      if (_activeStrategy.Equals(newStrategy))
         return;

      _activeStrategy.Exit(_map);
      _activeStrategy = newStrategy;
      _activeStrategy.Enter(_map);

      ArcLog.WriteLine("MAP", LogLevel.DBG, "Switched to strategy: " + newStrategy.GetType().Name);
   }

   public void HandleKeyDown(object _, KeyEventArgs e)
   {
      switch (e.Key)
      {
         case Key.Escape:
            SwitchToStrategy(NavigationStrategy);
            break;
         case Key.C:
            SwitchToStrategy(_activeStrategy is BrushSelectionStrategy ? NavigationStrategy : BrushSelectionStrategy);
            break;
      }
   }

   public void HandleKeyUp()
   {
   }

   public void HandleMouseDown(MouseButtonEventArgs e)
   {
      if (_activeStrategy is MapNavigationStrategy && e.ChangedButton == MouseButton.Left)
         if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            SwitchToStrategy(LassoSelectionStrategy);
         else
            SwitchToStrategy(RectangleSelectionStrategy);

      _activeStrategy.OnMouseDown(_map, e);
   }

   public void HandleMouseMove(MouseEventArgs e)
   {
      _activeStrategy.OnMouseMove(_map, e);
   }

   public void HandleMouseUp(MouseButtonEventArgs e)
   {
      _activeStrategy.OnMouseUp(_map, e);
      if (_activeStrategy is not Strategy.BrushSelectionStrategy && e.ChangedButton == MouseButton.Left)
         SwitchToStrategy(NavigationStrategy);
   }

   public void HandleMouseWheel(MouseWheelEventArgs e)
   {
      _activeStrategy.OnMouseWheel(_map, e);
   }
}