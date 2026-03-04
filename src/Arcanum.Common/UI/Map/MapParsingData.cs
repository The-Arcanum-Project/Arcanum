namespace Common.UI.Map;

/// <summary>
/// Contains all relevant results of the map parsing step
/// </summary>
public class MapParsingData((int, int) mapSize, Polygon[] polygons)
{
    public readonly (int,int) MapSize = mapSize;
    public readonly Polygon[] Polygons = polygons;
}