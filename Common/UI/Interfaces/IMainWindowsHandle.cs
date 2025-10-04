using System.Windows;

namespace Common.UI.Interfaces;

public interface IMainWindowsHandle
{
   public void OpenMainMenuScreen();

   public void TransferToMainMenuScreen(Window sender, Enum view);
}