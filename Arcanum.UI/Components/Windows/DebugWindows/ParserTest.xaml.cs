using System.Buffers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.UI.Components.Windows.PopUp;
using Common.UI.MBox;

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class ParserTest : INotifyPropertyChanged
{
   private FilesToTest _selectedFile = FilesToTest.Block;
   private string _outputText = string.Empty;
   private string _inputText = string.Empty;
   private string _time = "0 ms";

   public IEnumerable<FilesToTest> FilesToTestValues => Enum.GetValues<FilesToTest>();

   public FilesToTest SelectedFile
   {
      get => _selectedFile;
      set
      {
         if (value == _selectedFile)
            return;

         _selectedFile = value;
         OnPropertyChanged();
      }
   }

   public string OutputText
   {
      get => _outputText;
      set
      {
         if (value == _outputText)
            return;

         _outputText = value;
         OnPropertyChanged();
      }
   }

   public string InputText
   {
      get => _inputText;
      set
      {
         if (value == _inputText)
            return;

         _inputText = value;
         OnPropertyChanged();
      }
   }

   public string Time
   {
      get => _time;
      set
      {
         if (value == _time)
            return;

         _time = value;
         OnPropertyChanged();
      }
   }

   public ParserTest()
   {
      InitializeComponent();
   }

   public const int MAX_OUTPUT_LENGTH = 10_000;

   public void RunLexer()
   {
      var filePath = ParserTesting.GetFilePath(SelectedFile);
      if (!System.IO.File.Exists(filePath))
         return;

      var source = System.IO.File.ReadAllText(filePath);
      if (source.Length < MAX_OUTPUT_LENGTH)
         InputText = source;
      else
         InputText = $"--- Input truncated due to length ({source.Length} chars) ---";
      var tokenBuffer = ArrayPool<Token>.Shared.Rent(source.Length / 4); // Rent a buffer

      var watch = System.Diagnostics.Stopwatch.StartNew();
      var lexer = new Lexer(source.AsSpan(), tokenBuffer.AsSpan());
      var tokens = lexer.ScanTokens();
      watch.Stop();
      Time = $"Lexing: {watch.ElapsedMilliseconds} ms";

      var sb = new StringBuilder();

      if (tokens.Length > MAX_OUTPUT_LENGTH)
      {
         sb.AppendLine($"--- Output truncated due to length ({tokens.Length} tokens) ---\n");
      }
      else
      {
         if (tokens.Length != 0)
         {
            sb.AppendLine($"--- Tokens Found ({tokens.Length}) ---\n");
            foreach (var token in tokens)
            {
               if (sb.Length > MAX_OUTPUT_LENGTH)
               {
                  sb.AppendLine("\n\n--- Output truncated due to length ---");
                  break;
               }

               sb.AppendLine(token.ToString(source));
            }

            OutputText = sb.ToString();
         }
         else
         {
            OutputText = "No tokens were generated.";
         }
      }

      ArrayPool<Token>.Shared.Return(tokenBuffer);
      Time += $", lines: {tokens[^1].Line}|{source.Length} chars";
   }

   public event PropertyChangedEventHandler? PropertyChanged;

   protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
   }

   protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
   {
      if (EqualityComparer<T>.Default.Equals(field, value))
         return false;

      field = value;
      OnPropertyChanged(propertyName);
      return true;
   }

   private void RunLexer_ButtonClick(object sender, RoutedEventArgs e)
   {
      RunLexer();
   }

   private void RunParser_ButtonClick(object sender, RoutedEventArgs e)
   {
      RunParser();
   }

   private void RunParser()
   {
      try
      {
         Time = string.Empty;
         var filePath = ParserTesting.GetFilePath(SelectedFile);
         if (!System.IO.File.Exists(filePath))
            return;

         var source = System.IO.File.ReadAllText(filePath);
         if (source.Length < MAX_OUTPUT_LENGTH)
            InputText = source;
         else
            InputText = $"--- Input truncated due to length ({source.Length} chars) ---";
         var watch = System.Diagnostics.Stopwatch.StartNew();
         var tokenBuffer = ArrayPool<Token>.Shared.Rent(source.Length / 4); // Rent a buffer
         var lexer = new Lexer(source.AsSpan(), tokenBuffer.AsSpan());
         var tokens = lexer.ScanTokens();
         watch.Stop();
         Time = $"Lexing {watch.ElapsedMilliseconds} ms";
         var lexTime = watch.ElapsedMilliseconds;

         watch.Restart();
         var parser = new Parser(tokens, source, Eu5FileObj.Empty);
         var rn = parser.Parse();
         watch.Stop();

         var sb = new StringBuilder();
         if (tokens.Length > MAX_OUTPUT_LENGTH)
            sb.AppendLine($"--- Output truncated due to length ({tokens.Length} tokens) ---\n");
         else
         {
            Parser.PrintAst(rn, sb, "", source);
            OutputText = sb.ToString();
         }

         Time +=
            $", Parsing {watch.ElapsedMilliseconds} ms, Total {lexTime + watch.ElapsedMilliseconds} ms";
         Time += $", lines: {tokens[^1].Line}|{source.Length} chars";
         ArrayPool<Token>.Shared.Return(tokenBuffer);
      }
      catch (Exception e)
      {
         MBox.Show($"An error occurred during parsing:\n{e.Message}", "Error", MBoxButton.OK, MessageBoxImage.Error);
      }
   }
}