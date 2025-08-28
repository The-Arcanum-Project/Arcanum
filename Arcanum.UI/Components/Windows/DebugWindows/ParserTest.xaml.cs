using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser;

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class ParserTest : INotifyPropertyChanged 
{
   private FilesToTest _selectedFile = FilesToTest.Block;
   private string _outputText = string.Empty;
   private string _inputText = string.Empty;
   
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

   public ParserTest()
   {
      InitializeComponent();
   }
   
   public void RunLexer()
   {
      var filePath = ParserTesting.GetFilePath(SelectedFile);
      if (!System.IO.File.Exists(filePath))
         return;
      
      var source = System.IO.File.ReadAllText(filePath);
      InputText = source;
      var lexer = new Lexer(source);
      var tokens = lexer.ScanTokens();
      
      if (tokens.Count != 0)
      {
         OutputText = $"--- Tokens Found ({tokens.Count}) ---\n";
         foreach (var token in tokens)
            OutputText += token + "\n";
      }
      else
      {
         OutputText = "No tokens were generated.";
      }
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
      
   }
}