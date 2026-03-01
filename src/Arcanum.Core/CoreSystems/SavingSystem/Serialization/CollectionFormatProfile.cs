using System.ComponentModel;

namespace Arcanum.Core.CoreSystems.SavingSystem.Serialization;

public class CollectionFormatProfile
{
   [Description("How the items are arranged (Vertical, Grid, Flow, Compact).")]
   public CollectionLayoutMode LayoutMode { get; set; } = CollectionLayoutMode.Flow;

   [Description("For Grid/Flow: Maximum items before a line break.")]
   public int ItemsPerRow { get; set; } = 7;

   [Description("Align items into columns using spaces.")]
   public bool AlignColumns { get; set; } = false;

   [Description("If AlignColumns is true, the fixed width of each column.")]
   public int ColumnWidth { get; set; } = 5;

   [Description("Sort items before saving.")]
   public CollectionSortMode SortMode { get; set; } = CollectionSortMode.None;

   [Description("Override global setting for writing empty collection headers.")]
   public bool WriteEmpty { get; set; } = false; // Is overwritten by owner setting 
}

public enum CollectionLayoutMode
{
   Flow,
   Grid,
   Vertical,
   Compact,
}

public enum CollectionSortMode
{
   None,
   Alphabetical,
   Numeric,
}