using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.Court;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Setup;

public class CharacterWriter() : SetupFileWriter([typeof(Character)], "05_characters.txt")
{
   public override IndentedStringBuilder WriteFile()
   {
      var sb = new IndentedStringBuilder();
      using (sb.BlockWithName("character_db"))
         foreach (var character in Globals.Characters.Values)
            ((IEu5Object)character).ToAgsContext().BuildContext(sb);

      return sb;
   }
}