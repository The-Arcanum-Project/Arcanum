using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.UserControls;

namespace Arcanum.UI.Util;

public class ToolTipManager
{
   private readonly ToolTip _mapToolTip;
   private Location _lastShownTooltipLocation = Location.Empty;
   private bool _suppressUntilNotice = false;

   public ToolTipManager()
   {
      _mapToolTip = new()
      {
         Focusable = false,
         Placement = System.Windows.Controls.Primitives.PlacementMode.Relative,
         UseLayoutRounding = true,
         SnapsToDevicePixels = true,
         BorderThickness = new(1),
         Padding = new(2),
      };
   }

   public void SetUpToolTip(MapControl mainMap)
   {
      _mapToolTip.PlacementTarget = mainMap;

      mainMap.OnAbsolutePositionChanged += pos =>
      {
         if (_suppressUntilNotice || !Config.Settings.MapSettings.ShowTooltips)
            return;

         if (!Selection.GetLocation(pos, out var curLoc))
         {
            if (_mapToolTip.IsOpen)
            {
               _mapToolTip.IsOpen = false;
               _lastShownTooltipLocation = Location.Empty;
            }

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

      mainMap.OnPanningStarted += SuppressUntilNotice;
      mainMap.OnPanningEnded += ReenableToolTip;
   }

   public void SuppressUntilNotice()
   {
      _mapToolTip.IsOpen = false;
      _lastShownTooltipLocation = Location.Empty;
      _suppressUntilNotice = true;
   }

   public void ReenableToolTip()
   {
      _suppressUntilNotice = false;
   }
}