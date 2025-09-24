using System.Windows;
using Common.UI.MBox;

namespace Common.UI.Interfaces;

public interface IPopUpHandle
{
   /// <summary>
   /// Opens a new settings window and navigates to the specified property.
   /// </summary>
   /// <returns></returns>
   public void NavigateToSetting(string[] path);

   public void OpenPropertyGridWindow(object obj);

   public MBoxResult ShowMBox(
      string message,
      string title = "Message",
      MBoxButton buttons = MBoxButton.OK,
      MessageBoxImage icon = MessageBoxImage.Information,
      int height = -1,
      int width = -1);

   public Window GetPropertyGridOrCollectionView(object? obj);
}