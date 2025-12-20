using System.Text;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.GameObjects.BaseTypes;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;
using PopDefinition = Arcanum.Core.GameObjects.InGame.Pops.PopDefinition;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Setup;

public class PopsWriter() : SetupFileWriter([typeof(Location), typeof(PopDefinition)], "06_pops.txt")
{
   // windows-1252 encoding
   public override Encoding FileEncoding { get; } = Encoding.GetEncoding(1252);

   public override IndentedStringBuilder WriteFile()
   {
      var sb = new IndentedStringBuilder();
      using (sb.BlockWithName("locations"))
         foreach (var location in Globals.Locations.Values)
            if (location.Pops.Count > 0)
               using (sb.BlockWithName(location.UniqueId))
                  foreach (var pop in location.Pops)
                  {
                     sb.Append("define_pop");
                     ((IEu5Object)pop).ToAgsContext().BuildContext(sb);
                  }

      return sb;
   }
}