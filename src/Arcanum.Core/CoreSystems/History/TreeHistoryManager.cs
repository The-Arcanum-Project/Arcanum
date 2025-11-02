using Arcanum.Core.CoreSystems.History.Commands;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;
using Timer = System.Threading.Timer;

namespace Arcanum.Core.CoreSystems.History;

/// <summary>
/// TreeHistoryManager is responsible for managing the history of commands using a tree structure.
/// It allows undo and redo operations, supporting hierarchical management of command history,
/// including automatic compacting and event-driven alerts related to history depth or intervals.
/// </summary>
public class TreeHistoryManager : IHistoryManager
{
   private int _nodeId;
   private HistoryNode _root;
   private bool _compacting;

   /// <summary>
   /// Represents the settings for the <see cref="TreeHistoryManager"/> instance.
   /// These settings define configuration values such as compaction strategy,
   /// thresholds for automatic compaction, and other behavior modifiers
   /// that influence how the history manager operates.
   /// </summary>
   public TreeHistorySettings Settings { get; }

   public TreeHistoryManager(TreeHistorySettings settings)
   {
      Settings = settings;
      Current = new(_nodeId++, new CInitial(), HistoryEntryType.Normal);
      _root = Current;

      UndoEvent += TriggerCompaction;
      RedoEvent += TriggerCompaction;

      InitializeTimers();
   }

   // Events for when the undo/redo depth changes
   public EventHandler<ICommand?> UndoEvent { get; } = delegate { };
   public EventHandler<ICommand?> RedoEvent { get; } = delegate { };
   public event Action<ICommand>? CommandAdded;

   public event Action<Type, IEu5Object[]>? ModifiedType;

   private int _lastCompactionDepth;
   private Timer? _autoCompactingTimer;
   private Timer? _updateToolStripTimer;
   private DateTime _nextCompactionTime = DateTime.Now;

   public void Add(ICommand command) => AddCommand(command);

   /// <summary>
   /// Adds a new command to the history with the specified entry type.
   /// </summary>
   /// <param name="newCommand">The command to be added to the history.</param>
   /// <param name="type">The type of the history entry associated with the command.</param>
   public void AddCommand(ICommand newCommand, HistoryEntryType type = HistoryEntryType.Normal)
   {
      var newNode = new HistoryNode(_nodeId++, newCommand, type, Current);
      Current.Children.Add(newNode);
      Current = newNode;
      CommandAdded?.Invoke(newCommand);
      InvokeTypeUpdate(newCommand);
   }

   private void InvokeTypeUpdate(ICommand command)
   {
      var changedType = command.GetTargetPropertyType();
      if (changedType == null)
         return;

      if (changedType == typeof(JominiColor))
         InvokeTypeUpdate(command.GetTargets()[0].GetType(), command.GetTargets());
      else
         InvokeTypeUpdate(changedType, command.GetTargetProperties() ?? []);
   }

   public void InvokeTypeUpdate(Type type, IEu5Object[] targets)
   {
      ModifiedType?.Invoke(type, targets);
   }

   // Check if there are any commands to undo or redo
   public bool CanUndo => Current.Parent != null! || Current is CompactHistoryNode { HasStepUndo: true };

   // Check if there are any commands to redo
   public bool CanRedo => Current.Children.Count > 0 || Current is CompactHistoryNode { HasStepRedo: true };
   /// <summary>
   /// Represents the current node in the history tree managed by <see cref="TreeHistoryManager"/>.
   /// This property tracks the node that corresponds to the latest command or state in the history.
   /// It is updated whenever a new command is added, undone, or redone.
   /// </summary>
   public HistoryNode Current { get; private set; }

   public ICommand CurrentCommand => Current.Command;

   [Obsolete("Use <Undo(bool)> instead. This has missing functionality in a tree History")]
   public ICommand? Undo() => Undo(false);

   [Obsolete("Use <Redo(bool)> instead. This has missing functionality in a tree History")]
   public ICommand? Redo() => Redo(false);

   /// <summary>
   /// Undoes the most recently executed command if available, optionally performing a step-based undo for compacted history nodes.
   /// </summary>
   /// <param name="stepUndo">Indicates whether to perform a step-based undo for compacted history nodes.</param>
   /// <returns>The command that was undone, or <c>null</c> if no undo operation could be performed.</returns>
   public ICommand? Undo(bool stepUndo)
   {
      if (!CanUndo)
         return null;

      ICommand? undoCommand = null;
      if (Current.EntryType == HistoryEntryType.Compacted && Current is CompactHistoryNode compNode)
      {
         if (!compNode.HasStepUndo)
         {
            Current = Current.Parent;
            return Undo(true); // we have no more Step Undoes left, so we go up one node
         }

         if (stepUndo)
         {
            compNode.StepUndo();
         }
         else
         {
            compNode.FullUndo();
            Current = Current.Parent;
         }
      }
      else
      {
         undoCommand = Current.Command;
         undoCommand.Undo();
         Current = Current.Parent;
      }

      UndoEvent.Invoke(null, undoCommand);
      InvokeTypeUpdate(undoCommand!);
      return undoCommand;
   }

