using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Map;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Tracing;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Geometry;
using Arcanum.Core.Utils.Scheduling;
using Arcanum.Core.Utils.Sorting;
using Common.UI;
using Common.UI.Map;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;

public class LocationMapTracing(IEnumerable<IDependencyNode<string>> dependencies) : FileLoadingService(dependencies)
{
   public override List<Type> ParsedObjects { get; } = [];
   public int TotalPolygonsCount;
   public bool FinishedTesselation;
   private MapParsingData? _data;
   public override bool IsHeavyStep => true;
   public override bool HasPriority { get; set; } = true;

   public override string GetFileDataDebugInfo()
   {
      return $"Number of polygons: {TotalPolygonsCount}";
   }

   public override void ReloadSingleFile(Eu5FileObj fileObj,
                                         object? lockObject)
   {
   }

   public override bool LoadSingleFile(Eu5FileObj fileObj, object? lockObject)
   {
      if (AppData.IsHeadless)
      {
         ArcLog.WriteLine("MPS", LogLevel.INF, "Skipping map processing in headless mode.");
         return true;
      }
      
      (int, int) mapSize;
      List<PolygonParsing> parsingPolygons;
      
      using (var bitmap = new Bitmap(fileObj.Path.FullPath))
      using (MapTracing tracing = new(bitmap))
      {
         parsingPolygons = tracing.Trace();
         mapSize = new (bitmap.Width, bitmap.Height);
      }

      TotalPolygonsCount = parsingPolygons.Count;

      _ = Tessellate(parsingPolygons, mapSize);
      
      ArcLog.WriteLine("MPS", LogLevel.INF, "Finished loading and parsing map polygons.");

      return true;
   }

   private async Task Tessellate(List<PolygonParsing> parsingPolygons, (int, int) mapSize)
   {
      var polygons = new Polygon[parsingPolygons.Count];
      if (AppData.IsHeadless)
      {
         // In headless mode, we do not need to tessellate the polygons
         lock (this)
         {
            FinishedTesselation = true;
            //UIHandle.Instance.MapHandle.NotifyMapLoaded();
         }

         return;
      }


      if (Config.Settings.MapSettings.UseFastBorderSmoothing)
      {
         await Scheduler.QueueWorkInForParallel(parsingPolygons.Count,
            i =>
            {
               foreach (var segment in parsingPolygons[i].Segments)
                  if (segment is BorderSegmentDirectional directionalSegment)
                     directionalSegment.SmoothBorders();
            },
            Scheduler.AvailableHeavyWorkers - 2);

         ArcLog.WriteLine("MPS", LogLevel.INF, "Finished smoothing of map polygons.");
      }

      await Scheduler.QueueWorkInForParallel(parsingPolygons.Count,
                                             i => polygons[i] = parsingPolygons[i].Tessellate(),
                                             Scheduler.AvailableHeavyWorkers - 2);

      ArcLog.WriteLine("MPS", LogLevel.INF, "Finished tesselation of map polygons.");

      // TODO @MelCo: Make this right

      var tempDict = new Dictionary<int, List<Polygon>>();
      for (var index = 0; index < polygons.Length; index++)
      {
         var p = polygons[index];
         var color = parsingPolygons[index].Color;
         try
         {
            if (!tempDict.TryGetValue(color, out var list))
               tempDict[color] = list = [];
            list.Add(p);
         }
         catch (Exception e)
         {
            ArcLog.WriteLine("MPP", LogLevel.CRT, e.ToString());
            throw;
         }
      }

      foreach (var loc in Globals.Locations.Values)
      {
         loc.Polygons = tempDict.TryGetValue(loc.Color.AsInt(), out var polygonList) ? polygonList.ToArray() : [];
         if (polygonList == null)
            continue;

         foreach (var polygon in polygonList)
            polygon.ColorIndex = loc.ColorIndex;

         loc.Bounds = GeoRect.CalculateBounds(loc.Polygons);
      }
      
      var data = new MapParsingData(mapSize, polygons);
      
      lock (this)
      {
         FinishedTesselation = true;
      }
      
      if (!UIHandle.Instance.MapHandle.NotifyMapLoaded(data))
         // Map not loaded in UI -> Save data
         lock(this)
            _data = data;
         

      // End todo
   }
   
   /// <summary>
   /// Tries to get the map parsing data. This will only succeed if the tesselation is finished, otherwise it will return false and set data to null.
   /// If data has been disposed after UI init, this will also return false and set data to null.
   /// </summary>
   /// <param name="data"></param>
   /// <returns></returns>
   public bool TryGetMapData([MaybeNullWhen(false)] out MapParsingData data)
   {
      lock (this)
      {
         if (FinishedTesselation && _data != null)
         {
            data = _data;
            return true;
         }

         data = null;
         return false;
      }
   }
   
   public void DisposeMapData()
   {
      lock (this)
         _data = null;
   }
   
   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      // We do not really unload map data
      // TODO: @MelCo: Implement unloading of map data if necessary
      return true;
   }

   public override bool CanBeReloaded => false;
}