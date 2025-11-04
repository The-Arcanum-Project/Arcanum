using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
    * x Ctrl + LMB + Drag: Rectangle select (add to selection if not selected or remove from selection if selected)
    * x ALT + LMB + Drag: Lasso select (add to selection if not selected or remove from selection if selected)
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
   public static RectangleF DragArea { get; set; } = RectangleF.Empty;

   public static MapManager MapManager = new();

   public static Location CurrentLocationBelowMouse { get; set; } = Location.Empty;

   #region Events

   public static event Action<List<Location>>? LocationSelected;
   public static event Action<List<Location>>? LocationDeselected;
   public static event Action<List<Location>>? LocationHovered;
   public static event Action<List<Location>>? LocationUnhovered;
   public static event Action<List<Location>>? LocationHighlighted;
   public static event Action<List<Location>>? LocationUnhighlighted;
   public static event Action<List<Location>>? LocationSelectionChanged;
   public static event Action<(List<Location> add, List<Location> remove)>? RectangleSelectionUpdated;
   public static event Action<(List<Location> add, List<Location> remove)>? LassoSelectionUpdated;
   public static event Action? SelectionModified;

   private static void OnLocationsSelected(List<Location> locations)
   {
      LocationSelected?.Invoke(locations);
      LocationSelectionChanged?.Invoke(GetSelectedLocations);
   }

   private static void OnLocationsDeselected(List<Location> locations)
   {
      LocationDeselected?.Invoke(locations);
      LocationSelectionChanged?.Invoke(GetSelectedLocations);
   }

   private static void OnLocationsHovered(List<Location> locations)
   {
      LocationHovered?.Invoke(locations);
   }

   private static void OnLocationsUnhovered(List<Location> locations)
   {
      LocationUnhovered?.Invoke(locations);
   }

   private static void OnLocationsHighlighted(List<Location> locations)
   {
      LocationHighlighted?.Invoke(locations);
   }

   private static void OnLocationsUnhighlighted(List<Location> locations)
   {
      LocationUnhighlighted?.Invoke(locations);
   }

   private static void OnRectangleSelectionUpdated(List<Location> added, List<Location> removed)
   {
      RectangleSelectionUpdated?.Invoke((added, removed));
   }

   private static void OnLassoSelectionUpdated(List<Location> added, List<Location> removed)
   {
      LassoSelectionUpdated?.Invoke((added, removed));
   }

   private static void TriggerEvents(SelectionTarget target,
                                     SelectionMethod method,
                                     List<Location> added,
                                     List<Location> removed)
   {
      if (added.Count > 0)
         switch (target)
         {
            case SelectionTarget.Selection:
               OnLocationsSelected(added);
               break;
            case SelectionTarget.Hover:
               OnLocationsHovered(added);
               break;
            case SelectionTarget.Highlight:
               OnLocationsHighlighted(added);
               break;
            case SelectionTarget.SelectionPreview:
               // No event for preview changes
               break;
            default:
               throw new ArgumentOutOfRangeException(nameof(target), target, null);
         }

      if (removed.Count > 0)
         switch (target)
         {
            case SelectionTarget.Selection:
               OnLocationsDeselected(removed);
               break;
            case SelectionTarget.Hover:
               OnLocationsUnhovered(removed);
               break;
            case SelectionTarget.Highlight:
               OnLocationsUnhighlighted(removed);
               break;
            case SelectionTarget.SelectionPreview:
               // No event for preview changes
               break;
            default:
               throw new ArgumentOutOfRangeException(nameof(target), target, null);
         }

      switch (method)
      {
         case SelectionMethod.Simple:
         case SelectionMethod.Undefined:
         case SelectionMethod.Expand:
         case SelectionMethod.Rectangle:
            break;
         case SelectionMethod.Lasso:
            OnLassoSelectionUpdated(added, removed);
            break;
         default:
            throw new ArgumentOutOfRangeException(nameof(method), method, null);
      }

      if ((added.Count > 0 || removed.Count > 0) && target == SelectionTarget.Selection)
         SelectionModified?.Invoke();
   }

   #endregion

   #region Convenience Getter

   public static List<Location> GetSelectedLocations => SelectedLocations.ToList();
   public static List<Location> GetHoveredLocations => HoveredLocations.ToList();
   public static List<Location> GetHighlightedLocations => HighlightedLocations.ToList();
   public static List<Location> GetSelectionPreviewLocations => SelectionPreview.ToList();

   public static int SelectedLocationCount => SelectedLocations.Count;
   public static int HoveredLocationCount => HoveredLocations.Count;
   public static int HighlightedLocationCount => HighlightedLocations.Count;
   public static int SelectionPreviewCount => SelectionPreview.Count;

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
                             SelectionMethod method,
                             IEnumerable<Location> locations,
                             bool additive = false,
                             bool invert = true,
                             bool clearAllFirst = false)
   {
      var targetSet = GetTarget(target);

      if (clearAllFirst)
         RemoveCache.AddRange(targetSet);

      // in each branch respectively we gather the toAdd and toRemove lists
      if (additive)
         foreach (var loc in locations)
         {
            if (loc == Location.Empty)
               continue;

            if (targetSet.Contains(loc))
            {
               if (invert)
                  RemoveCache.Add(loc);
            }
            else
               AddCache.Add(loc);
         }
      else
         foreach (var loc in locations)
         {
            if (loc == Location.Empty)
               continue;

            if (targetSet.Contains(loc))
               RemoveCache.Add(loc);
            else
            {
               if (invert)
                  AddCache.Add(loc);
            }
         }

      AddTo(target, AddCache, false);
      RemoveFrom(target, RemoveCache, false);
      TriggerEvents(target, method, AddCache, RemoveCache);
      AddCache.Clear();
      RemoveCache.Clear();
   }

   public static void Set(SelectionTarget target, SelectionMethod method, List<Location> locations)
   {
      var targetSet = GetTarget(target);

      AddTo(target, locations.Except(targetSet));
      RemoveFrom(target, targetSet.Except(locations));
      TriggerEvents(target, method, locations.Except(targetSet).ToList(), targetSet.Except(locations).ToList());
   }

   private static void AddTo(SelectionTarget target, IEnumerable<Location> locations, bool autoClear = true)
   {
      var targetSet = GetTarget(target);
      targetSet.UnionWith(locations);
      // TODO The internal handling of the addition

      if (autoClear)
         AddCache.Clear();
   }

   private static void RemoveFrom(SelectionTarget target, IEnumerable<Location> locations, bool autoClear = true)
   {
      var targetSet = GetTarget(target);
      targetSet.ExceptWith(locations);
      // TODO The internal handling of the removal

      if (autoClear)
         RemoveCache.Clear();
   }

   private static HashSet<Location> GetTarget(SelectionTarget target)
   {
      return target switch
      {
         SelectionTarget.Selection => SelectedLocations,
         SelectionTarget.Hover => HoveredLocations,
         SelectionTarget.Highlight => HighlightedLocations,
         SelectionTarget.SelectionPreview => SelectionPreview,
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
            var selection = SelectedLocations.ToList();
            SelectedLocations.Clear();
            OnLocationsDeselected(selection);
            break;
         case SelectionTarget.Hover:
            var hover = HoveredLocations.ToList();
            HoveredLocations.Clear();
            OnLocationsUnhovered(hover);
            break;
         case SelectionTarget.Highlight:
            var highlight = HighlightedLocations.ToList();
            HighlightedLocations.Clear();
            OnLocationsUnhighlighted(highlight);
            break;
         case SelectionTarget.SelectionPreview:
            SelectionPreview.Clear();
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
         Modify(SelectionTarget.Selection,
                isLasso ? SelectionMethod.Lasso : SelectionMethod.Rectangle,
                SelectionPreview,
                true,
                false);
         Clear(SelectionTarget.SelectionPreview);
      }

      IsDragging = isDragging;
      if (!isDragging)
         return;

      var sType = isLasso ? "Lasso" : "Rectangle";
      //Console.WriteLine($"Selection: {sType,10} | Mouse Position: {mousePos} | Drag Path Count: {DragPath.Count} | Drag Area: {DragArea}");

      DragPath.Add(mousePos);

      switch (isLasso)
      {
         case true when DragPath.Count > 2:
            SetLassoLocations(DragPath[^3..].Select(v => new Vector2(v.X, v.Y)).ToArray());
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

         List<Location> added;
         List<Location> removed;

         if (GeoRect.IsRectangleContained(horizontalRect, DragArea))
         {
            added = MapManager.Lqt.FindLocations(horizontalRect);
            removed = MapManager.Lqt.FindLocations(verticalRect);
         }
         else
         {
            added = MapManager.Lqt.FindLocations(verticalRect);
            removed = MapManager.Lqt.FindLocations(horizontalRect);
         }

         Modify(SelectionTarget.SelectionPreview, SelectionMethod.Rectangle, added, invert: false);
         Modify(SelectionTarget.SelectionPreview, SelectionMethod.Rectangle, removed, true, false);

         OnRectangleSelectionUpdated(added, removed);
      }
   }

   private static void SetLassoLocations(Vector2[] vector2S)
   {
      // TODO: @Minnator why do you use the tessellated polygon here?
      var polygon = new Polygon(vector2S.Select(v => new Vector2(v.X, v.Y)).ToArray(), []);

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
         if (polygon.Contains(prov.Polygons[0].Vertices[0])) //TODO: Crash potential if a location has no vertices
            addToPreview.Add(prov);
         else
            rmvfrPreview.Add(prov);

      locsOnLine = GeoRect.GetLocationsInPolygon(polygon, locList);

      foreach (var prov in locsOnLine)
         if (SelectionPreview.Contains(prov))
            addToPreview.Add(prov);
         else
            rmvfrPreview.Add(prov);

      sw.Stop();
      Debug.WriteLine($"Checks: {sw.ElapsedTicks} nano seconds");

      Modify(SelectionTarget.SelectionPreview, SelectionMethod.Lasso, addToPreview, true, false);
      Modify(SelectionTarget.SelectionPreview, SelectionMethod.Lasso, rmvfrPreview, false, false);
   }

   /// <summary>
   /// LMB Select a single location. <br/>
   /// Ctrl + LMB to add/remove from selection has to be called with <c>invert = true</c>
   /// </summary>
   public static void LmbSelect(Location location, bool invert = false)
   {
      Modify(SelectionTarget.Selection, SelectionMethod.Simple, [location], true, invert);
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

      Set(SelectionTarget.Selection, SelectionMethod.Expand, bp.GetLocations());
   }

   public static void ShrinkSelection(Location location)
   {
      var ptst = SelectionHelpers.FindParentToShrinkTo(location);
      if (ptst == null)
         return;

      Set(SelectionTarget.Selection, SelectionMethod.Expand, ptst.GetLocations());
   }

   #endregion

   #region SelectionStartAndEndHelpers

   // For Rectangle Selection we can just call UpdateDragSelection with isLasso = false
   public static void StartRectangleSelection(Vector2 startPos)
   {
      Console.WriteLine($"Rectangle selection started at {startPos} with initial area {DragArea}");
      UpdateDragSelection(startPos, true, false);
   }

   public static void EndRectangleSelection(Vector2 endPos)
   {
      Console.WriteLine($"Rectangle selection ended at {endPos} with final area {DragArea}");
      UpdateDragSelection(endPos, false, false);
      DragArea = RectangleF.Empty;
   }

   // For Lasso Selection we can just call UpdateDragSelection with isLasso = true
   public static void StartLassoSelection(Vector2 startPos)
   {
      UpdateDragSelection(startPos, true, true);
   }

   public static void EndLassoSelection(Vector2 endPos)
   {
      UpdateDragSelection(endPos, false, true);
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
            Modify(ts.Target, SelectionMethod.Undefined, ts.Locations);

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

   public static bool GetLocation(Vector2 vec2, [MaybeNullWhen(false)] out Location location)
   {
      location = MapManager.FindLocationAt(vec2);
      return location != null;
   }

   public static List<Location> GetLocations(RectangleF rect)
   {
      return MapManager.Lqt.FindLocations(rect);
   }

   public static List<Location> GetLocations(Polygon polygon)
   {
      return MapManager.Lqt.FindLocations(polygon);
   }
}