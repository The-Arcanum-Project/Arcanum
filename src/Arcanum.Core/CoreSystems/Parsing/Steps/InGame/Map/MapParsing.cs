using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
      
      // Overall requirements:
      // After tesselation data needs to include a single array containing all borders (finalBorder)
      // A finalBorder in that context is an array of finalSegments that only contain Vector2 s, which contains not only the smoothed border segments but the node positions as well.
      // A finalBorder exists for both the left and right side of a smoothed border. but this can easily be done by having only the left side index and the right side index is just the left one + the original number of borders
      
      // = Border Smoothing
      // Border smoothing should therefore be more generalized, and convert Vector2I to Vector2
      // However I do not know how to solve this since polygons orignially have the reference to the BorderSegmentDirectional
      // The idea would be 
      
      // First Adjacency calculation needs to happen here, so what location is bordering which, that needs to be done with a dictionary which converts color to location which is created here also
      
      var sw = Stopwatch.StartNew();
      
      // Color Lookup for locations
      var locationCount = Globals.Locations.Count;
      var locByColor = new Dictionary<int, Location>(locationCount);
      foreach (var loc in Globals.Locations.Values)
         locByColor[loc.Color.AsInt()] = loc;

      var colorPairToBorderIndex = new Dictionary<long, int>(locationCount * 3);
      var borderSegmentsList = new List<BorderSegment>(parsingPolygons.Count * 3);

      // Add holes for adjacency calculation
      var tempAdjData = new List<(Location neighbour, int borderIndex, bool isLeftSide)>[locationCount];
      foreach (var loc in Globals.Locations.Values)
         tempAdjData[loc.ColorIndex] = new(4);

      
      foreach (var poly in parsingPolygons)
      {
         foreach (var adder in poly.Segments)
         {
            if (adder is not BorderSegmentDirectional { IsForward: true } bsd)
               continue;
            var segment = bsd.Segment;
            borderSegmentsList.Add(segment);
            
            var colorKey = segment.ColorLeft < segment.ColorRight ? ((long)segment.ColorLeft << 32) | (uint)segment.ColorRight : ((long)segment.ColorRight << 32) | (uint)segment.ColorLeft;
            
            if (colorPairToBorderIndex.TryGetValue(colorKey, out var borderIndex)) continue;
            
            borderIndex = colorPairToBorderIndex.Count;
            colorPairToBorderIndex[colorKey] = borderIndex;
            
            if (segment.ColorLeft == MapTracing.OUTSIDE_COLOR || segment.ColorRight == MapTracing.OUTSIDE_COLOR)
               // TODO Handle left right warp
               continue;
            
            var leftLoc = locByColor[segment.ColorLeft];
            var rightLoc = locByColor[segment.ColorRight];
            
            tempAdjData[leftLoc.ColorIndex].Add((rightLoc, borderIndex, true));
            tempAdjData[rightLoc.ColorIndex].Add((leftLoc, borderIndex, false));
         }

         for (var index = 0; index < poly.Holes.Count; index++)
         {
            var hole = poly.Holes[index];
            for (var i = 0; i < hole.Segments.Count; i++)
            {
               var adder = hole.Segments[i];
               if (adder is not BorderSegmentDirectional { IsForward: true } bsd)
                  continue;
               var segment = bsd.Segment;
               borderSegmentsList.Add(segment);

               var colorKey = segment.ColorLeft < segment.ColorRight
                  ? ((long)segment.ColorLeft << 32) | (uint)segment.ColorRight
                  : ((long)segment.ColorRight << 32) | (uint)segment.ColorLeft;

               if (colorPairToBorderIndex.TryGetValue(colorKey, out var borderIndex)) continue;

               borderIndex = colorPairToBorderIndex.Count;
               colorPairToBorderIndex[colorKey] = borderIndex;

               if (segment.ColorLeft == MapTracing.OUTSIDE_COLOR || segment.ColorRight == MapTracing.OUTSIDE_COLOR)
                  // TODO Handle left right warp
                  continue;

               var leftLoc = locByColor[segment.ColorLeft];
               var rightLoc = locByColor[segment.ColorRight];

               tempAdjData[leftLoc.ColorIndex].Add((rightLoc, borderIndex, true));
               tempAdjData[rightLoc.ColorIndex].Add((leftLoc, borderIndex, false));
            }
         }
      }

      var totalBorderCount = colorPairToBorderIndex.Count;
      
      foreach (var loc in Globals.Locations.Values)
      {
         var raw = tempAdjData[loc.ColorIndex];
         var adjacencies = new Adjacency[raw.Count];

         for (var i = 0; i < raw.Count; i++)
         {
            var (neighbor, baseIdx, isLeft) = raw[i];
            adjacencies[i] = isLeft
               ? new Adjacency(neighbor, baseIdx, baseIdx + totalBorderCount)
               : new Adjacency(neighbor, baseIdx + totalBorderCount, baseIdx);
         }

         loc.Adjacencies = adjacencies;
      }
      
      sw.Stop();
      
      
      // Verify that no border is duplicate!
      // Easy check draw a map with all the polygons and holes of the polygons. Each border should be drawn exactly 2 times. So e.g. if a border segment is first gone through paint it blue if it goes a second time green and any other time red
      /*var mapBitmap = new Bitmap(mapSize.Item1 + 2, mapSize.Item2 + 2);
      var errorCounter = 0;
      
      foreach (var poly in parsingPolygons)
      {
         // Get all polygons with the holes


         for (var index = 0; index < poly.Holes.Count; index++)
         {
            var hole = poly.Holes[index];
            
            Debug.Assert(hole.Holes.Count == 0, "Holes within holes should not happen");
            
            foreach (var adder in hole.Segments)
            {
               if (adder is not BorderSegmentDirectional bsd)
                  continue;
               var segment = bsd.Segment;
               foreach (var point in segment.Points)
               {
                  var existingColor = mapBitmap.GetPixel(point.X, point.Y);
                  if (existingColor.ToArgb() == 0) // Not drawn yet
                     mapBitmap.SetPixel(point.X, point.Y, Color.Blue);
                  else if (existingColor.ToArgb() == Color.Blue.ToArgb()) // Drawn once before
                     mapBitmap.SetPixel(point.X, point.Y, Color.Green);
                  else if (existingColor.ToArgb() != Color.Red.ToArgb())
                  {
                     mapBitmap.SetPixel(point.X, point.Y, Color.Red);
                     errorCounter++;
                  }
               }
            }
         }

         foreach (var adder in poly.Segments)
         {   
            if (adder is not BorderSegmentDirectional bsd)
               continue;
            var segment = bsd.Segment;
            foreach (var point in segment.Points)
            {
               var existingColor = mapBitmap.GetPixel(point.X, point.Y);
               if (existingColor.ToArgb() == 0) // Not drawn yet
                  mapBitmap.SetPixel(point.X, point.Y, Color.Blue);
               else if (existingColor.ToArgb() == Color.Blue.ToArgb()) // Drawn once before
                  mapBitmap.SetPixel(point.X, point.Y, Color.Green);
               else if (existingColor.ToArgb() != Color.Red.ToArgb())
               {
                  mapBitmap.SetPixel(point.X, point.Y, Color.Red);
                  errorCounter++;
               }
            }
         }
      }

      mapBitmap.Save("mapDebug.png");


      ArcLog.Write("MPS", LogLevel.INF, $"Finished border extraction and adjacency generation. Time taken: {sw.Elapsed.TotalMilliseconds} ms. Total borders: {totalBorderCount}.");*/
      // foreach (var poly in parsingPolygons)
      // foreach (var hole in poly.Holes)
      // foreach (var adder in hole.Segments)
      // {
      //    if (adder is not BorderSegmentDirectional bsd)
      //       continue;
      //    var segment = bsd.Segment;
      //    
      //    var leftColor = segment.ColorLeft;
      //    var rightColor = segment.ColorRight;
      //    
      //    // check colors for hole and polygon
      //    if (!bsd.IsForward)
      //       if (leftColor != poly.Color || rightColor != hole.Color)
      //       {
      //          ArcLog.WriteLine("MPS", LogLevel.WRN, $"Invalid border direction for polygon with color {poly.Color} and hole with color {hole.Color}. Expected forward direction.");
      //       }
      //       else if (leftColor != hole.Color || rightColor != poly.Color)
      //       {
      //          ArcLog.WriteLine("MPS", LogLevel.WRN, $"Invalid border direction for polygon with color {poly.Color} and hole with color {hole.Color}. Expected forward direction.");
      //       }
      // }
      
      // Check if there arent any duplicate holes with the same nodes
      /*
      foreach (var poly in parsingPolygons)
      {
         var nodes = new HashSet<Node>();
         
         foreach (var hole in poly.Holes)
         {
            var holeNodes = hole.Segments.OfType<Node>();
            foreach (var node in holeNodes)
            {
               if (!nodes.Add(node))
                  ArcLog.WriteLine("MPS", LogLevel.WRN, $"Duplicate node found in holes of polygon with color {poly.Color}. Node position: {node.Position}");
            }
         }
      }*/

      // use borders and locByColor and borders later in Tesselate!
      
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