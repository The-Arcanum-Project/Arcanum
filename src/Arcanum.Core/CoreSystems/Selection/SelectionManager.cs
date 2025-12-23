using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;
using Application = System.Windows.Application;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

namespace Arcanum.Core.CoreSystems.Selection;

/// <summary>
/// Handles what selection is currently going to the pipeline for processing. <br/>
/// Either we are in location selection or infer selection mode <br/>
/// While we still only listen to the map click events and then decide what to do with them here.
/// And change the variables the UI listens to.
/// </summary>
public static class SelectionManager
{
   private static CancellationTokenSource? _previewCts;
   private static ObjectSelectionMode _previousObjectSelectionMode = ObjectSelectionMode.LocationSelection;
   /// <summary>
   /// This is the collection of objects that are currently editable in the UI. <br/>
   /// This is what the UI listens to.
   /// </summary>
   public static ObservableRangeCollection<IEu5Object> EditableObjects { get; } = new() { IsDistinct = true };
   private static ObservableRangeCollection<IEu5Object> _searchSelectedObjects = new() { IsDistinct = true };

   public static ObservableRangeCollection<Location> PreviewedLocations { get; } = new() { IsDistinct = true };

   public static event Action? PreviewChanged;

   public static bool SelectWater { get; set; } = true;
   public static bool SelectWasteland { get; set; } = true;

   public static ObjectSelectionMode ObjectSelectionMode
   {
      get;
      set
      {
         if (field == value)
            return;

         field = value;
         OnPropertyChanged(nameof(ObjectSelectionMode));
         Application.Current?.Dispatcher.BeginInvoke(new Action(InvalidateSelection));
      }
   } = ObjectSelectionMode.LocationSelection;

   /// <summary>
   /// Is Called after the selection has already updated the editable objects
   /// </summary>
   public static event PropertyChangedEventHandler? PropertyChanged;
   public static void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(null, new(propertyName));

   static SelectionManager()
   {
      Selection.SelectionModified += InvalidateSelection;
   }

   public static void Preview(List<IEu5Object> eu5Objects)
   {
      var targetLocs = GetRelevantLocationsForObjects(eu5Objects);
      for (var i = targetLocs.Count - 1; i >= 0; i--)
         if (targetLocs[i] == Location.Empty)
            targetLocs.RemoveAt(i);
      PreviewedLocations.AddRange(targetLocs);
      PreviewChanged?.Invoke();
   }

   public static async void Preview(List<IEu5Object> eu5Objects, int msDuration)
   {
      try
      {
         Preview(eu5Objects);

         // If we want only one preview at a time, cancel any existing preview
         // _previewCts?.Cancel(); 

         var cts = new CancellationTokenSource();
         _previewCts = cts;

         var token = cts.Token;
         try
         {
            await Task.Delay(msDuration, token);
            Application.Current?.Dispatcher.Invoke(() =>
            {
               if (!token.IsCancellationRequested)
                  UnPreview(eu5Objects);
            });
         }
         catch (TaskCanceledException)
         {
            // The preview was cleared manually or cancelled before the timer finished.
         }
         finally
         {
            if (_previewCts == cts)
               _previewCts = null;
            cts.Dispose();
         }
      }
      catch (Exception e)
      {
         ArcLog.WriteLine("SMN", LogLevel.ERR, $"Error during timed preview: {e}");
      }
   }

   public static void UnPreview(List<IEu5Object> eu5Objects)
   {
      PreviewedLocations.RemoveRange(GetRelevantLocationsForObjects(eu5Objects));
      PreviewChanged?.Invoke();
   }

   public static void ClearPreview()
   {
      _previewCts?.Cancel();
      _previewCts = null;

      PreviewedLocations.Clear();
      PreviewChanged?.Invoke();
   }

   public static List<Location> GetActiveSelectionLocations()
   {
      List<Location> locs = [];
      foreach (var obj in EditableObjects)
      {
         switch (obj)
         {
            case Location loc:
               if (IsAllowedByFilters(loc))
                  locs.Add(loc);
               break;
            case IMapInferable inferable:
               locs.AddRange(inferable.GetRelevantLocations([obj]));
               break;
         }
      }

      return locs;
   }

   private static bool IsAllowedByFilters(Location loc)
   {
      if (!SelectWater && (Globals.DefaultMapDefinition.Lakes.Contains(loc) || Globals.DefaultMapDefinition.SeaZones.Contains(loc)))
         return false;

      if (!SelectWasteland && (Globals.DefaultMapDefinition.NotOwnable.Contains(loc) || Globals.DefaultMapDefinition.ImpassableMountains.Contains(loc)))
         return false;

      return true;
   }

   /// <summary>
   /// Updates the current selection based on the current selection mode
   /// </summary>
   private static void InvalidateSelection()
   {
      // If we are frozen, do nothing
      if (ObjectSelectionMode == ObjectSelectionMode.Frozen)
         return;

      var sw = Stopwatch.StartNew();
      IncrementalInvalidateSelection(Selection.GetSelectedLocations);
      sw.Stop();
      ArcLog.WriteLine("SMN", LogLevel.DBG, $"InvalidateSelection took {sw.ElapsedMilliseconds}ms");
   }

