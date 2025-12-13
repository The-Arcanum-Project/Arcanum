namespace Arcanum.Core.CoreSystems.History;

/// <summary>
/// Represents a manager for handling command history operations, including
/// adding commands, undoing, and redoing operations. This interface provides
/// a contract for implementing different types of history management systems.
/// </summary>
public interface IHistoryManager
{
   /// <summary>
   /// An event that is triggered when an undo operation occurs in the command history system.
   /// This event provides the ability to react to undo actions and can include
   /// additional information about the command being undone.
   /// </summary>
#pragma warning disable CS0067 // Event is never used
   public static event Action<ICommand?>? UndoEvent;
#pragma warning restore CS0067 // Event is never used
   /// <summary>
   /// An event that is triggered when a redo operation occurs in the command history system.
   /// Provides notifications that allow handling or reacting to redo actions, with
   /// additional context about the command being redone.
   /// </summary>
#pragma warning disable CS0067 // Event is never used
   public static event Action<ICommand?>? RedoEvent;
#pragma warning restore CS0067 // Event is never used

   /// <summary>
   /// Adds a command to the history manager for potential undo and redo operations.
   /// </summary>
   /// <param name="entry">The command to be added to the history.</param>
   public void Add(ICommand entry);

   /// <summary>
   /// Indicates whether an undo operation can currently be performed based on the state of the command history.
   /// This property evaluates and returns true if there are valid commands available to undo, otherwise false.
   /// </summary>
   public bool CanUndo { get; }
   /// <summary>
   /// A property that determines whether there are commands available to redo
   /// in the command history system. This property evaluates the state of the
   /// history manager to identify if a redo operation can be performed.
   /// </summary>
   public bool CanRedo { get; }

   /// <summary>
   /// Undoes the last executed command in the history, if possible.
   /// </summary>
   /// <returns>The command that was undone, or null if no undo operation is possible.</returns>
   public ICommand? Undo();

   /// <summary>
   /// Redoes the last undone command in the history, if possible.
   /// </summary>
   /// <returns>The command that was redone, or null if no redo operation is possible.</returns>
   public ICommand? Redo();

   /// <summary>
   /// Clears the command history, resetting it to its initial state.
   /// </summary>
   public void Clear();

   /// <summary>
   /// Gets the current command in the history manager.
   /// </summary>
   public ICommand CurrentCommand { get; }
}