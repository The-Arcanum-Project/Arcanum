using System.Windows;
using Arcanum.API.UI;

namespace Arcanum.API.UtilServices;

public interface IWindowLinker
{
   public void OpenPropertyGridWindow(object obj);
   public void OpenMainMenuScreen();

   public MBoxResult ShowMBox(
      string message,
      string title = "Message",
      MBoxButton buttons = MBoxButton.OK,
      MessageBoxImage icon = MessageBoxImage.Information,
      int height = 150,
      int width = 400);

   public Window GetPropertyGridOrCollectionView(object? obj);
}