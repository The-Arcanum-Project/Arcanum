using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.UserControls.Map;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

namespace Arcanum.UI.Util;

public class ToolTipManager
{
   private readonly ToolTip _mapToolTip;
   private Location _lastShownTooltipLocation = Location.Empty;
   private bool _suppressUntilNotice;

   public ToolTipManager()
   {
      _mapToolTip = new()
      {
         Focusable = false,
         Placement = PlacementMode.Relative,
         UseLayoutRounding = true,
         SnapsToDevicePixels = true,
         BorderThickness = new(1),
         Padding = new(2),
      };
   }

   public void SetUpToolTip(MapControl mainMap)
   {
      _mapToolTip.PlacementTarget = mainMap;

      mainMap.OnAbsoluteLocationChangedLocation += (curLoc, pos) =>
      {
         if (_suppressUntilNotice || !Config.Settings.MapSettings.ShowTooltips)
            return;

         if (curLoc == Location.Empty)
         {
            if (!_mapToolTip.IsOpen) return;
            _mapToolTip.IsOpen = false;
            _lastShownTooltipLocation = Location.Empty;

            return;
         }

         if (curLoc != _lastShownTooltipLocation || !_mapToolTip.IsOpen)
         {
            _mapToolTip.Content = ToolTipBuilder.CreateContent(ToolTipBuilder.CreateToolTipSegments(curLoc));
            _lastShownTooltipLocation = curLoc;
         }

         var curMousePos = Mouse.GetPosition(mainMap);
         _mapToolTip.HorizontalOffset = curMousePos.X + 15;
         _mapToolTip.VerticalOffset = curMousePos.Y + 15;
         _mapToolTip.IsOpen = true;
      };

      mainMap.MouseLeave += (_, _) =>
      {
         _mapToolTip.IsOpen = false;
         _lastShownTooltipLocation = Location.Empty;
         _suppressUntilNotice = false;
      };

      mainMap.MapInteractionManager.NavigationStrategy.OnPanningStarted += SuppressUntilNotice;
      mainMap.MapInteractionManager.NavigationStrategy.OnPanningEnded += ReenableToolTip;
   }

   public void SuppressUntilNotice()
   {
      if (!_mapToolTip.IsOpen)
         return;

      _mapToolTip.IsOpen = false;
      _lastShownTooltipLocation = Location.Empty;
      _suppressUntilNotice = true;
   }

   public void ReenableToolTip()
   {
      _suppressUntilNotice = false;
   }
}