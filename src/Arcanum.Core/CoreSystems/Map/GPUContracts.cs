using System.Numerics;
using Arcanum.Core.GameObjects.LocationCollections;
using Common.UI;

namespace Arcanum.Core.CoreSystems.Map;

public static class GPUContracts
{
   /// <summary>
   /// Transfers an array of colors to the GPU for rendering.
   /// </summary>
   /// <param name="colors">A <see cref="List{T}"/> of colors represented as ARGB integers.</param>
   public static void SetColors(int[] colors)
   {
      UIHandle.Instance.MapHandle.SetColor(colors);
   }

   /// <summary>
   /// Sets the color used for rendering borders on the map.
   /// </summary>
   /// <param name="argb">An int representing a color in ARGB format.</param>
   public static void SetBorderColor(int argb)
   {
   }

   /// <summary>
   /// Renders all locations on the map with the current set of colors on the GPU.
   /// </summary>
   public static void RenderAllLocations()
   {
   }

   /// <summary>
   /// ReRenders the specified locations on the map with the current set of colors on the GPU.
   /// </summary>
   /// <param name="locations"></param>
   public static void RenderLocations(List<Location> locations)
   {
   }

   /// <summary>
   /// Renders all borders on the map with the currently specified BorerColor on the GPU.
   /// </summary>
   public static void RenderAllBorders()
   {
   }

   /// <summary>
   /// ReRenders the borders of the specified locations on the map with the currently specified BorderColor on the GPU.
   /// </summary>
   /// <param name="locations"></param>
   public static void RenderBorders(List<Location> locations)
   {
   }

   /// <summary>
   /// Removes any rendered borders from the map.
   /// </summary>
   public static void CleanBorders()
   {
   }

   /// <summary>
   /// Shuts down the GPU map rendering system and releases any associated resources.
   /// </summary>
   public static void ShutDownMap()
   {
   }

   /// <summary>
   /// Creates a new map with the specified width and height.
   /// Initializes the GPU resources needed for rendering.
   /// </summary>
   /// <param name="width"></param>
   /// <param name="height"></param>
   public static void CreateMap(int width, int height)
   {
   }

   /// <summary>
   /// Converts a <see cref="Vector2"/> representing a point on the map to a <see cref="Point"/> which is in the <c>Locations.png</c> coordinate space.
   /// </summary>
   /// <param name="mapPoint">The Vector2 in map coordinates</param>
   /// <returns></returns>
   public static Point MapToLocation(Vector2 mapPoint)
   {
      return new((int)mapPoint.X, (int)mapPoint.Y);
   }
}