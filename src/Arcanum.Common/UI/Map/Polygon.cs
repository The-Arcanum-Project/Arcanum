using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Common.UI.Map;

public sealed class Polygon
{
    public Vector2[] Vertices { get; }
    public int[] TriangleIndices { get; } // [0,1,2, 0,2,3,...]
    public RectangleF Bounds { get; }

    public int ColorIndex;

    public Polygon(Vector2[] vertices, int[] triangleIndices)
    {
        Vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
        TriangleIndices = triangleIndices ?? throw new ArgumentNullException(nameof(triangleIndices));

#if DEBUG
        if (Vertices.Length < 3)
            throw new ArgumentException("A polygon must have at least 3 vertices.", nameof(vertices));

        if (TriangleIndices.Length % 3 != 0)
            throw new ArgumentException("Triangle indices must be a multiple of 3.", nameof(triangleIndices));
#endif

        Bounds = CalculateBounds_SIMD();
    }


    private RectangleF CalculateBounds_SIMD()
    {
        if (Vertices.Length == 0)
            return RectangleF.Empty;

        // Get the number of Vector2s we can process at once.
        // Vector<float>.Count is the number of floats in a SIMD register (e.g., 4 or 8).
        // So we process (Vector<float>.Count / 2) Vector2s at a time.
        var vectorSize = Vector<float>.Count;
        var vector2Count = vectorSize / 2;

        var minValues = new Vector<float>(float.PositiveInfinity);
        var maxValues = new Vector<float>(float.NegativeInfinity);

        var i = 0;
        // Process the main part of the array in large, vectorized chunks
        for (; i <= Vertices.Length - vector2Count; i += vector2Count)
        {
            // Load a chunk of vertices into a SIMD vector.
            // The data layout needs to be [X1, Y1, X2, Y2, X3, Y3, X4, Y4] for this to work.
            // We can achieve this with a Span and MemoryMarshal.
            var span = new Span<Vector2>(Vertices, i, vector2Count);
            var vector = MemoryMarshal.Cast<Vector2, float>(span);

            minValues = Vector.Min(minValues, new(vector));
            maxValues = Vector.Max(maxValues, new(vector));
        }

        // Process the remaining elements that didn't fit into a full vector chunk
        float minX = float.PositiveInfinity,
            maxX = float.NegativeInfinity;
        float minY = float.PositiveInfinity,
            maxY = float.NegativeInfinity;

        for (; i < Vertices.Length; i++)
        {
            var vertex = Vertices[i];
            if (vertex.X < minX)
                minX = vertex.X;
            if (vertex.X > maxX)
                maxX = vertex.X;
            if (vertex.Y < minY)
                minY = vertex.Y;
            if (vertex.Y > maxY)
                maxY = vertex.Y;
        }

        // Reduce the SIMD vectors to single min/max values
        // minValues vector might look like [minX1, minY1, minX2, minY2]
        // We need to find the minimum of all X's and all Y's.
        for (var j = 0; j < vectorSize; j++)
            if (j % 2 == 0) // Even indices are X
            {
                if (minValues[j] < minX)
                    minX = minValues[j];
                if (maxValues[j] > maxX)
                    maxX = maxValues[j];
            }
            else // Odd indices are Y
            {
                if (minValues[j] < minY)
                    minY = minValues[j];
                if (maxValues[j] > maxY)
                    maxY = maxValues[j];
            }

        return new(minX, minY, maxX - minX, maxY - minY);
    }
}