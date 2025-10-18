namespace Arcanum.Core.CoreSystems.Map;

/// <summary>
/// A lightweight reference to a specific polygon owned by a location.
/// This avoids storing heavy objects or references in the QuadTree.
/// </summary>
public readonly struct PolygonReference(int locationId, int polygonIndex)
{
   public readonly int LocationId = locationId;
   public readonly int PolygonIndex = polygonIndex;
}