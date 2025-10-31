using System.Collections;
using System.ComponentModel;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.Registry;
using Common.Logger;

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
   /// <summary>
   /// This is the collection of objects that are currently editable in the UI. <br/>
   /// This is what the UI listens to.
   /// </summary>
   public static ObservableRangeCollection<IEu5Object> EditableObjects { get; } = new() { IsDistinct = true };

   public static ObjectSelectionMode ObjectSelectionMode
   {
      get => _objectSelectionMode;
      set
      {
         if (_objectSelectionMode == value)
            return;

         _objectSelectionMode = value;
         InvalidateSelection();
         OnPropertyChanged(nameof(ObjectSelectionMode));
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

   /// <summary>
   /// Updates the current selection based on the current selection mode
   /// </summary>
   private static void InvalidateSelection()
   {
      var sw = System.Diagnostics.Stopwatch.StartNew();
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
            break;
         }
         case ObjectSelectionMode.InferSelection:
         {
            var mapMode = MapModeManager.GetCurrent();

            if (!EmptyRegistry.TryGet(mapMode.DisplayType, out var empty) || empty is not IMapInferable inferable)
               return;

            EditableObjects.ReplaceRange(inferable.GetInferredList(cLocs));
            break;
         }
         default:
            throw new ArgumentOutOfRangeException(nameof(ObjectSelectionMode), ObjectSelectionMode, null);
      }
   }

   public static List<IEu5Object>? GetInferredObjectsForLocations(List<Location> cLocs)
   {
      var mapMode = MapModeManager.GetCurrent();

      if (!EmptyRegistry.TryGet(mapMode.DisplayType, out var empty) || empty is not IMapInferable inferable)
         return null;

      return inferable.GetInferredList(cLocs);
   }

   public static List<Location>? GetRelevantLocationsForObjects(IEnumerable items)
   {
      var obj = items.Cast<IEu5Object>().ToArray();
      if (obj.Length == 0)
         return null;

      var mapMode = MapModeManager.GetCurrent();

      if (!EmptyRegistry.TryGet(mapMode.DisplayType, out var empty) ||
          empty is not IMapInferable inferable ||
          obj.GetType() != mapMode.DisplayType)
         return null;

      return inferable.GetRelevantLocations(obj);
   }
}