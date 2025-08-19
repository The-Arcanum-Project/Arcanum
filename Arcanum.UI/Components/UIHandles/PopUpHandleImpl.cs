using Arcanum.UI.Components.Windows.MinorWindows;
using Common.UI;
using Common.UI.Interfaces;

namespace Arcanum.UI.Components.UIHandles;

public class PopUpHandleImpl : IPopUpHandle
{
   public void NavigateToSetting(string[] path)
   {
      var sw = SettingsWindow.ShowSettingsWindow();
      sw.NavigateToSetting(path);
      UIHandle.Instance.UIUtils.OpenWindowOnSTAThread(sw, true);
   }
}