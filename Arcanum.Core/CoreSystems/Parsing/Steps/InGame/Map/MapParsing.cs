using System.Windows.Threading;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Tracing;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;
using Common.UI;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;

public class LocationMapTracing(IEnumerable<IDependencyNode<string>> dependencies) : FileLoadingService(dependencies)
{
    public override List<Type> ParsedObjects { get; }
    public List<PolygonParsing> ParsingPolygons = [];
    public Polygon[] polygons;
    public bool finishedTesselation = false;
    public (int, int) mapSize;

    public override string GetFileDataDebugInfo()
    {
        return $"Number of polygons: {ParsingPolygons.Count}";
    }

    public override bool LoadSingleFile(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
    {
        using (var bitmap =
               new Bitmap(
                   fileObj.Path.FullPath))
        {
            using (MapTracing tracing = new(bitmap))
            {
                ParsingPolygons = tracing.Trace();
                polygons = new Polygon[ParsingPolygons.Count];
                mapSize = (bitmap.Width, bitmap.Height);
            }
        }

        Task.Run(() =>
        {
            int maxThreads = Math.Max(1, (Environment.ProcessorCount/2));

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxThreads
            };
            
            Parallel.For(0, ParsingPolygons.Count, options, i => { polygons[i] = ParsingPolygons[i].Tesselate(); });
            lock (this)
            {
                finishedTesselation = true;
                Console.WriteLine("Finished tesselation of map polygons.");
            }

        });
        return true;
    }

    public override bool UnloadSingleFileContent(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
    {
        // We do not really unload map data
        // TODO: @MelCo: Implement unloading of map data if necessary
        return true;
    }
}