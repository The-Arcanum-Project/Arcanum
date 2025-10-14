using System.Windows;
using System.Windows.Controls;
using Arcanum.UI.Components.Helpers;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class LogWindow
{
   public LogWindow()
   {
      InitializeComponent();
   }

   private bool _autoScroll = true;

   private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
   {
      if (sender is not ScrollViewer sw)
         return;

      if (e.ExtentHeightChange == 0)
         _autoScroll = Math.Abs(sw.VerticalOffset - sw.ScrollableHeight) < 0.1;

      if (_autoScroll && e.ExtentHeightChange > 0)
         sw.ScrollToEnd();
   }
}