using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands;

/// Represents the initial command in the command history system.
/// This command acts as the starting point for the history tree
/// and does not perform any execution or state changes.
public class CInitial : ICommand
{
   public void Execute()
   {
   }

   public void Undo()
   {
   }

   public void Redo()
   {
   }

   public List<int> GetTargetHash() => [-1];

   public IEu5Object[] GetTargets() => [];

   public string GetDescription => "Initial Command";

   public string GetDebugInformation(int indent) => "Initial Command Debug Information";
   public Type? GetTargetPropertyType() => null;
   public IEu5Object[]? GetTargetProperties() => null;
}