using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Arcanum.UI.Components.Helpers;

public class ViewModelTextWriter(Action<string> writeAction) : TextWriter
{
   private readonly Action<string> _writeAction = writeAction ?? throw new ArgumentNullException(nameof(writeAction));
   private readonly StringBuilder _buffer = new();
   private readonly Dispatcher _dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

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

   public override Encoding Encoding => Encoding.UTF8;
}

public class LogViewModel : INotifyPropertyChanged
{
   private readonly StringBuilder _logContent = new();

   public string LogText
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   } = "";

   public StreamWriter LogWriter { get; }

   public LogViewModel()
   {
      var customWriter = new ViewModelTextWriter(newLine =>
      {
         _logContent.AppendLine(newLine);
         LogText = _logContent.ToString();
      });

      LogWriter = new(new TextWriterStream(customWriter)) { AutoFlush = true, };

      Console.SetOut(LogWriter);
   }

   public event PropertyChangedEventHandler? PropertyChanged;

   private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}

// Helper class to adapt a TextWriter to a Stream for StreamWriter
public class TextWriterStream(TextWriter writer) : Stream
{
   public override bool CanRead => false;
   public override bool CanSeek => false;
   public override bool CanWrite => true;

   public override void Flush()
   {
      writer.Flush();
   }

   public override long Length => throw new NotSupportedException();
   public override long Position
   {
      get => throw new NotSupportedException();
      set => throw new NotSupportedException();
   }
   public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
   public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
   public override void SetLength(long value) => throw new NotSupportedException();

   public override void Write(byte[] buffer, int offset, int count)
   {
      writer.Write(Encoding.UTF8.GetString(buffer, offset, count));
   }
}