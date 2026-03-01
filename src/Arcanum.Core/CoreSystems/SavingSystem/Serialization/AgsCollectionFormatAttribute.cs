namespace Arcanum.Core.CoreSystems.SavingSystem.Serialization;

[AttributeUsage(AttributeTargets.Property)]
public class AgsCollectionFormatAttribute : Attribute
{
   public CollectionLayoutMode LayoutMode { get; set; } = CollectionLayoutMode.Flow;

   public int ItemsPerRow { get; set; } = 1;

   public bool AlignColumns { get; set; } = false;

   public int ColumnWidth { get; set; } = 5;

   public CollectionSortMode SortMode { get; set; } = CollectionSortMode.None;

   /// <summary>
   /// Note: This may be overwritten by the owner setting if logic dictates.
   /// </summary>
   public bool WriteEmpty { get; set; } = false;
}