using System.Collections;
using System.Diagnostics;
using Arcanum.Core.CoreSystems.History.Commands;

namespace Arcanum.Core.CoreSystems.History;

public enum HistoryEntryType
{
   Normal,
   Compacted,
}

public class HistoryNode(int id, ICommand command, HistoryEntryType entryType, HistoryNode parent = null!)
   : IEnumerable<(HistoryNode Node, int Level)>
{
   /// <summary>
   /// Gets the unique identifier of the history node.
   /// </summary>
   /// <remarks>
   /// The identifier is assigned at the creation of the history node
   /// and is used to uniquely identify and locate the node within
   /// the history tree structure. It is immutable after initialization.
   /// </remarks>
   public int Id { get; } = id;
   /// <summary>
   /// Represents the command associated with the history node.
   /// </summary>
   /// <remarks>
   /// The command corresponds to an operation in the system's history tracking,
   /// allowing for execution, undo, and redo of specific actions. This property
   /// is immutable after the construction of the history node.
   /// </remarks>
   public ICommand Command { get; } = command;
   /// <summary>
   /// Gets or sets the parent node of the current history node.
   /// </summary>
   /// <remarks>
   /// The parent property is used to maintain the hierarchical relationship
   /// between history nodes in the tree structure. It allows traversal or
   /// reorganization of the history tree, ensuring each node has a consistent
   /// point of reference within the structure. Modifying this property should
   /// respect the integrity of the tree, particularly during operations
   /// such as compaction or insertion.
   /// </remarks>
   public HistoryNode Parent { get; set; } = parent;
   /// <summary>
   /// Gets the entry type of the history node.
   /// </summary>
   /// <remarks>
   /// The entry type indicates the classification of the history node
   /// within the command history, such as whether it is a normal entry
   /// or a compacted entry. It provides context for the node's behavior
   /// and role within the history tree structure.
   /// </remarks>
   public HistoryEntryType EntryType { get; } = entryType;
   /// <summary>
   /// Gets the collection of child history nodes associated with this node.
   /// </summary>
   /// <remarks>
   /// This property contains the child nodes of the current history node within the
   /// hierarchical structure. It is initialized to an empty list and can be used to
   /// manage the tree structure by adding or removing child nodes. Each child node
   /// will have its <see cref="Parent"/> property set to this node.
   /// </remarks>
   public List<HistoryNode> Children { get; init; } = [];
   /// <summary>
   /// Indicates whether the history node has been compacted.
   /// </summary>
   /// <remarks>
   /// When set to true, this property signifies that the node has been compacted,
   /// typically as part of a process to reduce the number of nodes in the history
   /// tree. Compacted nodes may represent multiple commands or operations that
   /// have been merged into a single entity for optimization or simplification purposes.
   /// </remarks>
   public bool IsCompacted { get; set; } = false;

   /// <summary>
   /// Retrieves a child node with the specified identifier from the list of children.
   /// </summary>
   /// <param name="id">The identifier of the child node to retrieve.</param>
   /// <returns>The child node with the specified identifier.</returns>
   /// <exception cref="InvalidOperationException">Thrown if no child node with the specified identifier exists.</exception>
   public HistoryNode GetChildWithId(int id)
   {
      return Children.Single(child => child.Id == id);
   }

   /// <summary>
   /// Returns an enumerator that iterates through the history nodes in a hierarchical structure,
   /// starting from the current node and traversing its descendants.
   /// </summary>
   /// <returns>An enumerator of tuple values, where each tuple contains a <see cref="HistoryNode"/> and its level in the hierarchy.</returns>
   public IEnumerator<(HistoryNode Node, int Level)> GetEnumerator()
   {
      return Traverse(this, 0).GetEnumerator();
   }

   /// <summary>
   /// Returns an enumerator that iterates through the history nodes in a hierarchical structure,
   /// starting from the current node and traversing its descendants.
   /// </summary>
   /// <returns>An enumerator of tuple values, where each tuple contains a <see cref="HistoryNode"/> and its level in the hierarchy.</returns>
   IEnumerator IEnumerable.GetEnumerator()
   {
      return GetEnumerator();
   }

   private IEnumerable<(HistoryNode, int)> Traverse(HistoryNode node, int level)
   {
      yield return (node, level);

      foreach (var child in node.Children)
         foreach (var descendant in Traverse(child, level + 1))
            yield return descendant;
   }
}

/// <summary>
/// Represents a specialized type of <see cref="HistoryNode"/> that compacts multiple history nodes
/// into a single node, with functionality for undoing, redoing, and managing compacted nodes.
/// </summary>
public class CompactHistoryNode : HistoryNode
{
   /// <summary>
   /// Gets the collection of history nodes that have been compacted into this node.
   /// </summary>
   /// <remarks>
   /// The <c>CompactedNodes</c> property contains the individual <see cref="HistoryNode"/> instances
   /// that were combined to form this compacted node. It allows access to the underlying nodes
   /// for operations such as undo, redo, or inspection. The nodes in this collection retain their
   /// original order as they were prior to being compacted.
   /// </remarks>
   public List<HistoryNode> CompactedNodes { get; }
   private int _current;

