using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Setup;

public class PopsWriter() : SetupFileWriter([typeof(Location)], "06_pops.txt")
{
   public override IndentedStringBuilder WriteFile()
   {
      var sb = new IndentedStringBuilder();
      using (sb.BlockWithName("locations"))
         foreach (var location in Globals.Locations.Values)
            if (location.Pops.Count > 0)
               using (sb.BlockWithName(location.UniqueId))
                  foreach (var pop in location.Pops)
                     ((IEu5Object)pop).ToAgsContext().BuildContext(sb);

      return sb;
   }
}