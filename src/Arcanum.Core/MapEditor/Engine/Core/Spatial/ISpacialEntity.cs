using System.Numerics;

namespace Arcanum.Core.MapEditor.Engine.Core.Spatial;

public interface I3DEntity
{
   protected static int _nextId;
   public int Id { get; }

   Vector3 Position3D { get; set; }
   Quaternion Rotation3D { get; set; }
   Vector3 Scale3D { get; set; }
   /// <summary>
   /// The size in 3D space without rotation and scaling applied.
   /// </summary>
   Vector3 LocalSize3D { get; }

   BoundingBoxF Bounds3D { get; }
}