   public CompactHistoryNode(int id, List<HistoryNode> compactedNodes) : base(id,
                                                                              null!,
                                                                              HistoryEntryType.Compacted,
                                                                              compactedNodes[0].Parent)
   {
      CompactedNodes = compactedNodes;
      _current = CompactedNodes.Count - 1;
   }
   
   public new ICommand Command => new CompactingCommandDummy(this);

   /// <summary>
   /// Inserts the compacted history node into the hierarchy of history nodes,
   /// replacing the compacted nodes and connecting their children to the compacted node.
   /// </summary>
   /// <exception cref="System.Diagnostics.Debug.Assert(bool)">Thrown if the parent of the first compacted node is null.</exception>
   public void InsertInTree()
   {
      // remove the compacted nodes from the tree
      Debug.Assert(CompactedNodes[0].Parent != null,
                   "Parent must never be null when inserting a compacted node into the tree");

      CompactedNodes[0].Parent.Children.Remove(CompactedNodes[0]);
      CompactedNodes[0].Parent = null!;

      foreach (var endChild in CompactedNodes[^1].Children)
      {
         endChild.Parent = this;
         Children.Add(endChild);
      }

      CompactedNodes[^1].Children.Clear();

      // insert the node into the tree
      Parent.Children.Add(this);
   }

   /// <summary>
   /// Restores the original structure of compacted history nodes by replacing the compacted node
   /// with its original nodes and reassociating their child nodes back into the hierarchy.
   /// </summary>
   /// <exception cref="InvalidOperationException">Thrown if the parent node is null during the un-compacting process.</exception>
   public void UnCompact()
   {
      // remove the compacted nodes from the tree
      Parent.Children.Remove(this);

      foreach (var endChild in Children)
      {
         endChild.Parent = CompactedNodes[^1];
         CompactedNodes[^1].Children.Add(endChild);
      }

      Children.Clear();

      // insert the node into the tree
      CompactedNodes[0].Parent = Parent;
      Parent.Children.Add(CompactedNodes[0]);
   }

   /// <summary>
   /// Executes a single undo operation within the sequence of compacted history nodes.
   /// Decrements the current position in the compacted nodes and calls the Undo method of the corresponding command.
   /// </summary>
   /// <exception cref="InvalidOperationException">Thrown if invoked when there are no available steps to undo.</exception>
   public void StepUndo()
   {
      _current--;
      Debug.Assert(_current >= 0, $"False invocation of \"{nameof(StepUndo)}\" without checking availability first!");

      CompactedNodes[_current].Command.Undo();
   }

   /// <summary>
   /// Retrieves the command corresponding to the next redo step within the compacted history nodes.
   /// </summary>
   /// <returns>The command associated with the next step in the redo sequence.</returns>
   /// <exception cref="InvalidOperationException">
   /// Thrown if the next redo command is accessed without verifying its availability.
   /// </exception>
   public ICommand GetStepRedoCommand()
   {
      _current++;
      Debug.Assert(_current < CompactedNodes.Count,
                   $"False invocation of \"{nameof(GetStepRedoCommand)}\" without checking availability first!");

      return CompactedNodes[_current].Command;
   }

   /// <summary>
   /// Performs a full undo operation by reversing and undoing all commands contained within the compacted nodes.
   /// </summary>
   /// <exception cref="InvalidOperationException">
   /// Thrown if the compacted node list is empty or an undo operation fails.
   /// </exception>
   public void FullUndo()
   {
      List<HistoryNode> invertedNodes = new(CompactedNodes);
      invertedNodes.Reverse();
      foreach (var node in invertedNodes)
         node.Command.Undo();
   }

   /// <summary>
   /// Executes a full redo operation on all commands within the compacted nodes of the current compact history node.
   /// </summary>
   /// <exception cref="InvalidOperationException">Thrown if the compacted nodes contain commands that cannot be redone.</exception>
   public void FullRedo()
   {
      foreach (var node in CompactedNodes)
         node.Command.Redo();
   }

   /// <summary>
   /// Determines whether the current compacted history node can perform a step undo operation.
   /// </summary>
   /// <remarks>
   /// A step undo operation allows reverting one step within the compacted nodes managed by the current
   /// <see cref="CompactHistoryNode"/>. This property evaluates to <c>true</c> if there are one or more
   /// steps available to undo within the compacted sequence, based on the current position in the node.
   /// </remarks>
   public bool HasStepUndo => _current > 0;
   /// <summary>
   /// Gets a value indicating whether there is a later step available for redo
   /// in the sequence of compacted nodes.
   /// </summary>
   /// <remarks>
   /// This property evaluates to true if the current position in the compacted nodes
   /// is not at the last node, indicating that a step redo operation can be performed.
   /// </remarks>
   public bool HasStepRedo => _current < CompactedNodes.Count - 1;
   public string GetDescription => $"Compacting {CompactedNodes.Count} Nodes";
}