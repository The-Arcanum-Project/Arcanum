namespace Arcanum.UI.Components.Windows.MinorWindows.PopUpEditors;

public class CollectionEditResult<T>
{
   public T[][] ToAddPerCollection { get; set; } = [];
   public T[][] ToRemovePerCollection { get; set; } = [];
   public bool Canceled { get; set; } = true;
}