using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.Court;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Setup;

public class CharacterWriter() : SetupFileWriter(GetContainedTypes(), "05_characters.txt")
{
   private static Type[] GetContainedTypes()
   {
      List<Type> types = [typeof(Character)];
      var empty = Character.Empty;
      foreach (var prop in empty.GetAllProperties())
         if (empty.GetNxPropType(prop) is { } t && typeof(IEu5Object).IsAssignableFrom(t))
            types.Add(t);
      return types.ToArray();
   }

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