namespace Arcanum.UI.Components.Windows.MinorWindows.PopUpEditors;

public enum EditState
{
   NotPresent, // In the "Available" pool initially.
   InSome, // Exists in some, but not all, source collections.
   InAll, // Exists in all source collections.
   MarkedForRemoval, // User has decided to remove this item.
   MarkedForAddition, // User has decided to add this item from the available pool.
}