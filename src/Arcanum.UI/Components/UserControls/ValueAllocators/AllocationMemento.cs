namespace Arcanum.UI.Components.UserControls.ValueAllocators;

public struct AllocationMemento
{
   public AllocationItem Item;
   public int Value;
   public bool IsLocked;

   public AllocationMemento(AllocationItem item)
   {
      Item = item;
      Value = item.Value;
      IsLocked = item.IsLocked;
   }

   public void Restore()
   {
      // Restore lock state first to allow value changes
      Item.IsLocked = IsLocked;
      // Use SetValueInternal to bypass logic loops
      Item.SetValueInternal(Value);
   }
}