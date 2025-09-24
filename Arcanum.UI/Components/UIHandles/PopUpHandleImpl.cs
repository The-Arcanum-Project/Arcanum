using System.Windows;
using Arcanum.UI.Components.Windows.MinorWindows;
using Arcanum.UI.Components.Windows.PopUp;
using Common.UI;
using Common.UI.Interfaces;
using Common.UI.MBox;

namespace Arcanum.UI.Components.UIHandles;

public class PopUpHandleImpl : IPopUpHandle
{
   public void NavigateToSetting(string[] path)
   {
      var sw = SettingsWindow.ShowSettingsWindow();
      sw.NavigateToSetting(path);
      UIHandle.Instance.UIUtils.OpenWindowOnSTAThread(sw, true);
   }

   public void OpenPropertyGridWindow(object obj)
   {
      UIHandle.Instance.UIUtils.OpenWindowOnSTAThread(GetPropertyGridOrCollectionView(obj), true);
   }

   public MBoxResult ShowMBox(string message,
                              string title = "Message",
                              MBoxButton buttons = MBoxButton.OK,
                              MessageBoxImage icon = MessageBoxImage.Asterisk,
                              int height = -1,
                              int width = -1)
   {
      return MBox.Show(message, title, buttons, icon, height, width);
   }

   public Window GetPropertyGridOrCollectionView(object? obj)
   {
      if (obj is null)
         throw new ArgumentNullException(nameof(obj), "Object cannot be null");

      if (obj is System.Collections.IEnumerable enumerable)
         return new BaseCollectionView(enumerable);

      return new PropertyGridWindow(obj);
   }
}