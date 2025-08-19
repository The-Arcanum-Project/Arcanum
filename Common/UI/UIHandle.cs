using Common.UI.Interfaces;

namespace Common.UI;

public class UIHandle
{
   private static readonly Lazy<UIHandle> LazyInstance = new(() => new());

   public static UIHandle Instance => LazyInstance.Value;

   private UIHandle()
   {
   }
   
   public IPopUpHandle PopUpHandle { get; set; } = null!;
   public IUIUtils UIUtils { get; set; } = null!;
   public IMainWindowsHandle MainWindowsHandle { get; set; } = null!;
}
