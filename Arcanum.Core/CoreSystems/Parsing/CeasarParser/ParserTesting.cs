using System.IO;

namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser;

public enum FilesToTest
{
   ArrayDeclarations,
   Block,
   BlockWithContent,
   BlockWithSeveralContents,
   BracesOnNextLine,
   ColorCases,
   ComplexCase,
   ContentSeparators,
   InlineMath,
   NestedBlocks,
}

public static class ParserTesting
{
   public static void RunLexer(FilesToTest files)
   {
      var filePath = GetFilePath(files);
      if (!File.Exists(filePath))
         return;

      var source = File.ReadAllText(filePath);

      var lexer = new Lexer(source);
      var tokens = lexer.ScanTokens();

      if (tokens.Count != 0)
      {
         Console.WriteLine($"--- Tokens Found ({tokens.Count}) ---");
         foreach (var token in tokens)
            Console.WriteLine(token);
      }
      else
      {
         Console.WriteLine("No tokens were generated.");
      }
   }

   public static string GetFilePath(FilesToTest file)
   {
      const string location =
         @"C:\Users\david\source\repos\Arcanum\Arcanum.Core\CoreSystems\Parsing\CeasarParser\TestFiles";
      return Path.Combine(location, file + ".txt");
   }
}