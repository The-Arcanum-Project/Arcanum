using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;

public readonly struct BorderSegment()
{
   public readonly List<Vector2I> Points = [];
}

[SkipLocalsInit]
public readonly struct BorderSegmentDirectional(BorderSegment segment, bool isForward) : ICoordinateAdder
{
   public readonly BorderSegment Segment = segment;
   public readonly bool IsForward = isForward;

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void AddTo(List<Vector2I> target)
   {
      ReadOnlySpan<Vector2I> sourceSpan = CollectionsMarshal.AsSpan(Segment.Points);

      var count = sourceSpan.Length;
      if (count == 0)
         return;

      var writeStartIndex = target.Count;
      CollectionsMarshal.SetCount(target, writeStartIndex + count);

      var targetSpan = CollectionsMarshal.AsSpan(target);
      var destSlice = targetSpan.Slice(writeStartIndex, count);

      if (IsForward)
         sourceSpan.CopyTo(destSlice);
      else
      {
         ref var srcStart = ref MemoryMarshal.GetReference(sourceSpan);
         ref var destStart = ref MemoryMarshal.GetReference(destSlice);

         for (var i = 0; i < count; i++)
            Unsafe.Add(ref destStart, i) = Unsafe.Add(ref srcStart, count - 1 - i);
      }
   }

   public BorderSegmentDirectional Invert() => new(Segment, !IsForward);

   public override string ToString() => IsForward ? "Fwd" : "Rev";
}