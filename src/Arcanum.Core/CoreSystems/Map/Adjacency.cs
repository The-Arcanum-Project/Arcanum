using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Helper;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map;

public readonly struct Adjacency(Location neighbor, int borderIndex, int neighborBorderIndex)
{
    public readonly Location Neighbor = neighbor;
    public readonly int BorderIndex = borderIndex;
    public readonly int NeighborBorderIndex = neighborBorderIndex;
}

public readonly struct EdgeGeometry(Node start, BorderSegment segment, Node end)
{
    public readonly Node StartNode = start;
    public readonly BorderSegment Segment = segment;
    public readonly Node EndNode = end;
}

public class AdjacencyBorder(EdgeGeometry[] segments)
{
    public EdgeGeometry[] Segments { get; set; } = segments;
}