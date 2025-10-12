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
                   "D:\\SteamLibrary\\steamapps\\common\\Project Caesar Review\\game\\in_game\\map_data\\provinces_small.bmp"))
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
            Parallel.For(0, ParsingPolygons.Count, i => { polygons[i] = ParsingPolygons[i].Tesselate(); });
            lock (this)
            {
                finishedTesselation = true;
                UIHandle.Instance.MapHandle.NotifyMapLoaded();
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