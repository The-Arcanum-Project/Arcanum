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

   /// <summary>
   /// Returns the list of properties affected by this command.
   /// if No properties are affected, return null.
   /// </summary>
   /// <returns></returns>
   public Type? GetTargetPropertyType();

   /// <summary>
   /// Returns an array of target properties affected by this command.
   /// If no properties are affected, return null. Or if the properties are not of type IEu5Object, return null.
   /// </summary>
   public IEu5Object[]? GetTargetProperties();
}