namespace Arcanum.UI.Components.UserControls.ValueAllocators;

public readonly struct AllocationMemento(AllocationItem item)
{
   private readonly int _value = item.Value;
   private readonly bool _isLocked = item.IsLocked;

   public void Restore()
   {
      // Restore lock state first to allow value changes
      item.IsLocked = _isLocked;
      // Use SetValueInternal to bypass logic loops
      item.SetValueInternal(_value);
   }
}