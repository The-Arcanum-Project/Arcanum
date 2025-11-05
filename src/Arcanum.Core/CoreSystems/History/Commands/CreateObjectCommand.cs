using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands;

public class CreateObjectCommand(IEu5Object eu5Object, bool isAdd, bool addToGlobals) : ICommand
{
   public void Execute()
   {
      if (addToGlobals)
      {
         var globals = eu5Object.GetGlobalItemsNonGeneric();
         if (isAdd)
            globals.Add(eu5Object.UniqueId, eu5Object);
         else
            globals.Remove(eu5Object.UniqueId);
      }

      SaveMaster.AddNewObject(eu5Object);
   }

   public void Undo()
   {
      if (addToGlobals)
      {
         var globals = eu5Object.GetGlobalItemsNonGeneric();
         if (isAdd)
            globals.Remove(eu5Object.UniqueId);
         else
            globals.Add(eu5Object.UniqueId, eu5Object);
      }

      SaveMaster.RemoveNewObject(eu5Object);
   }

   public void Redo()
   {
      Execute();
   }

   public List<int> GetTargetHash() => [eu5Object.GetHashCode()];

   public string GetDescription
      => isAdd ? "Create Object" : "Delete Object" + $": {eu5Object.UniqueId} ({eu5Object.GetType().Name})";

   public string GetDebugInformation(int indent) => GetDescription;
}