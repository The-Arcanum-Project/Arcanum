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
   private static int _maxPopulation = -1;
   private static int _minPopulation = -1;
   private static int _medianPopulation = -1;
   private static List<float>? _populationValues;

   public override int GetColorForLocation(Location location)
   {
      var population = CalculateLocationPopulation(location);
      return ColorGenerator.GetColorSigmoid(population, _medianPopulation).AsAbgrInt();
   }

   public override string[] GetTooltip(Location location) => [$"Population: {CalculateLocationPopulation(location)}"];

   public override string? GetLocationText(Location location) => CalculateLocationPopulation(location).ToString();

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
      _maxPopulation = -1;
      _minPopulation = -1;
      _medianPopulation = -1;

      _populationValues ??= new(MapModeManager.LocationsArray.Length);

      foreach (var location in MapModeManager.LocationsArray)
      {
         var pop = CalculateLocationPopulation(location);
         if (pop > 0) // Only include inhabited places for the median logic
            _populationValues.Add(pop);
      }

      if (_populationValues.Count == 0)
      {
         _medianPopulation = 1;
         return;
      }

      _populationValues.Sort();
      _medianPopulation = (int)(_populationValues[_populationValues.Count / 2]);
   }

   public override void OnDeactivateMode()
   {
   }

   private static int CalculateLocationPopulation(Location location)
   {
      var population = 0d;
      foreach (var pop in location.Pops)
         population += pop.Size;

      return (int)(population * 1000);
   }
}