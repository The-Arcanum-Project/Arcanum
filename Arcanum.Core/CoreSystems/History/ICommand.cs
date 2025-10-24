using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History;

/// Represents the base interface for commands in the command history system.
/// Commands implementing this interface are used to modify the state, and they
/// support execution, undo, and redo operations.
public interface ICommand
{
   /// <summary>
   /// Normal command execution.
   /// </summary>
   public void Execute();
   
   /// <summary>
   /// Undo the command.
   /// </summary>
   public void Undo();

   /// <summary>
   /// Redo the command often can directly call Execute, but can also be used to restore state after an undo.
   /// </summary>
   public void Redo();
   
   /// <summary>
   /// The hash is needed to determine commands which target the same objects in history compaction
   /// </summary>
   /// <returns></returns>
   public List<int> GetTargetHash();

   /// <summary>
   /// Returns the target objects affected by this command.
   /// </summary>
   public IEu5Object[] GetTargets();
   
   public string GetDescription { get; }
   
   /// <summary>
   /// Provides detailed information about the command for debugging purposes.
   /// This will be written to the log if a crash or issue occurs.
   /// </summary>
   /// <param name="indent"></param>
   /// <returns></returns>
   public string GetDebugInformation(int indent);
}

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
}