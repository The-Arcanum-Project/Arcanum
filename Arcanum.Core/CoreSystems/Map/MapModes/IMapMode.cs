using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes;

public interface IMapMode
{
   /// <summary>
   /// An internal name for this map mode.
   /// This should be unique among all map modes. <br/>
   /// This is not necessarily user-facing, but it should be descriptive enough to identify the mapmode
   /// </summary>
   public string Name { get; }

   /// <summary>
   /// The enum type of this map mode.
   /// </summary>
   public MapModeManager.MapModeType Type { get; }

   /// <summary>
   /// A brief description of what this map mode does.
   /// </summary>
   public string Description { get; }

   /// <summary>
   /// Points to the icon resource for this map mode.
   /// This should be a path that the UI can use to load the icon. <br/>
   /// The optimal size for the icon is 20x20 pixels.
   /// </summary>
   public string? IconSource { get; }

   /// <summary>
   /// Returns a color for the given location on the map.
   /// </summary>
   /// <param name="location"></param>
   /// <returns></returns>
   public int GetColorForLocation(Location location);
}