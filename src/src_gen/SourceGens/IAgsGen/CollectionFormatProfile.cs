namespace ParserGenerator.IAgsGen;

public class CollectionFormatProfile
{
   public CollectionLayoutMode LayoutMode { get; set; } = CollectionLayoutMode.Flow;

   public int ItemsPerRow { get; set; } = 1;

   public bool AlignColumns { get; set; } = false;

   public int ColumnWidth { get; set; } = 5;

   public CollectionSortMode SortMode { get; set; } = CollectionSortMode.None;

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