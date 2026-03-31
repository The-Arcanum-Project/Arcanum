using Common.UI.Interfaces;
using Common.UI.Map;
using Common.UI.State;

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

   public IMapHandle MapHandle { get; set; } = null!;
   public ILogWindowHandle LogWindowHandle { get; set; } = null!;
   public IMapInterface MapInterface { get; set; } = null!;
   public IStateHandle StateHandle { get; set; } = null!;
}