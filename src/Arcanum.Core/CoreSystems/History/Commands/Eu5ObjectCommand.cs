using Arcanum.Core.CoreSystems.CommandSystem;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands;

public abstract class Eu5ObjectCommand : ICommand
{
   public readonly Enum Attribute;
   public readonly Type Type;
   private bool _initialized;
   private ICommand _iCommandImplementation;

   public abstract string GetDescription { get; }

   protected Eu5ObjectCommand(IEu5Object target, Enum attribute)
   {
      Type = target.GetType();
      Attribute = attribute;
      SaveMaster.InitCommand(this, target);
   }

   public bool DisallowMerge(IEu5Object target, Enum attribute)
      => _initialized || target.GetType() != Type || !attribute.Equals(Attribute);

   public abstract IEu5Object[] GetTargets();

   public virtual void FinalizeSetup()
   {
      _initialized = true;
   }

   public virtual void Execute()
   {
      SaveMaster.CommandExecuted(this);
   }

   public virtual void Undo()
   {
      SaveMaster.CommandUndone(this);
   }

   public virtual void Redo()
   {
      SaveMaster.CommandExecuted(this);
   }

   public List<int> GetTargetHash() => GetTargets().Select(t => t.GetHashCode()).ToList();

   public string GetDebugInformation(int indent)
   {
      var indentStr = new string(' ', indent);
      return $"{indentStr}{GetType().Name} targeting {GetTargets().Length} objects.";
   }

   public abstract Type? GetTargetPropertyType();
   public abstract IEu5Object[]? GetTargetProperties();
}