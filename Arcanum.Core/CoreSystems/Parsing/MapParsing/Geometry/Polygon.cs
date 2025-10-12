using System.Numerics;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;

public class Polygon(Vector2[] vertices, int[] indices)
{
    public Vector2[] Vertices { get; } = vertices;
    public int[] Indices { get; } = indices;
}