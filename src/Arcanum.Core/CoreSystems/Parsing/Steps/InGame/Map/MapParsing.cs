using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Arcanum.Core.CoreSystems.Map;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Helper;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Tracing;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
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
      
      // I tried to optimize this, but it really did not change the performance
      // TODO @MelCo: Extract Function
      var colorMap = new Dictionary<int, List<Polygon>>();
      for (var index = 0; index < polygons.Length; index++)
      {
         var p = polygons[index];
         var color = parsingPolygons[index].Color;
         if (!colorMap.TryGetValue(color, out var list))
            colorMap[color] = list = [];
         list.Add(p);
      }

      foreach (var loc in Globals.Locations.Values)
      {
         loc.Polygons = colorMap.TryGetValue(loc.Color.AsInt(), out var polygonList) ? polygonList.ToArray() : [];
         if (polygonList == null)
            continue;

         foreach (var polygon in polygonList)
            polygon.ColorIndex = loc.ColorIndex;

         loc.Bounds = GeoRect.CalculateBounds(loc.Polygons);
      }
      /*
      // TODO use locByColor above too
      var locationCount = Globals.Locations.Count;
      var globalEdgeLists = new List<List<EdgeGeometry>>(locationCount * 3);
      var locByColor = new Dictionary<int, Location>(locationCount);
      foreach (var loc in Globals.Locations.Values)
         locByColor[loc.Color.AsInt()] = loc;
      
      var tempAdjacencies = new List<Adjacency>[locationCount];
      foreach (var loc in Globals.Locations.Values)
      {
         locByColor[loc.Color.AsInt()] = loc;
         tempAdjacencies[loc.ColorIndex] = new(8);
      }
      
      var stopWatch = Stopwatch.StartNew();

      for (var index = 0; index < parsingPolygons.Count; index++)
      {
         var poly = parsingPolygons[index];
         if (poly.Segments.Count == 1)
         {
            // This is a single-segment polygon, which means it is a hole
            var segmentDir = (BorderSegmentDirectional)poly.Segments[0];
            var segment = segmentDir.Segment;
            if (!locByColor.TryGetValue(segment.LeftColor, out var locA) ||
                !locByColor.TryGetValue(segment.RightColor, out var locB))
               continue;
            
            
         }
         
         for (var i = 0; i < poly.Segments.Count; i += 2)
         {
           var segmentDir = (BorderSegmentDirectional)poly.Segments[i + 1];
              
            if (!segmentDir.IsForward)
               continue;

            var segment = segmentDir.Segment;

            if (!locByColor.TryGetValue(segment.LeftColor, out var locA) ||
                !locByColor.TryGetValue(segment.RightColor, out var locB))
               continue;

            var startNode = (Node)poly.Segments[i];
            var endNode = (Node)poly.Segments[(i + 2) % poly.Segments.Count];
            var newEdge = new EdgeGeometry(startNode, segment, endNode);

            var adjListA = tempAdjacencies[locA.ColorIndex];
            var foundBorderId = -1;

            // 3. SPANIFICATION: Completely removes array bounds-checking overhead
            var spanA = CollectionsMarshal.AsSpan(adjListA);
            for (var j = 0; j < spanA.Length; j++)
               // ReferenceEquals is a pure pointer check (fastest possible comparison)
               if (ReferenceEquals(spanA[j].Neighbor, locB))
               {
                  foundBorderId = spanA[j].BorderIndex;
                  break;
               }

            if (foundBorderId != -1)
               globalEdgeLists[foundBorderId].Add(newEdge);
            else
            {
               var newBorderId = globalEdgeLists.Count;

               // Pre-allocate capacity for the geometry list (guess ~4 segments per border)
               globalEdgeLists.Add(new(4) { newEdge });

               adjListA.Add(new(locB, newBorderId, -1));
               tempAdjacencies[locB.ColorIndex].Add(new Adjacency(locA, newBorderId, -1));
            }
         }
      }

      ArcLog.WriteLine("MPS", LogLevel.INF, $"Finished adjacency generation. Time taken: {stopWatch.Elapsed.TotalSeconds:F2} seconds.");
      stopWatch.Restart();
      
      // Use color Index

      // for (var index = 0; index < parsingPolygons.Count; index++)
      // {
      //    var polygon = parsingPolygons[index];
      //    for (var i = 0; i < polygon.Segments.Count; i++)
      //    {
      //       var segment = polygon.Segments[i];
      //       if (segment is not BorderSegmentDirectional directionalSegment)
      //          continue;
      //       if (directionalSegment.IsForward)
      //          Debug.Assert(polygon.Color == directionalSegment.Segment.LeftColor);
      //       else
      //          Debug.Assert(polygon.Color == directionalSegment.Segment.RightColor);
      //    }
      // }
      //
      ArcLog.WriteLine("MPS", LogLevel.INF, $"Finished adjacency verification. Time taken: {stopWatch.Elapsed.TotalSeconds:F2} seconds.");
      stopWatch.Stop();
      
      var finalBorders = new AdjacencyBorder[globalEdgeLists.Count];
      for (var i = 0; i < globalEdgeLists.Count; i++)
         finalBorders[i] = new (globalEdgeLists[i].ToArray() );
      

      foreach (var loc in Globals.Locations.Values)
         // Convert to array and clear the temp reference
         loc.Adjacencies = tempAdjacencies[loc.ColorIndex].ToArray();*/

      var data = new MapParsingData(mapSize, polygons);

      lock(this)
      {
         FinishedTesselation = true;
         _data = data;
      }
      
      UIHandle.Instance.MapHandle.NotifyMapLoaded();
   }
   
   /// <summary>
   /// Tries to get the map parsing data. This will only succeed if the tesselation is finished, otherwise it will return false and set data to null.
   /// If data has been disposed after UI init, this will also return false and set data to null.
   /// </summary>
   /// <param name="data"></param>
   /// <returns></returns>
   public bool TryGetMapData([MaybeNullWhen(false)] out MapParsingData data)
   {
         if (FinishedTesselation && _data != null)
         {
            data = _data;
            return true;
         }

         data = null;
         return false;
   }
   
   public void DisposeMapData()
   {
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