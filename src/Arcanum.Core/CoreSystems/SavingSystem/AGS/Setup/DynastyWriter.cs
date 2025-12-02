using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.Court;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Setup;

public class DynastyWriter() : SetupFileWriter([typeof(Dynasty)], "04_dynasties.txt")
{
   public override IndentedStringBuilder WriteFile()
   {
      var isb = new IndentedStringBuilder();

      using (isb.BlockWithName("dynasty_manager"))
         foreach (var dynasty in Globals.Dynasties.Values)
            ((IEu5Object)dynasty).ToAgsContext().BuildContext(isb);
      return isb;
   }
}