using Arcanum.Core.CoreSystems.Common;
using Road = Arcanum.Core.GameObjects.InGame.Map.Road;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Setup;

public class RoadsWriter() : SetupFileWriter([typeof(Road)], "09_roads.txt")
{
   public override IndentedStringBuilder WriteFile()
   {
      Enum from = Road.Field.StartLocation;
      Enum to = Road.Field.EndLocation;
      var sb = new IndentedStringBuilder();
      using (sb.BlockWithName("road_network"))
         foreach (var road in Globals.Roads.Values)
            sb.Append(SavingUtil.FormatValue(SavingValueType.Identifier, road, from))
              .Append(" = ")
              .AppendLine(SavingUtil.FormatValue(SavingValueType.Identifier, road, to));
      return sb;
   }
}