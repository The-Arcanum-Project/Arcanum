using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.Core.MapEditor.Engine.Core.Math;

namespace Arcanum.Core.MapEditor.Engine.Core.Spatial;

public interface ISpatialEntity
{
   int Id { get; }
   Vector2I Position2D { get; set; }
   RectF Bounds { get; set; }
   // Vector3 Position3D { get; set; }

   public void MoveTo(Vector2I newPosition)
   {
      Position2D = newPosition;
      Bounds = new(newPosition.X, newPosition.Y, Bounds.Width, Bounds.Height);
   }
}