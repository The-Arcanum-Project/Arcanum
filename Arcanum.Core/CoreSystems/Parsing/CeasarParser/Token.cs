namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser;

/// <summary>
/// Represents a token scanned from the source code.
/// It is an immutable struct for efficiency.
/// </summary>
public readonly struct Token
{
   public TokenType Type { get; }
   public string Lexeme { get; }
   public int Line { get; }
   public int Column { get; }

   public Token(TokenType type, string lexeme, int line, int column)
   {
      Type = type;
      Lexeme = lexeme;
      Line = line;
      Column = column;
   }

   public override string ToString()
   {
      return $"[Line {Line,3}, Col {Column,3}] {Type,-15} | '{Lexeme}'";
   }
}