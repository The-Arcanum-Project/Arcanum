using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.Registry;
using Application = System.Windows.Application;

namespace Arcanum.Core.CoreSystems.Selection;

/// <summary>
/// Handles what selection is currently going to the pipeline for processing. <br/>
/// Either we are in location selection or infer selection mode <br/>
/// While we still only listen to the map click events and then decide what to do with them here.
/// And change the variables the UI listens to.
/// </summary>
public static class SelectionManager
{
   private static ObjectSelectionMode _objectSelectionMode = ObjectSelectionMode.LocationSelection;
   private static ObjectSelectionMode _previousObjectSelectionMode = ObjectSelectionMode.LocationSelection;
   /// <summary>
   /// This is the collection of objects that are currently editable in the UI. <br/>
   /// This is what the UI listens to.
   /// </summary>
   public static ObservableRangeCollection<IEu5Object> EditableObjects { get; } = new() { IsDistinct = true };
   private static ObservableRangeCollection<IEu5Object> _searchSelectedObjects = new() { IsDistinct = true };

   public static ObjectSelectionMode ObjectSelectionMode
   {
      get => _objectSelectionMode;
      set
      {
         if (_objectSelectionMode == value)
            return;

         _objectSelectionMode = value;
         OnPropertyChanged(nameof(ObjectSelectionMode));
         Application.Current?.Dispatcher.BeginInvoke(new Action(InvalidateSelection));
      }
   }

   /// <summary>
   /// Is Called after the selection has already updated the editable objects
   /// </summary>
   public static event PropertyChangedEventHandler? PropertyChanged;
   public static void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(null, new(propertyName));

   static SelectionManager()
   {
      Selection.SelectionModified += InvalidateSelection;
   }

   public static List<Location> GetActiveSelectionLocations()
   {
      //If frozen, return nothing
      // TODO: @Minnator we need a method to get all the locations which are selected which are not in the frozen state
      // or we continue to ignore new selected locations while frozen
      if (ObjectSelectionMode == ObjectSelectionMode.Frozen)
         return [];

      List<Location> locs = [];
      foreach (var obj in EditableObjects)
      {
         if (obj is Location loc)
            locs.Add(loc);
         if (obj is IMapInferable inferable)
            locs.AddRange(inferable.GetRelevantLocations([obj]));
      }

      return locs;
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
            EditableObjects.ReplaceRange(cLocs);
            _searchSelectedObjects.Clear();
            break;
         }
         case ObjectSelectionMode.InferSelection:
         {
            var mapMode = MapModeManager.GetCurrent();

            if (!EmptyRegistry.TryGet(mapMode.DisplayType, out var empty) || empty is not IMapInferable inferable)
               return;

            EditableObjects.ReplaceRange(inferable.GetInferredList(cLocs));
            _searchSelectedObjects.Clear();
            break;
         }
         case ObjectSelectionMode.FromSearch:
            EditableObjects.ReplaceRange(_searchSelectedObjects);
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

      if (mapMode.DisplayType.IsPrimitiveType())
         return [];

      if (!EmptyRegistry.TryGet(mapMode.DisplayType, out var empty) ||
          empty is not IMapInferable inferable ||
          obj.GetType() != mapMode.DisplayType)
         return null;

      return inferable.GetRelevantLocations(obj);
   }

   public static void SetSearchSelectedObjects(IEnumerable<IEu5Object> objects) => _searchSelectedObjects.ReplaceRange(objects);

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

         SetSearchSelectedObjects([obj]);
         if (_searchSelectedObjects.Count == 1)
            if (obj is IMapInferable inferable)
               EditableObjects.ReplaceRange(inferable.GetInferredList(inferable.GetRelevantLocations([obj])));
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