   /// <summary>
   /// Re-does a previously undone command or steps forward in the history,
   /// depending on the specified parameters.
   /// </summary>
   /// <param name="stepRedo">Specifies whether to re-do a single step when the current node is a compact history node.</param>
   /// <param name="childIndex">The index of the child node to navigate to when redoing. If -1, defaults to the last child node.</param>
   /// <returns>The command that was re-done, or null if no action was performed.</returns>
   public ICommand? Redo(bool stepRedo, int childIndex = -1)
   {
      if (childIndex == -1)
         childIndex += Current.Children.Count;
      if (!CanRedo || childIndex >= Current.Children.Count)
         return null;

      ICommand? redoCommand = null;
      if (Current is CompactHistoryNode compNode)
      {
         if (!compNode.HasStepRedo)
         {
            Current = Current.Children[childIndex];
            if (Current is CompactHistoryNode compact)
               compact.FullRedo();
            else
               redoCommand = Current.Command;

            goto end;
         }

         if (stepRedo)
            compNode.GetStepRedoCommand();
         else
            compNode.FullRedo();
      }
      else
      {
         Current = Current.Children[childIndex];
         if (Current is CompactHistoryNode compact)
            compact.FullRedo();
         else
            redoCommand = Current.Command;
      }

      end:
      RedoEvent.Invoke(null, redoCommand);
      redoCommand?.Redo();
      InvokeTypeUpdate(redoCommand!);
      return redoCommand;
   }

   /// <summary>
   /// Reverts the current history state to the specified node identified by its ID.
   /// </summary>
   /// <param name="id">The unique identifier of the target node to which the history should revert.</param>
   public void RevertTo(int id)
   {
      if (id == Current.Id)
         return;

      var (undo, redo) = GetPathBetweenNodes(Current.Id, id);
      RestoreState(undo, redo);
   }

   private void RestoreState(List<HistoryNode> undo, List<HistoryNode> redo)
   {
      foreach (var node in undo)
         if (node is CompactHistoryNode compNode)
            compNode.FullUndo();
         else
            node.Command.Undo(); // Cant use Undo() because it would change the current node

      foreach (var node in redo)
         if (node is CompactHistoryNode compNode)
            compNode.FullRedo();
         else
            node.Command.Redo(); // Cant use Redo() because it would change the current node

      Current = redo[^1];
   }

   /// <summary>
   /// Calculates the undo depth and the total number of undoable commands in the history tree.
   /// </summary>
   /// <returns>A tuple containing the current undo depth as the first value and the total number of undoable commands as the second value.</returns>
   public (int, int) GetUndoDepth()
   {
      var depth = 0;
      var total = 0;
      var node = Current;
      while (node.Parent != null!)
      {
         depth++;
         if (node is CompactHistoryNode compNode)
            total += compNode.CompactedNodes.Count;
         else
            total++;
         node = node.Parent;
      }

      return (depth, total);
   }

   /// <summary>
   /// Calculates and returns the redo depth and total number of redo entries in the history tree.
   /// </summary>
   /// <returns>A tuple where the first value represents the depth of the redo tree
   /// and the second value indicates the total number of redo entries, including compacted nodes if present.</returns>
   public (int, int) GetRedoDepth()
   {
      var depth = 0;
      var total = 0;
      var node = Current;
      while (node.Children.Count > 0)
      {
         depth++;
         if (node.Children[0] is CompactHistoryNode compNode)
            total += compNode.CompactedNodes.Count;
         else
            total++;
         node = node.Children[0];
      }

      return (depth, total);
   }

   public (List<HistoryNode> add, List<HistoryNode> rmv) GetPathToCurrent()
   {
      return GetPathBetweenNodes(Root.Id, Current.Id);
   }

   /// <summary>
   /// Returns the path between two nodes in the history tree.
   /// </summary>
   /// <param name="from"></param>
   /// <param name="to"></param>
   /// <returns></returns>
   private (List<HistoryNode>, List<HistoryNode>) GetPathBetweenNodes(int from, int to)
   {
      List<HistoryNode> p1 = [];
      List<HistoryNode> p2 = [];

      GetPath(_root, p1, from);
      GetPath(_root, p2, to);

      int i = 0,
          intersection = -1;

      var a = i != p1.Count;
      var b = i != p2.Count;

      while (a && b)
      {
         if (a != b)
         {
            intersection = i - 1;
            break;
         }

         if (p1[i] == p2[i])
         {
            i++;
         }
         else
         {
            intersection = i - 1;
            break;
         }

         a = i != p1.Count;
         b = i != p2.Count;
      }

      for (var j = 0; j <= intersection; j++)
      {
         p1.RemoveAt(0);
         p2.RemoveAt(0);
      }

      p1.Reverse();

      return (p1, p2);
   }

