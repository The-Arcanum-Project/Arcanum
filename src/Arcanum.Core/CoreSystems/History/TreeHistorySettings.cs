namespace Arcanum.Core.CoreSystems.History;

public enum AutoCompactingStrategy
{
   None,
   AfterXSize,
   EveryXMinutes
}

/// <summary>
/// 
/// </summary>
public class TreeHistorySettings
{
   /// <summary>
   /// The minimum number of nodes which have to share the same target hash to be considered for compaction.
   /// </summary>
   public int MinNumOfEntriesToCompact { get; set; } = 5;
   /// <summary>
   /// The minimum required undo depth to trigger an automatic compaction when the auto-compacting strategy is set to <see cref="AutoCompactingStrategy.AfterXSize"/>.
   /// </summary>
   public int AutoCompactingMinSize { get; set; } = 100;
   /// <summary>
   /// The delay, in milliseconds, before the auto-compaction process is triggered  when the auto-compacting strategy is set to <see cref="AutoCompactingStrategy.EveryXMinutes"/>.
   /// </summary>
   public int AutoCompactingDelay { get; set; } = 60000 * 5;
   /// <summary>
   /// Determines the strategy to be used for automatic compaction of the command history.
   /// This setting manages whether compaction is disabled, triggered after a certain number of entries,
   /// or initiated on a regular time interval.
   /// </summary>
   public AutoCompactingStrategy CompactingStrategy { get; set; } = AutoCompactingStrategy.AfterXSize;
}

public class LinearHistorySettings
{
   /// <summary>
   /// The maximum number of operations or states that can be retained in the history before older entries are discarded.
   /// </summary>
   public int MaxHistorySize { get; set; } = 1000;
}