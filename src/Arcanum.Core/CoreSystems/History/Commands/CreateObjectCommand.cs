#region

using Arcanum.Core.CoreSystems.EventDistribution;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.GameObjects.BaseTypes;

#endregion

namespace Arcanum.Core.CoreSystems.History.Commands;

public sealed class CreateObjectCommand(IEu5Object eu5Object, bool isAdd, bool addToGlobals)
   : Eu5ObjectCommand([eu5Object], null)
{
   private readonly IEu5Object _eu5Object = eu5Object;
   private readonly bool _addToGlobals = addToGlobals;
   private readonly bool _isAdd = isAdd;

   public override void Execute()
   {
      // base.Execute();
      if (_addToGlobals)
      {
         var globals = _eu5Object.GetGlobalItemsNonGeneric();
         if (_isAdd)
            globals.Add(_eu5Object.UniqueId, _eu5Object);
         else
            globals.Remove(_eu5Object.UniqueId);
      }

      SaveMaster.AddNewObject(_eu5Object);
   }

   public override void Undo()
   {
      base.Undo();
      if (_addToGlobals)
      {
         var globals = _eu5Object.GetGlobalItemsNonGeneric();
         if (_isAdd)
            globals.Remove(_eu5Object.UniqueId);
         else
            globals.Add(_eu5Object.UniqueId, _eu5Object);
      }

      SaveMaster.RemoveNewObject(_eu5Object);
      EventDistributor.UpdateNUI?.Invoke();
   }

   public override void Redo()
   {
      Execute();
      EventDistributor.UpdateNUI?.Invoke();
   }

   public override string GetDescription => _isAdd ? "Create Object" : "Delete Object" + $": {_eu5Object.UniqueId} ({_eu5Object.GetType().Name})";
   public override IEu5Object[] GetTargets() => [_eu5Object];
}