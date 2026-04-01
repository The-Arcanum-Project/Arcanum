#region

using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

#endregion

namespace Arcanum.UI.Components.Helpers;

public class ViewModelTextWriter(Action<string> writeAction) : TextWriter
{
   private readonly StringBuilder _buffer = new();
   private readonly Dispatcher _dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
   private readonly Action<string> _writeAction = writeAction ?? throw new ArgumentNullException(nameof(writeAction));

   public override Encoding Encoding => Encoding.UTF8;

   public override void Write(char value)
   {
      if (_dispatcher == null! || _dispatcher.HasShutdownStarted)
         return;

      if (value == '\n')
         _dispatcher.Invoke(() =>
         {
            _writeAction(_buffer.ToString());
            _buffer.Clear();
         });
      else if (value != '\r')
         _buffer.Append(value);
   }

   public override void Write(string? value)
   {
      if (_dispatcher == null! || _dispatcher.HasShutdownStarted)
         return;

      if (string.IsNullOrEmpty(value))
         return;

      var lines = value.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

      for (var i = 0; i < lines.Length; i++)
      {
         _buffer.Append(lines[i]);
         if (i < lines.Length - 1)
            _dispatcher.Invoke(() =>
            {
               _writeAction(_buffer.ToString());
               _buffer.Clear();
            });
      }
   }
}

public record LogEntry(string Message, Brush Color);

public class LogViewModel : INotifyPropertyChanged
{
   public LogViewModel()
   {
      var customWriter = new ViewModelTextWriter(newLine =>
      {
         var color = (Brush)Application.Current?.FindResource("DefaultForeColorBrush")!;
         if (newLine.Contains("[ERR]"))
            color = Brushes.Tomato;
         else if (newLine.Contains("[WRN]"))
            color = Brushes.Gold;
         else if (newLine.Contains("[INF]"))
            color = Brushes.SkyBlue;
         else if (newLine.Contains("[DBG]"))
            color = Brushes.LightGreen;

         PendingLogs.Enqueue(new(newLine, color));
      });

      LogWriter = new(new TextWriterStream(customWriter)) { AutoFlush = true };
      Console.SetOut(LogWriter);
   }

   public ConcurrentQueue<LogEntry> PendingLogs { get; } = new();

   public StreamWriter LogWriter { get; }
   public event PropertyChangedEventHandler? PropertyChanged;
}

// Helper class to adapt a TextWriter to a Stream for StreamWriter
public class TextWriterStream(TextWriter writer) : Stream
{
   public override bool CanRead => false;
   public override bool CanSeek => false;
   public override bool CanWrite => true;

   public override long Length => throw new NotSupportedException();
   public override long Position
   {
      get => throw new NotSupportedException();
      set => throw new NotSupportedException();
   }

   public override void Flush()
   {
      writer.Flush();
   }

   public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
   public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
   public override void SetLength(long value) => throw new NotSupportedException();

   public override void Write(byte[] buffer, int offset, int count)
   {
      writer.Write(Encoding.UTF8.GetString(buffer, offset, count));
   }
}