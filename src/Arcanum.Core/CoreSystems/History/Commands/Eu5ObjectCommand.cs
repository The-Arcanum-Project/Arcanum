using Arcanum.Core.CoreSystems.EventDistribution;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands;

public abstract class Eu5ObjectCommand : ICommand
{
   public readonly Enum? Attribute;
   public readonly Type Type;
   private bool _initialized;

   public abstract string GetDescription { get; }

   protected Eu5ObjectCommand(IEu5Object target, Enum? attribute)
   {
      Type = target.GetType();
      Attribute = attribute;
      SaveMaster.InitCommand(this, target);
   }

   protected Eu5ObjectCommand(IEu5Object[] targets, Enum? attribute)
   {
      if (targets.Length == 0)
         throw new ArgumentException("Targets array cannot be empty.", nameof(targets));

      Type = targets[0].GetType();
      Attribute = attribute;
      _initialized = true;
      SaveMaster.InitCommand(this, targets);
   }

   public bool DisallowMerge(IEu5Object target, Enum attribute)
      => _initialized || target.GetType() != Type || !attribute.Equals(Attribute);

   public virtual void FinalizeSetup()
   {
      _initialized = true;
   }

   protected void InvalidateUI()
   {
      EventDistributor.RegisterChanges(Type, Attribute, GetTargets());
   }

   public virtual void Execute()
   {
      SaveMaster.CommandExecuted(this);
      InvalidateUI();
   }

   public virtual void Undo()
   {
      SaveMaster.CommandUndone(this);
      InvalidateUI();
   }

   public virtual void Redo()
   {
      SaveMaster.CommandExecuted(this);
      InvalidateUI();
   }

   public List<int> GetTargetHash() => GetTargets().Select(t => t.GetHashCode()).ToList();

   public string GetDebugInformation(int indent)
   {
      var indentStr = new string(' ', indent);
      return $"{indentStr}{GetType().Name} targeting {GetTargets().Length} objects.";
   }

   /// <summary>
   /// Returns the target objects affected by this command.
   /// </summary>
   public abstract IEu5Object[] GetTargets();
}