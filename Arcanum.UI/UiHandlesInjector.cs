using Arcanum.UI.Components.UIHandles;
using Common.UI;

namespace Arcanum.UI;

public static class UiHandlesInjector
{
   /// <summary>
   /// Injects all UI handles implementations into the UIHandle singleton instance.
   /// </summary>
   public static void InjectUiHandles()
   {
      UIHandle.Instance.UIUtils = new UIUtilsImpl();
      UIHandle.Instance.PopUpHandle = new PopUpHandleImpl();
      UIHandle.Instance.MainWindowsHandle = new MainWindowHandleImpl();
   }
}