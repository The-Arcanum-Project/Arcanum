namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser;

public readonly struct Token
{
   public TokenType Type { get; }
   public int Start { get; }    // The starting index in the source string
   public int Length { get; }   // The length of the lexeme
   public int Line { get; }
   public int Column { get; }

   public Token(TokenType type, int start, int length, int line, int column)
   {
      Type = type;
      Start = start;
      Length = length;
      Line = line;
      Column = column;
   }

   /// <summary>
   /// Gets the actual string lexeme by slicing the source text on demand.
   /// This performs a string allocation.
   /// </summary>
   public string GetLexeme(string source)
   {
      if (Length == 0 || Start + Length > source.Length) return "";
      return source.Substring(Start, Length);
   }

   /// <summary>
   /// Returns a string representation for debugging purposes.
   /// </summary>
   public string ToString(string source)
   {
      return $"[Line {Line,3}, Col {Column,3}] {Type,-15} | '{GetLexeme(source)}'";
   }
}

/// <summary>
/// Holds the result of a lexing operation, pairing the original source code
/// with the list of tokens that refer to it.
/// </summary>
public class LexerResult
{
   public string Source { get; }
   public IReadOnlyList<Token> Tokens { get; }

   public LexerResult(string source, IReadOnlyList<Token> tokens)
   {
      Source = source;
      Tokens = tokens;
   }
}