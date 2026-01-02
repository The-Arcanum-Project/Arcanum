using System.Numerics;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.Core.MapEditor.Engine.Core.Spatial;

namespace Arcanum.Core.MapEditor.Engine.Modes.Nudge.Entities;

public class NudgeEntity(Location location, Vector3 position3D, Quaternion rotation3D, Vector3 scale3D, Vector3 localSize3D)
   : I3DEntity
{
   /// <summary>
   /// The location this entities content is for
   /// </summary>
   public Location Location { get; } = location;

   public int Id { get; } = I3DEntity._nextId++;
   public Vector3 Position3D { get; set; } = position3D;
   public Quaternion Rotation3D { get; set; } = rotation3D;
   public Vector3 Scale3D { get; set; } = scale3D;
   public Vector3 LocalSize3D { get; } = localSize3D;
   public BoundingBoxF Bounds3D => BoundingBoxF.FromSpacialEntity(this);

   // Overrides
   public override string ToString() => $"NudgeEntity(Id={Id}, LocationId={Location.ColorIndex})";

   public override bool Equals(object? obj) => obj is NudgeEntity other && Id == other.Id;
   public override int GetHashCode() => Id;
}