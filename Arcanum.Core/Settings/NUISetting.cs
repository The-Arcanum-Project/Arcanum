using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects;
using Arcanum.Core.GameObjects.CountryLevel;
using Arcanum.Core.GameObjects.Economy;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Pops;
using Region = Arcanum.Core.GameObjects.LocationCollections.Region;

namespace Arcanum.Core.Settings;

public class NUISettings
{
    public NUISetting PopSettings { get; set; } = new(Pop.Field.Type,
                                                      Enum.GetValues<Pop.Field>().Cast<Enum>().ToArray(),
                                                      Enum.GetValues<Pop.Field>().Cast<Enum>().ToArray(),
                                                      Enum.GetValues<Pop.Field>().Cast<Enum>().ToArray());
   public NUISetting PopTypeSettings { get; set; } = new(PopType.Field.Name,
                                                         Enum.GetValues<PopType.Field>().Cast<Enum>().ToArray(),
                                                         Enum.GetValues<PopType.Field>().Cast<Enum>().ToArray(),
                                                         Enum.GetValues<PopType.Field>().Cast<Enum>().ToArray());
   public NUISetting LocationSettings { get; set; } = new(Location.Field.Name,
                                                          Enum.GetValues<Location.Field>().Cast<Enum>().ToArray(),
                                                          Enum.GetValues<Location.Field>().Cast<Enum>().ToArray(),
                                                          Enum.GetValues<Location.Field>().Cast<Enum>().ToArray());

   public NUISetting MarketSettings { get; set; } = new(Market.Field.Location,
                                                        Enum.GetValues<Market.Field>().Cast<Enum>().ToArray(),
                                                        Enum.GetValues<Market.Field>().Cast<Enum>().ToArray(),
                                                        Enum.GetValues<Market.Field>().Cast<Enum>().ToArray());
   public NUISetting ProvinceSettings { get; set; } = new(Province.Field.Name,
                                                          Enum.GetValues<Province.Field>().Cast<Enum>().ToArray(),
                                                          Enum.GetValues<Province.Field>().Cast<Enum>().ToArray(),
                                                          Enum.GetValues<Province.Field>().Cast<Enum>().ToArray());

   public NUISetting AreaSettings { get; set; } = new(Area.Field.Name,
                                                      Enum.GetValues<Area.Field>().Cast<Enum>().ToArray(),
                                                      Enum.GetValues<Area.Field>().Cast<Enum>().ToArray(),
                                                      Enum.GetValues<Area.Field>().Cast<Enum>().ToArray());

   public NUISetting RegionSettings { get; set; } = new(Region.Field.Name,
                                                        Enum.GetValues<Region.Field>().Cast<Enum>().ToArray(),
                                                        Enum.GetValues<Region.Field>().Cast<Enum>().ToArray(),
                                                        Enum.GetValues<Region.Field>().Cast<Enum>().ToArray());

   public NUISetting SuperRegionSettings { get; set; } = new(SuperRegion.Field.Name,
                                                             Enum.GetValues<SuperRegion.Field>().Cast<Enum>().ToArray(),
                                                             Enum.GetValues<SuperRegion.Field>().Cast<Enum>().ToArray(),
                                                             Enum.GetValues<SuperRegion.Field>()
                                                                 .Cast<Enum>()
                                                                 .ToArray());

   public NUISetting ContinentSettings { get; set; } = new(Continent.Field.Name,
                                                           Enum.GetValues<Continent.Field>().Cast<Enum>().ToArray(),
                                                           Enum.GetValues<Continent.Field>().Cast<Enum>().ToArray(),
                                                           Enum.GetValues<Continent.Field>().Cast<Enum>().ToArray());

   public NUISetting AdjacencySettings { get; set; } = new(Adjacency.Field.Name,
                                                           Enum.GetValues<Adjacency.Field>().Cast<Enum>().ToArray(),
                                                           Enum.GetValues<Adjacency.Field>().Cast<Enum>().ToArray(),
                                                           Enum.GetValues<Adjacency.Field>().Cast<Enum>().ToArray());

   public NUISetting LocationRankSettings { get; set; } = new(LocationRank.Field.Name,
                                                              Enum.GetValues<LocationRank.Field>()
                                                                  .Cast<Enum>()
                                                                  .ToArray(),
                                                              Enum.GetValues<LocationRank.Field>()
                                                                  .Cast<Enum>()
                                                                  .ToArray(),
                                                              Enum.GetValues<LocationRank.Field>()
                                                                  .Cast<Enum>()
                                                                  .ToArray());

   public NUISetting RoadSettings { get; set; } = new(Road.Field.StartLocation,
                                                      Enum.GetValues<Road.Field>().Cast<Enum>().ToArray(),
                                                      Enum.GetValues<Road.Field>().Cast<Enum>().ToArray(),
                                                      Enum.GetValues<Road.Field>().Cast<Enum>().ToArray());

   public NUISetting CountrySettings { get; set; } = new(Country.Field.Tag,
                                                         Enum.GetValues<Country.Field>().Cast<Enum>().ToArray(),
                                                         Enum.GetValues<Country.Field>().Cast<Enum>().ToArray(),
                                                         Enum.GetValues<Country.Field>().Cast<Enum>().ToArray());

   public NUISetting TagSettings { get; set; } = new(Tag.Field.Name,
                                                     Enum.GetValues<Tag.Field>().Cast<Enum>().ToArray(),
                                                     Enum.GetValues<Tag.Field>().Cast<Enum>().ToArray(),
                                                     Enum.GetValues<Tag.Field>().Cast<Enum>().ToArray());

   public NUISetting CountryRankSettings { get; set; } = new(CountryRank.Field.Name,
                                                             Enum.GetValues<CountryRank.Field>()
                                                                 .Cast<Enum>()
                                                                 .ToArray(),
                                                             Enum.GetValues<CountryRank.Field>()
                                                                 .Cast<Enum>()
                                                                 .ToArray(),
                                                             Enum.GetValues<CountryRank.Field>()
                                                                 .Cast<Enum>()
                                                                 .ToArray());
}