   private static bool GetPath(HistoryNode subRoot, List<HistoryNode> path, int id)
   {
      if (subRoot == null!)
         return false;

      path.Add(subRoot);

      if (subRoot.Id == id)
         return true;

      if (subRoot.Children.Any(child => GetPath(child, path, id)))
         return true;

      path.Remove(subRoot);

      return false;
   }

   public HistoryNode Root => _root;
   public bool CanStepRedo
   {
      get
      {
         if (Current is CompactHistoryNode compNode)
            return compNode.HasStepRedo;

         return false;
      }
   }
   public bool CanStepUndo
   {
      get
      {
         if (Current is CompactHistoryNode compNode)
            return compNode.HasStepUndo;

         return false;
      }
   }

   public void Clear()
   {
      _nodeId = 0;
      _root = Current = new(_nodeId++, new CInitial(), HistoryEntryType.Normal);
      Current = _root;
   }

   /// <summary>
   /// Retrieves the node that is a specified number of levels above the given node in the history hierarchy.
   /// </summary>
   /// <param name="node">The starting node from which to determine the node above.</param>
   /// <param name="n">The number of levels to navigate upwards in the hierarchy.</param>
   /// <returns>The node located n levels above the given node. If the root is reached before completing the levels, the root is returned.</returns>
   public static HistoryNode GetNodeAbove(HistoryNode node, int n)
   {
      var current = node;
      for (var i = 0; i < n; i++)
      {
         if (current.Parent == null!)
            return current;

         current = current.Parent;
      }

      return current;
   }

   /// <summary>
   /// Retrieves the history node with the specified unique identifier.
   /// </summary>
   /// <param name="id">The unique identifier of the history node to retrieve.</param>
   /// <returns>The history node with the specified identifier if found; otherwise, null.</returns>
   public HistoryNode GetNodeWithId(int id)
   {
      return GetNodeWithId(_root, id);
   }

   private static HistoryNode GetNodeWithId(HistoryNode node, int id)
   {
      if (node.Id == id)
         return node;

      foreach (var child in node.Children)
      {
         var result = GetNodeWithId(child, id);

         if (result != null!)
            return result;
      }

      return null!;
   }

   // ----------------------------------------- Compacting ----------------------------------------- \\

   /// <summary>
   /// Expands the compacted nodes in the history tree starting from the given node.
   /// </summary>
   /// <param name="node">The root node from which the un-compaction process will begin.</param>
   public static void Uncompact(HistoryNode node)
   {
      for (var i = node.Children.Count - 1; i >= 0; i--)
      {
         Uncompact(node.Children[i]);
         if (node.Children[i] is CompactHistoryNode compNode)
            compNode.UnCompact();
      }
   }

   /// <summary>
   /// Compacts the history tree by aggregating multiple history entries into compact nodes based on defined settings.
   /// This process identifies optimal groups of history entries for compaction, ensuring that a minimum number
   /// of entries are met before creating a compact node. Compact nodes help optimize memory usage and efficiency.
   /// </summary>
   public void Compact()
   {
      if (_compacting)
         return;

      _compacting = true;
      // We need to uncompact the tree first so that we can find all optimal groups
      Uncompact(_root);

      var groups = FindGroups(_root);
      var compGroups = FindCompactableGroups(groups);

      foreach (var group in compGroups)
      {
         if (group.Count < Settings.MinNumOfEntriesToCompact)
            continue;

         var node = new CompactHistoryNode(_nodeId++, group);
         node.InsertInTree();
         if (Current == group[^1])
            Current = node;
      }

      _compacting = false;
   }

   private static List<List<HistoryNode>> FindCompactableGroups(Dictionary<List<int>, List<HistoryNode>> groups)
   {
      var compGroups = new List<List<HistoryNode>>();

      foreach (var group in groups.Values)
      {
         // Sort nodes by ID for consistent order
         var sortedNodes = group.OrderBy(node => node.Id).ToList();

         List<HistoryNode> currentGroup = [];

         for (var i = 0; i < sortedNodes.Count; i++)
         {
            var currentNode = sortedNodes[i];

            // Check if the current group is linear
            if (currentGroup.Count > 0)
            {
               var lastNode = currentGroup[^1];
               if (lastNode.Children.Count != 1 || lastNode.Children[0] != currentNode)
               {
                  // Linearity breaks, finalize the current group
                  if (currentGroup.Count > 1)
                     compGroups.Add(currentGroup);
                  currentGroup.Clear();
               }
            }

            // Add the current node to the current group
            currentGroup.Add(currentNode);

            // Handle the last node in the list
            if (i == sortedNodes.Count - 1 && currentGroup.Count > 1)
               compGroups.Add([..currentGroup]);
         }
      }

      return compGroups;
   }

