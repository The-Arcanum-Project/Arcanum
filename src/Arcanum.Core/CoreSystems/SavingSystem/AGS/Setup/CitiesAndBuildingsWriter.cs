using System.Diagnostics;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;
using Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Setup;

public class CitiesAndBuildingsWriter()
   : SetupFileWriter([typeof(Location), typeof(BuildingDefinition)], "07_cities_and_buildings.txt")
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
         foreach (var bd in Globals.BuildingsManager.BuildingDefinitions)
         {
            Debug.Assert(bd.Level >= 0, "Building level should be non-negative.");
            Debug.Assert(bd.Location != Location.Empty, "Building should be associated with a location.");
            Debug.Assert(bd.Owner != Country.Empty, "Building should have an owner country.");

            sb.Append(SavingUtil.FormatValue(SavingValueType.Identifier, bd, BuildingDefinition.Field.UniqueId))
              .Append(" = { ")
              .Append("tag = ")
              .Append(SavingUtil.FormatValue(SavingValueType.Identifier, bd, BuildingDefinition.Field.Owner))
              .Append(" level = ")
              .Append(SavingUtil.FormatValue(SavingValueType.Int, bd, BuildingDefinition.Field.Level))
              .Append(" location = ")
              .Append(SavingUtil.FormatValue(SavingValueType.Identifier, bd, BuildingDefinition.Field.Location))
              .AppendLine(" }");
         }

      return sb;
   }
}