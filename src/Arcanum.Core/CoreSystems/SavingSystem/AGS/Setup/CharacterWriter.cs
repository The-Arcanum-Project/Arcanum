using System.Text;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.Steps.Setup;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Utils.Sorting;
using Character = Arcanum.Core.GameObjects.InGame.Court.Character;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Setup;

public class CharacterWriter() : SetupFileWriter(SetupParsingManager.NestedSubTypes(Character.Empty), "05_characters.txt")
{
   // windows-1252 encoding
   public override Encoding FileEncoding { get; } = Encoding.GetEncoding(1252);

   public override IndentedStringBuilder WriteFile()
   {
      var characters = Globals.Characters.Values;
      var sorted = TopologicalSort.Sort<string, Character>(characters);
      var sb = new IndentedStringBuilder();
      using (sb.BlockWithName("character_db"))
         foreach (var character in sorted)
            ((IEu5Object)character).ToAgsContext().BuildContext(sb);

      return sb;
   }
}