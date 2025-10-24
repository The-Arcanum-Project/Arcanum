using System.Collections;
using System.Windows.Threading;
using Arcanum.Core.CoreSystems.Map;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Tracing;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.Utils.Scheduling;
using Arcanum.Core.Utils.Sorting;
using Common.Logger;
using Common.UI;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;

public class LocationMapTracing(IEnumerable<IDependencyNode<string>> dependencies) : FileLoadingService(dependencies)
{
   public override List<Type> ParsedObjects { get; } = [];
   public List<PolygonParsing> ParsingPolygons = [];
   public Polygon[]? Polygons;
   public int TotalPolygonsCount;
   public bool FinishedTesselation;
   public (int, int) MapSize;
   public override bool IsHeavyStep => true;
   public override bool HasPriority { get; set; } = true;

   public override string GetFileDataDebugInfo()
   {
      return $"Number of polygons: {ParsingPolygons.Count}";
   }

   public override bool LoadSingleFile(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      using (var bitmap =
             new Bitmap(fileObj.Path.FullPath))
      {
         using (MapTracing tracing = new(bitmap))
         {
            ParsingPolygons = tracing.Trace();
            Polygons = new Polygon[ParsingPolygons.Count];
            MapSize = (bitmap.Width, bitmap.Height);
         }
      }
      TotalPolygonsCount = ParsingPolygons.Count;

      _ = Tessellate();

      ArcLog.WriteLine("MPS", LogLevel.INF, "Finished loading and parsing map polygons.");

      return true;
   }

   private async Task Tessellate()
   {
      await Scheduler.QueueWorkInForParallel(ParsingPolygons.Count,
                                             i => Polygons![i] = ParsingPolygons[i].Tessellate(),
                                             Scheduler.AvailableHeavyWorkers - 2);

      ArcLog.WriteLine("MPS", LogLevel.INF, "Finished tesselation of map polygons.");

      // TODO @MelCo: Make this right

      var tempDict = new Dictionary<int, List<Polygon>>();
      for (var index = 0; index < Polygons!.Length; index++)
      {
         var p = Polygons![index];
         var color = ParsingPolygons[index].Color;
         try
         {
            if (!tempDict.TryGetValue(color, out var list))
               tempDict[color] = list = [];
            list.Add(p);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      foreach (var loc in Globals.Locations.Values)
      {
         loc.Polygons = tempDict.TryGetValue(loc.Color.AsInt(), out var polygonList) ? polygonList.ToArray() : [];
         if (polygonList == null) continue;
         foreach (var polygon in polygonList)
         {
            polygon.ColorIndex = loc.ColorIndex;
         }
      }
      
      lock (this)
      {
         FinishedTesselation = true;
         UIHandle.Instance.MapHandle.NotifyMapLoaded();
      }
      
      // End todo
   }

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      // We do not really unload map data
      // TODO: @MelCo: Implement unloading of map data if necessary
      return true;
   }
}