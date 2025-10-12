using System.Diagnostics;
using System.Numerics;
using Arcanum.Core.CoreSystems.Map;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.Utils.Geometry;
using Timer = System.Threading.Timer;

namespace Arcanum.Core.CoreSystems.Selection;

public static class Selection
{
   static Selection()
   {
      TimedSelectionTimer = new(Callback);
   }

   /*
    * ALL SELECTION KEYS ANS OPTIONS
    *
    * ## SIMPLE SELECTION
    * x LMB: Select single location
    * x Ctrl + LMB: Add/remove single location to/from selection
    * x ALT + LMB: Expand selection to next parent scope (location -> province -> area -> region -> superregion -> continent)
    * x ALT + Ctrl + LMB: Shrink selection to next child scope (continent -> superregion -> region -> area -> province -> location)
    *
    * RMB: Context menu with quick selections (flood-filled) and custom user defined selection filters (e.g. all coastal locations with a harbor)
    * Ctrl + RMB: Selection modification menu (e.g. grow/shrink selection by, invert selection, select all of type, select none)
    * ALT + RMB: Popup to select an option to magic wand select locations
    *
    * ## COMPLEX SELECTION (requires holding down of buttons and moving the mouse)
    *
    * Ctrl + LMB + Drag: Rectangle select (add to selection if not selected or remove from selection if selected)
    * ALT + LMB + Drag: Lasso select (add to selection if not selected or remove from selection if selected)
    */

   // Used to e.g. make a province flash for a short time when it is selected via a script or event
   private static List<TimedSelection> TimedSelections { get; } = [];
   private static Timer TimedSelectionTimer { get; set; }

   private static HashSet<Location> SelectedLocations { get; } = [];
   private static HashSet<Location> HoveredLocations { get; } = [];
   private static HashSet<Location> HighlightedLocations { get; } = [];
   private static HashSet<Location> SelectionPreview { get; } = [];

   // List to keep alive to reduce allocations when adding/removing multiple locations at once or via drag selection
   private static List<Location> AddCache { get; } = [];
   private static List<Location> RemoveCache { get; } = [];

   // State Variables for drag selection
   public static bool IsDragging { get; set; }
   public static List<Vector2> DragPath { get; set; } = [];
   private static RectangleF DragArea { get; set; } = RectangleF.Empty;

   private static QuadTree QuadTree { get; set; } =
      new(new(0, 0, 10000, 10000)); // Placeholder, will be set by Map system

   #region Convenience Getter

   public static List<Location> GetSelectedLocations => SelectedLocations.ToList();
   public static List<Location> GetHoveredLocations => HoveredLocations.ToList();
   public static List<Location> GetHighlightedLocations => HighlightedLocations.ToList();

   #endregion

   #region General Modify Methods

   /// <summary>
   /// Adds or removes locations from the specified selection target. <br/>
   /// If <c>additive</c> is true, locations will be added to the selection if they are not already present. <br/>
   /// If <c>additive</c> is false, locations will be removed from the selection if they are present. <br/>
   /// If <c>invert</c> is true, locations that are already present in the selection will be removed instead of added,
   /// and locations that are not present will be added instead of removed. <br/>
   /// </summary>
   public static void Modify(SelectionTarget target,
                             IEnumerable<Location> locations,
                             bool additive = false,
                             bool invert = true)
   {
      var targetSet = GetTarget(target);

      // in each branch respectively we gather the toAdd and toRemove lists
      if (additive)
         foreach (var loc in locations)
            if (targetSet.Contains(loc))
            {
               if (invert)
                  RemoveCache.Add(loc);
            }
            else
               AddCache.Add(loc);
      else
         foreach (var loc in locations)
            if (targetSet.Contains(loc))
               RemoveCache.Add(loc);
            else
            {
               if (invert)
                  AddCache.Add(loc);
            }

      AddTo(target, AddCache);
      RemoveFrom(target, RemoveCache);
   }

   public static void Set(SelectionTarget target, List<Location> locations)
   {
      var targetSet = GetTarget(target);

      AddTo(target, locations.Except(targetSet));
      RemoveFrom(target, targetSet.Except(locations));
   }

