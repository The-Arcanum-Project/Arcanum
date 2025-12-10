using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.GameObjects.Cultural;
using Arcanum.Core.GameObjects.Religious;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Setup;

public class CoreWriter() : SetupFileWriter([typeof(Institution), typeof(ReligiousSchool)], "02_core.txt")
{
   public override IndentedStringBuilder WriteFile()
   {
      var isb = new IndentedStringBuilder();
      using (isb.BlockWithName("institution_manager"))
      {
         using (isb.BlockWithName("institutions"))
         {
            // ReSharper disable once UnusedVariable
            foreach (var institution in Globals.Institutions.Values)
            {
               // TODO: not yet supported.
            }
         }
      }

      isb.AppendLine();
      isb.AppendLine();
      using (isb.BlockWithName("religion_manager"))
      {
         // ReSharper disable once UnusedVariable
         foreach (var school in Globals.ReligiousSchools.Values)
         {
            // TODO: not yet supported.
         }
      }

      return isb;
   }
}