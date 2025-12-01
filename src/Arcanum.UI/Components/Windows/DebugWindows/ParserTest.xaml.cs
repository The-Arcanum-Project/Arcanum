using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class ParserTest : INotifyPropertyChanged
{
   public IEnumerable<FilesToTest> FilesToTestValues => Enum.GetValues<FilesToTest>();

   public FilesToTest SelectedFile
   {
      get;
      set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = FilesToTest.Block;

   public string OutputText
   {
      get;
      set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = string.Empty;

   public string InputText
   {
      get;
      set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = string.Empty;

   public string Time
   {
      get;
      set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = "0 ms";

   public ParserTest()
   {
      InitializeComponent();
   }

   public const int MAX_OUTPUT_LENGTH = 10_000;

   public void RunLexer()
   {
      var filePath = ParserTesting.GetFilePath(SelectedFile);
      if (!File.Exists(filePath))
         return;

      var source = File.ReadAllText(filePath);
      if (source.Length < MAX_OUTPUT_LENGTH)
         InputText = source;
      else
         InputText = $"--- Input truncated due to length ({source.Length} chars) ---";
      var lexer = new Lexer(source);

      var watch = Stopwatch.StartNew();
      var result = lexer.ScanTokens();
      watch.Stop();
      Time = $"Lexing: {watch.ElapsedMilliseconds} ms";

      var sb = new StringBuilder();

      if (result.Tokens.Count > MAX_OUTPUT_LENGTH)
      {
         sb.AppendLine($"--- Output truncated due to length ({result.Tokens.Count} tokens) ---\n");
      }
      else
      {
         if (result.Tokens.Count != 0)
         {
            sb.AppendLine($"--- Tokens Found ({result.Tokens.Count}) ---\n");
            foreach (var token in result.Tokens)
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

      Time += $", lines: {result.Tokens[^1].Line}|{source.Length} chars";
   }

   public event PropertyChangedEventHandler? PropertyChanged;

   protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
      PropertyChanged?.Invoke(this, new(propertyName));
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
      var filePath = ParserTesting.GetFilePath(SelectedFile);
      if (!File.Exists(filePath))
         return;

      var source = File.ReadAllText(filePath);
      if (source.Length < MAX_OUTPUT_LENGTH)
         InputText = source;
      else
         InputText = $"--- Input truncated due to length ({source.Length} chars) ---";
      var lexer = new Lexer(source);
      var watch = Stopwatch.StartNew();
      var result = lexer.ScanTokens();
      watch.Stop();
      Time = $"Lexing {watch.ElapsedMilliseconds} ms";
      var lexTime = watch.ElapsedMilliseconds;
      watch.Restart();
      var parser = new Parser(result);
      var ast = parser.Parse();
      watch.Stop();

      var sb = new StringBuilder();
      if (result.Tokens.Count > MAX_OUTPUT_LENGTH)
         sb.AppendLine($"--- Output truncated due to length ({result.Tokens.Count} tokens) ---\n");
      else
      {
         Parser.PrintAst(ast, sb, "", source);
         OutputText = sb.ToString();
      }

      Time +=
         $", Parsing {watch.ElapsedMilliseconds} ms, Total {lexTime + watch.ElapsedMilliseconds} ms";
      Time += $", lines: {result.Tokens[^1].Line}|{source.Length} chars";
   }
}