   private static void AddTo(SelectionTarget target, IEnumerable<Location> locations)
   {
      var targetSet = GetTarget(target);
      targetSet.UnionWith(locations);
      // TODO The internal handling of the addition

      AddCache.Clear();
   }

   private static void RemoveFrom(SelectionTarget target, IEnumerable<Location> locations)
   {
      var targetSet = GetTarget(target);
      targetSet.ExceptWith(locations);
      // TODO The internal handling of the removal

      RemoveCache.Clear();
   }

   private static HashSet<Location> GetTarget(SelectionTarget target)
   {
      return target switch
      {
         SelectionTarget.Selection => SelectedLocations,
         SelectionTarget.Hover => HoveredLocations,
         SelectionTarget.Highlight => HighlightedLocations,
         _ => throw new ArgumentOutOfRangeException(nameof(target), target, null),
      };
   }

   #endregion

   #region Clear Methods

   public static void Clear(SelectionTarget target)
   {
      switch (target)
      {
         case SelectionTarget.Selection:
            SelectedLocations.Clear();
            break;
         case SelectionTarget.Hover:
            HoveredLocations.Clear();
            break;
         case SelectionTarget.Highlight:
            HighlightedLocations.Clear();
            break;
         default:
            throw new ArgumentOutOfRangeException(nameof(target), target, null);
      }
   }

   public static void ClearAll()
   {
      Clear(SelectionTarget.Highlight);
      Clear(SelectionTarget.Selection);
      Clear(SelectionTarget.Hover);
   }

   #endregion

   #region LMB Selection

   public static void UpdateDragSelection(Vector2 mousePos, bool isDragging, bool isLasso)
   {
      // Dragging just started
      if (isDragging && !IsDragging)
      {
         DragPath.Clear();
         SelectionPreview.Clear();
      }
      // Dragging just ended
      else if (!isDragging && IsDragging)
      {
         // Perform the final selection
         Modify(SelectionTarget.Selection, SelectionPreview, true, false);
      }

      IsDragging = isDragging;
      if (!isDragging)
         return;

      DragPath.Add(mousePos);

      switch (isLasso)
      {
         case true when DragPath.Count > 2:
            SetLassoLocations(DragPath[^3..].Select(v => new PointF(v.X, v.Y)).ToList());
            break;
         case false when DragPath.Count > 1:
            SetRectanglePreviewLocations(DragPath.First(), DragPath.Last());
            break;
      }
   }

   private static void SetRectanglePreviewLocations(Vector2 first, Vector2 last)
   {
      // We just started dragging, so we need to initialize the drag area
      if (DragArea == RectangleF.Empty)
      {
         DragArea = new(Math.Min(first.X, last.X),
                        Math.Min(first.Y, last.Y),
                        Math.Abs(first.X - last.X),
                        Math.Abs(first.Y - last.Y));
      }
      // We are dragging, so we need to update the drag area
      else
      {
         // Calculate the newly added area to the drag area and only check locations in that area
         var newArea = RectangleF.Union(new(Math.Min(first.X, last.X),
                                            Math.Min(first.Y, last.Y),
                                            Math.Abs(first.X - last.X),
                                            Math.Abs(first.Y - last.Y)),
                                        DragArea);

         var (horizontalRect, verticalRect) = GeoRect.RectDiff(DragArea, newArea);

         AddLocationsFromRectangle(horizontalRect);
         AddLocationsFromRectangle(verticalRect);
      }
   }

   private static void AddLocationsFromRectangle(RectangleF rect)
   {
      if (GeoRect.IsRectangleContained(rect, DragArea))
      {
         // Add all locations in the verticalRect to the SelectionPreview
         var addLocs = QuadTree.GetAllPolygonsInRectangle(rect);
         Modify(SelectionTarget.SelectionPreview, SelectionHelpers.PolygonsToLocations(addLocs), invert: false);
      }
      else
      {
         // Add all locations in the horizontalRect to the SelectionPreview
         var addLocs = QuadTree.GetAllPolygonsInRectangle(rect);
         Modify(SelectionTarget.SelectionPreview, SelectionHelpers.PolygonsToLocations(addLocs), true, false);
      }
   }

