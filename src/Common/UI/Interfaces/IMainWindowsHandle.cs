using System.Windows;

namespace Common.UI.Interfaces;

public interface IMainWindowsHandle
{
   public event Action OnOpenMainMenuScreen;

   public void OpenMainMenuScreen();

   public void TransferToMainMenuScreen(Window sender, Enum view);

   public void SetToNui(object obj);
}