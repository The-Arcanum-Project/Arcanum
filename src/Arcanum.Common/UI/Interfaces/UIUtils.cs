using System.Windows;

namespace Common.UI.Interfaces;

public interface IUIUtils
{
   public void OpenWindowOnSTAThread(Window window, bool asDialog);
}