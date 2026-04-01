#region

using System.Windows.Documents;
using System.Windows.Threading;
using Arcanum.UI.Components.Helpers;

#endregion

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class LogWindow
{
   private readonly FlowDocument _document;
   private readonly DispatcherTimer _renderTimer;

   private const bool AUTO_SCROLL = true;

   public LogWindow()
   {
      InitializeComponent();

      _document = new();
      LogDisplay.Document = _document;

      _renderTimer = new(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(100) };
      _renderTimer.Tick += ProcessLogs;
      _renderTimer.Start();
   }

   private void ProcessLogs(object? sender, EventArgs e)
   {
      if (DataContext is not LogViewModel vm)
         return;

      var added = false;
      while (vm.PendingLogs.TryDequeue(out var entry))
      {
         var p = new Paragraph(new Run(entry.Message)) { Foreground = entry.Color };
         _document.Blocks.Add(p);
         added = true;
      }

      while (_document.Blocks.Count > 1000)
         _document.Blocks.Remove(_document.Blocks.FirstBlock);

      if (added && AUTO_SCROLL)
         LogDisplay.ScrollToEnd();
   }
}