   private static void SetLassoLocations(List<PointF> vector2S)
   {
      var polygon = new Polygon(vector2S.Select(v => new PointF(v.X, v.Y)).ToList());

      var sw = new Stopwatch();
      sw.Start();

      var locList = Globals.Locations.Values.ToList();
      List<Location> addToPreview = [];
      List<Location> rmvfrPreview = [];

      // The line connecting the last two points
      var locsOnLine = GeoRect.GetLocationsOnLine(vector2S[^2], vector2S[^1], locList);
      foreach (var prov in locsOnLine)
         addToPreview.Add(prov);

      // The line connecting the first and last point to close the lasso
      locsOnLine = GeoRect.GetLocationsOnLine(vector2S[0], vector2S[^1], locList);
      foreach (var prov in locsOnLine)
         addToPreview.Add(prov);

      // The line connecting the first two points
      locsOnLine = GeoRect.GetLocationsOnLine(vector2S[^2], vector2S[0], locList);
      foreach (var prov in locsOnLine)
      {
         if (polygon.Contains(prov.Polygons[0].Vertices[0])) //TODO: Crash potential if a location has no vertices
            addToPreview.Add(prov);
         else
            rmvfrPreview.Add(prov);
      }

      locsOnLine = GeoRect.GetLocationsInPolygon(polygon, locList);

      foreach (var prov in locsOnLine)
      {
         if (SelectionPreview.Contains(prov))
            addToPreview.Add(prov);
         else
            rmvfrPreview.Add(prov);
      }

      sw.Stop();
      Debug.WriteLine($"Checks: {sw.ElapsedTicks} nano seconds");

      Modify(SelectionTarget.SelectionPreview, addToPreview, true, false);
      Modify(SelectionTarget.SelectionPreview, rmvfrPreview, false, false);
   }

   /// <summary>
   /// LMB Select a single location. <br/>
   /// Ctrl + LMB to add/remove from selection has to be called with <c>invert = true</c>
   /// </summary>
   public static void LmbSelect(Location location, bool invert = false)
   {
      Modify(SelectionTarget.Selection, [location], true, invert);
   }

   /// <summary>
   /// LMB + ALT to expand selection to next parent scope <br/>
   /// Finds the biggest parent scope of which all locations are already selected, and then selects it's parent scope. <br/>
   /// E.g. if all locations of a province are selected, the area the province is in will be selected
   /// </summary>
   /// <param name="location"></param>
   public static void ExpandSelection(Location location)
   {
      var bp = SelectionHelpers.FindBiggestFullySelectedParent(location);

      // We have found no bigger parent scope or the selection is nto limited to a single location's parents
      if (bp == null)
         return;

      Set(SelectionTarget.Selection, bp.GetLocations());
   }

   public static void ShrinkSelection(Location location)
   {
      var ptst = SelectionHelpers.FindParentToShrinkTo(location);
      if (ptst == null)
         return;

      Set(SelectionTarget.Selection, ptst.GetLocations());
   }

   #endregion

   #region RMB Selection

   #endregion

   #region Timed Selection

   // Create a timer with the timespan and the inverse action to remove the selection after the time has passed
   // Add the (timespan, locations) tuple to the TimedSelections list so it can be drawn in a special way
   public static void AddTimedSelection(List<Location> locations, TimeSpan duration, SelectionTarget target)
   {
      TimedSelections.Add(new(locations, DateTime.Now.TimeOfDay + duration, target));
      UpdateTimerInterval(duration);
   }

   private static void UpdateTimerInterval(TimeSpan duration)
   {
      var nextInterval = TimedSelections.MinBy(t => t.EndTime).EndTime;
      if (nextInterval < duration)
         TimedSelectionTimer.Change(nextInterval, TimeSpan.FromMilliseconds(-1));
   }

   private static void Callback(object? state)
   {
      if (TimedSelections.Count == 0)
         return;

      var now = DateTime.Now.TimeOfDay;

      foreach (var ts in TimedSelections)
         if (now >= ts.EndTime)
            Modify(ts.Target, ts.Locations);

      TimedSelections.RemoveAll(ts => now >= ts.EndTime);
      if (TimedSelections.Count > 0)
         UpdateTimerInterval(TimedSelections.MinBy(t => t.EndTime).EndTime - now);
   }

   #endregion

   #region Drawing Helpers

   public static void DrawSelection(BorderType type, BorderModifier modifier, List<Location> locations)
   {
   }

   #endregion
}