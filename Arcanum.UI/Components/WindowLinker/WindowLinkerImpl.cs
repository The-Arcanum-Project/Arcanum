using Arcanum.API.UtilServices;
using Arcanum.UI.Components.Windows.PopUp;

namespace Arcanum.UI.Components.WindowLinker;

public class WindowLinkerImpl : IWindowLinker
{
   public void OpenPropertyGridWindow(object obj)
   {
      new PropertyGridWindow(obj).ShowDialog();
   }
}