   private Dictionary<List<int>, List<HistoryNode>> FindGroups(HistoryNode root)
   {
      var groups = new Dictionary<List<int>, List<HistoryNode>>(new ListComparer<int>());
      TraverseTree(root,
                   node =>
                   {
                      if (node.IsCompacted)
                         return;

                      // Add the node to the dictionary based on its targets
                      if (!groups.TryGetValue(node.Command.GetTargetHash(), out var nodeList))
                      {
                         nodeList = [];
                         groups[node.Command.GetTargetHash()] = nodeList;
                      }

                      nodeList.Add(node);
                   });

      return groups;
   }

   private static void TraverseTree(HistoryNode node, Action<HistoryNode> action)
   {
      if (node == null!)
         return;

      action(node);
      for (var i = node.Children.Count - 1; i >= 0; i--)
         TraverseTree(node.Children[i], action);
   }

   // ========================================= Auto compacting code ========================================= \\

   /// <summary>
   /// Event triggered when the undo depth exceeds a specified size threshold,
   /// as defined by <see cref="TreeHistorySettings.AutoCompactingMinSize"/> and the current compaction state.
   /// This event provides the remaining size until the next automatic compaction is initiated.
   /// </summary>
   public readonly EventHandler<int> CompactionInXSize = delegate { };

   /// <summary>
   /// Event triggered at regular intervals based on the delay specified in
   /// <see cref="TreeHistorySettings.AutoCompactingDelay"/> when the compacting strategy is
   /// set to <see cref="AutoCompactingStrategy.EveryXMinutes"/>.
   /// The event provides the remaining time until the next scheduled compaction.
   /// </summary>
   public readonly EventHandler<TimeSpan> CompactingInXMinutes = delegate { };

   private void TriggerCompaction(object? sender, ICommand? command)
   {
      var (undoDepth, _) = GetUndoDepth();
      if (Settings.CompactingStrategy == AutoCompactingStrategy.AfterXSize)
      {
         // We only compact if the undo depth is greater than the last compaction depth and the amount to trigger a compaction,
         // to not end in a compaction loop ast compaction is an expensive operation
         if (undoDepth >= Settings.AutoCompactingMinSize + _lastCompactionDepth)
         {
            Compact();
            (_lastCompactionDepth, var _) = GetUndoDepth();
         }

         CompactionInXSize.Invoke(null, Settings.AutoCompactingMinSize + _lastCompactionDepth - undoDepth);
      }
   }

   private void InitializeTimers()
   {
      if (Settings.CompactingStrategy != AutoCompactingStrategy.EveryXMinutes)
         return;

      StopTimers();
      _nextCompactionTime = DateTime.Now + TimeSpan.FromMinutes(Settings.AutoCompactingDelay);

      _autoCompactingTimer = new(OnTimerTick, null, Settings.AutoCompactingDelay, Settings.AutoCompactingDelay);

      _updateToolStripTimer = new(_ => CompactingInXMinutes.Invoke(null, _nextCompactionTime - DateTime.Now),
                                  null,
                                  TimeSpan.Zero,
                                  TimeSpan.FromSeconds(5));
   }

   private void OnTimerTick(object? sender)
   {
      TriggerCompaction(null, null);
      _nextCompactionTime = DateTime.Now + TimeSpan.FromMinutes(Settings.AutoCompactingDelay);
   }

   private void StopTimers()
   {
      _autoCompactingTimer?.Dispose();
      _updateToolStripTimer?.Dispose();
   }
}

/// <summary>
/// ListComparer is a utility class that provides logic to compare two lists for equality
/// and generate consistent hash codes for lists of elements. This class is designed to be
/// generic, allowing comparison of lists containing any type of elements.
/// </summary>
/// <typeparam name="T">The type of elements in the lists.</typeparam>
internal class ListComparer<T> : IEqualityComparer<List<T>>
{
   public bool Equals(List<T>? x, List<T>? y)
   {
      if (x == null || y == null)
         return false;
      if (x.Count != y.Count)
         return false;

      for (var i = 0; i < x.Count; i++)
         if (!EqualityComparer<T>.Default.Equals(x[i], y[i]))
            return false;

      return true;
   }

   public int GetHashCode(List<T> obj)
   {
      unchecked
      {
         var hash = 19;
         foreach (var item in obj)
            hash = hash * 31 + (item != null ? item.GetHashCode() : 1);
         return hash;
      }
   }
}