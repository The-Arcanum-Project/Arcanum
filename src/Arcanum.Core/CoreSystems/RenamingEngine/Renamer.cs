using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.RenamingEngine;

public static class Renamer
{
   public static bool RenameIEu5Object(IEu5Object target, string newId)
   {
      var oldId = target.UniqueId;
      if (oldId == newId)
         return false;

      var globals = target.GetGlobalItemsNonGeneric();
      if (!globals.Contains(oldId))
         return false;

      globals.Remove(oldId);
      target.UniqueId = newId;
      globals.Add(newId, target);

      return globals.Contains(newId);
   }
}