using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands;

public class CompactingCommandDummy(CompactHistoryNode node) : ICommand
{
   private CompactHistoryNode Node { get; } = node ?? throw new ArgumentNullException(nameof(node));

   public void FinalizeSetup()
   {
      throw new NotImplementedException();
   }

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

   public string GetDescription => $"Compacting {Node.CompactedNodes.Count} nodes";

   public string GetDebugInformation(int indent) => throw new NotImplementedException();
   public Type? GetTargetPropertyType() => null;
   public IEu5Object[]? GetTargetProperties() => null;
}