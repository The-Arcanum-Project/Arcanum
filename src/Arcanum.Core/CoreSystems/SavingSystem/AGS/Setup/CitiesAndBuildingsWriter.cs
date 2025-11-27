using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Setup;

public class CitiesAndBuildingsWriter() : SetupFileWriter([typeof(Location)], "07_cities_and_buildings.txt")
{
   public override IndentedStringBuilder WriteFile()
   {
      var sb = new IndentedStringBuilder();

      using (sb.BlockWithName("locations"))
      {
         // Only box the values once and not for every loop iteration
         Enum idEnum = Location.Field.UniqueId;
         Enum rankEnum = Location.Field.Rank;
         Enum townSetupEnum = Location.Field.TownSetup;
         Enum prosperityEnum = Location.Field.Prosperity;

         foreach (var location in Globals.Locations.Values)
         {
            var hasRank = location.Rank != LocationRank.Empty;
            var hasTown = location.TownSetup != TownSetup.Empty;
            var hasProsperity = location.Prosperity != 0;

            if (!hasRank && !hasTown && !hasProsperity)
               continue;

            sb.Append(SavingUtil.FormatValue(SavingValueType.Identifier, location, idEnum));
            sb.Append(" = { ");

            if (hasRank)
            {
               sb.Append("rank = ");
               sb.Append(SavingUtil.FormatValue(SavingValueType.Identifier, location, rankEnum));
               sb.Append(" ");
            }

            if (hasTown)
            {
               sb.Append("town_setup = ");
               sb.Append(SavingUtil.FormatValue(SavingValueType.Identifier, location, townSetupEnum));
               sb.Append(" ");
            }

            if (hasProsperity)
            {
               sb.Append("prosperity = ");
               sb.Append(SavingUtil.FormatValue(SavingValueType.Float, location, prosperityEnum));
               sb.Append(" ");
            }

            sb.AppendLine("}");
         }
      }

      // TODO: Buildings
      using (sb.BlockWithName("building_manager"))
      {
      }

      return sb;
   }
}