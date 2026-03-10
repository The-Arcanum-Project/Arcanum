using System.Text;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.SavingSystem.Serialization;
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
               {
                  for (var i = 0; i < location.Pops.Count; i++)
                  {
                     var pop = location.Pops[i];
                     sb.Append("define_pop");
                     TreeBuilder.ConstructAndWrite(pop, sb, true, false, null, false, false);
                     if (i < location.Pops.Count - 1)
                        sb.AppendLine();
                  }

                  sb.AppendLine();
               }

      return sb;
   }
}