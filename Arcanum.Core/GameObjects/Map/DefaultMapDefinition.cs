using System.ComponentModel;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.Utils.PropertyHelpers;

namespace Arcanum.Core.GameObjects.Map;

public class DefaultMapDefinition
{
   [DefaultValue("locations.png")]
   public string ProvinceFileName { get; set; } = string.Empty;
   [DefaultValue("rivers.png")]
   public string Rivers { get; set; } = string.Empty;
   [DefaultValue("heightmap.heightmap")]
   public string HeightMap { get; set; } = string.Empty;
   [DefaultValue("adjacencies.csv")]
   public string Adjacencies { get; set; } = string.Empty;
   [DefaultValue("definitions.txt")]
   public string Setup { get; set; } = string.Empty;
   [DefaultValue("ports.csv")]
   public string Ports { get; set; } = string.Empty;
   [DefaultValue("locations_templates.csv")]
   public string LocationsTemplates { get; set; } = string.Empty;

   public bool WrapX { get; set; } = true;
   public int EquatorY { get; set; } = -1;

   public List<(Location, Location)> SoundTolls { get; set; } = [];
   public HashSet<Location> Volcanoes { get; set; } = [];
   public HashSet<Location> Earthquakes { get; set; } = [];
   public HashSet<Location> SeaZones { get; set; } = [];
   public HashSet<Location> Lakes { get; set; } = [];
   public HashSet<Location> NotOwnable { get; set; } = [];
   public HashSet<Location> ImpassableMountains { get; set; } = [];

   public void SetCollection(string collectionName, List<Location> locations, LocationContext ctx)
   {
      switch (collectionName.ToLowerInvariant())
      {
         case "volcanoes":
            Volcanoes = new(locations);
            break;
         case "earthquakes":
            Earthquakes = new(locations);
            break;
         case "sea_zones":
            SeaZones = new(locations);
            break;
         case "lakes":
            Lakes = new(locations);
            break;
         case "impassable_mountains":
            ImpassableMountains = new(locations);
            break;
         case "non_ownable":
            NotOwnable = new(locations);
            break;
         default:
            DiagnosticException.LogWarning(ctx,
                                           ParsingError.Instance.UnknownDefaultMapCollectionName,
                                           nameof(DefaultMapDefinition),
                                           collectionName);
            break;
      }
   }

   public bool IsValid()
   {
      return !string.IsNullOrWhiteSpace(ProvinceFileName) &&
             !string.IsNullOrWhiteSpace(Rivers) &&
             !string.IsNullOrWhiteSpace(HeightMap) &&
             !string.IsNullOrWhiteSpace(Adjacencies) &&
             !string.IsNullOrWhiteSpace(Setup) &&
             !string.IsNullOrWhiteSpace(Ports) &&
             !string.IsNullOrWhiteSpace(LocationsTemplates) &&
             EquatorY >= 0;
   }

   public void SetDefaultValues()
   {
      this.SetAllPropsToDefault();
   }
}