   /// <summary>
   /// Only updates the selection for the given locations
   /// </summary>
   private static void IncrementalInvalidateSelection(List<Location> cLocs)
   {
      switch (ObjectSelectionMode)
      {
         case ObjectSelectionMode.LocationSelection:
         {
            for (var i = cLocs.Count - 1; i >= 0; i--)
            {
               if (!IsAllowedByFilters(cLocs[i]))
                  cLocs.RemoveAt(i);
            }

            EditableObjects.ClearAndAdd(cLocs);
            _searchSelectedObjects.Clear();
            break;
         }
         case ObjectSelectionMode.InferSelection:
         {
            var mapMode = MapModeManager.GetCurrent();

            if (!EmptyRegistry.TryGet(mapMode.DisplayTypes[0], out var empty) || empty is not IMapInferable inferable)
               return;

            EditableObjects.ClearAndAdd(inferable.GetInferredList(cLocs));
            _searchSelectedObjects.Clear();
            break;
         }
         case ObjectSelectionMode.FromSearch:
            EditableObjects.ClearAndAdd(_searchSelectedObjects);
            break;
         default:
            throw new ArgumentOutOfRangeException(nameof(ObjectSelectionMode), ObjectSelectionMode, null);
      }
   }

   public static void ToggleFreeze()
   {
      if (ObjectSelectionMode != ObjectSelectionMode.Frozen)
      {
         _previousObjectSelectionMode = ObjectSelectionMode;
         ObjectSelectionMode = ObjectSelectionMode.Frozen;
      }
      else
         ObjectSelectionMode = _previousObjectSelectionMode;
   }

   public static List<IEu5Object>? GetInferredObjectsForLocations(List<Location> cLocs, Type type)
   {
      if (!EmptyRegistry.TryGet(type, out var empty) || empty is not IMapInferable inferable)
         return null;

      return inferable.GetInferredList(cLocs);
   }

   public static List<Location>? GetRelevantLocationsForObjects(IEnumerable items)
   {
      var obj = items.Cast<IEu5Object>().ToArray();
      if (obj.Length == 0)
         return null;

      var mapMode = MapModeManager.GetCurrent();

      if (mapMode.DisplayTypes[0].IsPrimitiveType())
         return [];

      if (!EmptyRegistry.TryGet(mapMode.DisplayTypes[0], out var empty) ||
          empty is not IMapInferable inferable ||
          obj.GetType() != mapMode.DisplayTypes[0])
         return [];

      return inferable.GetRelevantLocations(obj);
   }

   public static List<Location> GetRelevantLocationsForObjects(IEnumerable<IEu5Object> objs)
   {
      var locations = new List<Location>();
      foreach (var obj in objs)
         locations.AddRange(GetRelevantLocationsForObject(obj));
      return locations;
   }

   public static List<Location> GetRelevantLocationsForObject(IEu5Object obj)
   {
      if (obj is not IMapInferable inferable)
         return [];

      return inferable.GetRelevantLocations([obj]);
   }

   public static void SetSearchSelectedObjects(IEnumerable<IEu5Object> objects) => _searchSelectedObjects.ClearAndAdd(objects);

   public static void ClearSearchSelectedObjects() => _searchSelectedObjects.Clear();

   public static void AddSearchSelectedObject(IEu5Object obj) => _searchSelectedObjects.Add(obj);

   public static void RemoveSearchSelectedObject(IEu5Object obj) => _searchSelectedObjects.Remove(obj);

   public static void Eu5ObjectSelectedInSearch(IEu5Object obj)
   {
      // if shift is held, add to selection
      if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
      {
         AddSearchSelectedObject(obj);
         if (_searchSelectedObjects.Count > 1)
            ObjectSelectionMode = ObjectSelectionMode.FromSearch;
      }
      else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
      {
         // if ctrl is held, toggle selection
         if (_searchSelectedObjects.Contains(obj))
            RemoveSearchSelectedObject(obj);
         else
            AddSearchSelectedObject(obj);

         if (_searchSelectedObjects.Count > 1)
            ObjectSelectionMode = ObjectSelectionMode.FromSearch;
      }
      else
      {
         // otherwise, set selection to only this object
         // but if we are frozen, do not change the selection
         if (ObjectSelectionMode == ObjectSelectionMode.Frozen)
            return;

         IEu5Object[] objs = [obj];
         SetSearchSelectedObjects(objs);
         if (_searchSelectedObjects.Count == 1)
            if (obj is IMapInferable inferable)
               EditableObjects.ClearAndAdd(inferable.GetInferredList(inferable.GetRelevantLocations([obj])));
            else
               EditableObjects.ClearAndAdd(objs);
      }
   }

   private static bool IsPrimitiveType(this Type type)
   {
      return type.IsPrimitive ||
             type == typeof(string) ||
             type == typeof(decimal) ||
             type == typeof(DateTime) ||
             type == typeof(DateTimeOffset) ||
             type == typeof(TimeSpan) ||
             type == typeof(Guid);
   }
}