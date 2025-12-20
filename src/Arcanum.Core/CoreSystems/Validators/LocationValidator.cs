using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Map;
using Region = Arcanum.Core.GameObjects.LocationCollections.Region;

namespace Arcanum.Core.CoreSystems.Validators;

public class LocationValidator : IValidator
{
   private const string ACTION_NAME = "LocationValidator";
   public string Name { get; } = ACTION_NAME;
   public int Priority { get; } = 1;

   public void Validate()
   {
      // For each location we require an entry in named_locations.txt
      //                                       in definitions.txt
      //                                       in location_templates.txt

      foreach (var location in Globals.Locations.Values)
      {
         if (location.Province == Province.Empty)
            De.Warning(BuildContextFromObject(location),
                       ParsingError.Instance.MissingPartInLocationHierarchy,
                       ACTION_NAME,
                       location.UniqueId,
                       location.UniqueId);
         else if (location.Province.Area == Area.Empty)
            De.Warning(BuildContextFromObject(location.Province),
                       ParsingError.Instance.MissingPartInLocationHierarchy,
                       ACTION_NAME,
                       location.UniqueId,
                       location.Province.UniqueId);
         else if (location.Province.Area.Region == Region.Empty)
            De.Warning(BuildContextFromObject(location.Province.Area),
                       ParsingError.Instance.MissingPartInLocationHierarchy,
                       ACTION_NAME,
                       location.UniqueId,
                       location.Province.Area.UniqueId);
         else if (location.Province.Area.Region.SuperRegion == SuperRegion.Empty)
            De.Warning(BuildContextFromObject(location.Province.Area.Region),
                       ParsingError.Instance.MissingPartInLocationHierarchy,
                       ACTION_NAME,
                       location.UniqueId,
                       location.Province.Area.Region.UniqueId);
         else if (location.Province.Area.Region.SuperRegion.Continent == Continent.Empty)
            De.Warning(BuildContextFromObject(location.Province.Area.Region.SuperRegion),
                       ParsingError.Instance.MissingPartInLocationHierarchy,
                       ACTION_NAME,
                       location.UniqueId,
                       location.Province.Area.Region.SuperRegion.UniqueId);

         if (location.TemplateData == LocationTemplateData.Empty)
            De.Warning(BuildContextFromObject(location),
                       ParsingError.Instance.MissingLocationTemplateEntry,
                       ACTION_NAME,
                       location.UniqueId);
      }
   }

   private static LocationContext BuildContextFromObject(IEu5Object obj) => new(obj.FileLocation.Line, obj.FileLocation.Column, obj.Source.Path.FullPath);
}