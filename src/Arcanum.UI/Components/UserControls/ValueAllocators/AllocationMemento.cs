namespace Arcanum.UI.Components.UserControls.ValueAllocators;

public struct AllocationMemento(AllocationItem item)
{
   private readonly AllocationItem _item = item;
   private readonly int _value = item.Value;
   private readonly bool _isLocked = item.IsLocked;

   public void Restore()
   {
      // Restore lock state first to allow value changes
      _item.IsLocked = _isLocked;
      // Use SetValueInternal to bypass logic loops
      _item.SetValueInternal(_value);
   }
}