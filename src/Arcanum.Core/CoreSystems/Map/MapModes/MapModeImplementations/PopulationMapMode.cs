using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Pops;
using Arcanum.Core.Utils.Colors;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class PopulationMapMode : LocationBasedMapMode
{
   public override string Name => "Population";
   public override string Description => "Displays the population of each location on the map.";
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.Population;
   public override Type[] DisplayTypes { get; } = [typeof(PopDefinition)];

   private float _medianPopulation = 1f;
   private float _sigmoidConstant = 1f;

   public override int GetColorForLocation(Location location)
   {
      var pop = GetPopInternal(location);
      var t = pop / (pop + _sigmoidConstant);
      return ColorGenerator.GetRedGreenGradient(1f - t).AsAbgrInt();
   }

   public override string[] GetTooltip(Location location) => [$"Population: {GetPopulation(location):N0}"];
   public override string? GetLocationText(Location location) => GetPopulation(location).ToString("N0");
   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
      var locations = MapModeManager.LocationsArray;
      var count = locations.Length;

      const int step = 20;
      const int unrollStride = step * 4;

      var estimatedSize = (count / step) + 8;
      var sampleBuffer = ArrayPool<float>.Shared.Rent(estimatedSize);

      try
      {
         var validCount = 0;
         var i = 0;
         ref var searchSpace = ref MemoryMarshal.GetArrayDataReference(locations);
         var limit = count - unrollStride;

         while (i <= limit)
         {
            var l1 = Unsafe.Add(ref searchSpace, i);
            var l2 = Unsafe.Add(ref searchSpace, i + step);
            var l3 = Unsafe.Add(ref searchSpace, i + step * 2);
            var l4 = Unsafe.Add(ref searchSpace, i + step * 3);

            var p1 = GetPopInternal(l1);
            if (p1 > 0)
               sampleBuffer[validCount++] = p1;

            var p2 = GetPopInternal(l2);
            if (p2 > 0)
               sampleBuffer[validCount++] = p2;

            var p3 = GetPopInternal(l3);
            if (p3 > 0)
               sampleBuffer[validCount++] = p3;

            var p4 = GetPopInternal(l4);
            if (p4 > 0)
               sampleBuffer[validCount++] = p4;

            i += unrollStride;
         }

         for (; i < count; i += step)
         {
            var loc = Unsafe.Add(ref searchSpace, i);
            var p = GetPopInternal(loc);
            if (p > 0)
               sampleBuffer[validCount++] = p;
         }

         if (validCount == 0)
            _medianPopulation = 1f;
         else
         {
            Array.Sort(sampleBuffer, 0, validCount);
            _medianPopulation = sampleBuffer[validCount / 2];
            _sigmoidConstant = _medianPopulation * 3.0f;
         }
      }
      finally
      {
         ArrayPool<float>.Shared.Return(sampleBuffer);
      }
   }

   public override void OnDeactivateMode()
   {
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static float GetPopInternal(Location location)
   {
      float sum = 0;
      var span = CollectionsMarshal.AsSpan(location.Pops.UnderlyingList);

      for (var i = 0; i < span.Length; i++)
         sum += (float)span[i].Size;

      return sum * 1000f;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static float GetPopulation(Location location) => GetPopInternal(location);
}