using System.Buffers;
using System.Text;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

public static class PLHelper
{
   public static RootNode ParseFile(Eu5FileObj fileObj)
   {
      var source = IO.IO.ReadAllText(fileObj.Path.FullPath, Encoding.UTF8);
      if (string.IsNullOrWhiteSpace(source))
         return new();

      var tokenBuffer = ArrayPool<Token>.Shared.Rent(source.Length / 4); // Rent a buffer
      var lexer = new Lexer(source.AsSpan(), tokenBuffer.AsSpan());
      var tokens = lexer.ScanTokens();

      var parser = new Parser(tokens, source, fileObj);
      var rn = parser.Parse();
      ArrayPool<Token>.Shared.Return(tokenBuffer);
      return rn;
   }

   public static RootNode ParseFile(Eu5FileObj fileObj, out string source)
   {
      source = IO.IO.ReadAllText(fileObj.Path.FullPath, Encoding.UTF8)!;
      if (string.IsNullOrWhiteSpace(source))
      {
         source = string.Empty;
         return new();
      }

      var tokenBuffer = ArrayPool<Token>.Shared.Rent(source.Length / 4); // Rent a buffer
      var lexer = new Lexer(source.AsSpan(), tokenBuffer.AsSpan());
      var tokens = lexer.ScanTokens();

      var parser = new Parser(tokens, source, fileObj);
      var rn = parser.Parse();
      ArrayPool<Token>.Shared.Return(tokenBuffer);
      return rn;
   }
}