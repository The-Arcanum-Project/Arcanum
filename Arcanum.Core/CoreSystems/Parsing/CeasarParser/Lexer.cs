namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser;

/// <summary>
/// Scans a source code string and produces a sequence of tokens.
/// </summary>
public class Lexer
{
   private readonly string _source;
   private readonly List<Token> _tokens = new();

   private int _start = 0; // Start of the current lexeme
   private int _current = 0; // Current character being processed
   private int _line = 1;
   private int _column = 1;

   // A map to distinguish keywords from identifiers
   private static readonly Dictionary<string, TokenType> Keywords = new()
   {
      { "yes", TokenType.Yes }, { "no", TokenType.No }
   };

   public Lexer(string source)
   {
      _source = source;
   }

   /// <summary>
   /// Scans the entire source text and returns a list of all tokens.
   /// </summary>
   public List<Token> ScanTokens()
   {
      while (!IsAtEnd())
      {
         _start = _current;
         ScanToken();
      }

      _tokens.Add(new(TokenType.EndOfFile, "", _line, _column));
      return _tokens;
   }

   private void ScanToken()
   {
      var c = Advance();
      switch (c)
      {
         // Single-character tokens
         case '{':
            AddToken(TokenType.LeftBrace);
            break;
         case '}':
            AddToken(TokenType.RightBrace);
            break;
         case ']':
            AddToken(TokenType.RightBracket);
            break;
         case '+':
            AddToken(TokenType.Plus);
            break;
         case '-':
            AddToken(TokenType.Minus);
            break;
         case '*':
            AddToken(TokenType.Multiply);
            break;

         // Operators (one or two characters)
         case '=':
            AddToken(TokenType.Equals);
            break;
         case '<':
            AddToken(Match('=') ? TokenType.LessOrEqual : TokenType.Unexpected);
            break;
         case '>':
            AddToken(Match('=') ? TokenType.GreaterOrEqual : TokenType.Unexpected);
            break;
         case '?':
            AddToken(Match('=') ? TokenType.QuestionEquals : TokenType.Unexpected);
            break;

         // Special handling for @
         case '@':
            if (Match('['))
            {
               AddToken(TokenType.AtLeftBracket);
            }
            else
            {
               ScanIdentifier(isAtIdentifier: true);
            }

            break;

         // Comments and Division
         case '#':
            // A comment goes until the end of the line.
            while (Peek() != '\n' && !IsAtEnd())
               Advance();
            break;
         case '/':
            AddToken(TokenType.Divide);
            break;

         // Whitespace (ignored)
         case ' ':
         case '\r':
         case '\t':
            // Ignore whitespace.
            break;

         case '\n':
            _line++;
            _column = 0; // It will be incremented to 1 by the Advance() call's column logic
            break;

         // String literals
         case '"':
            ScanString();
            break;

         default:
            if (IsDigit(c))
            {
               ScanNumber();
            }
            else if (IsAlpha(c))
            {
               ScanIdentifier();
            }
            else
            {
               // If we don't recognize the character, create an error token.
               AddToken(TokenType.Unexpected, $"Unexpected character: {c}");
            }

            break;
      }
   }

   private void ScanIdentifier(bool isAtIdentifier = false)
   {
      // The loop condition is the only part that changes here.
      while (IsIdentifierContinuationChar(Peek())) Advance(); // <-- CHANGED

      var text = _source.Substring(_start, _current - _start);
        
      if (isAtIdentifier)
      {
         // Remove the leading '@' for the lexeme
         AddToken(TokenType.AtIdentifier, text[1..]);
      }
      else
      {
         if (Keywords.TryGetValue(text, out var type))
         {
            AddToken(type);
         }
         else
         {
            AddToken(TokenType.Identifier);
         }
      }
   }

   private void ScanNumber()
   {
      while (IsDigit(Peek()))
         Advance();

      // Look for a fractional part.
      if (Peek() == '.' && IsDigit(PeekNext()))
      {
         // Consume the "."
         Advance();

         while (IsDigit(Peek()))
            Advance();
      }

      AddToken(TokenType.Number);
   }

   private void ScanString()
   {
      while (Peek() != '"' && !IsAtEnd())
      {
         if (Peek() == '\n')
            _line++; // Support multi-line strings
         Advance();
      }

      if (IsAtEnd())
      {
         // Unterminated string.
         AddToken(TokenType.Unexpected, "Unterminated string.");
         return;
      }

      // The closing ".
      Advance();

      // Trim the surrounding quotes.
      var value = _source.Substring(_start + 1, _current - _start - 2);
      AddToken(TokenType.String, value);
   }

   #region Helper Methods

   private bool IsAtEnd() => _current >= _source.Length;

   private char Advance()
   {
      _column++;
      return _source[_current++];
   }

   private void AddToken(TokenType type, object? literal = null)
   {
      var text = _source.Substring(_start, _current - _start);
      // Use the literal if provided (for strings), otherwise use the raw text
      var lexeme = literal as string ?? text;
      _tokens.Add(new(type, lexeme, _line, _column - text.Length));
   }

   private bool Match(char expected)
   {
      if (IsAtEnd())
         return false;
      if (_source[_current] != expected)
         return false;

      _current++;
      _column++;
      return true;
   }

   private char Peek()
   {
      if (IsAtEnd())
         return '\0';

      return _source[_current];
   }

   private char PeekNext()
   {
      if (_current + 1 >= _source.Length)
         return '\0';

      return _source[_current + 1];
   }

   private static bool IsDigit(char c) => c is >= '0' and <= '9';

   // IsAlpha remains, as it's used to validate the START of an identifier.
   private static bool IsAlpha(char c) => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
    
   // IsAlphaNumeric is no longer used by ScanIdentifier, but we can keep it.
   private static bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

   /// <summary>
   /// Checks for valid characters inside an identifier (after the first character).
   /// </summary>
   private static bool IsIdentifierContinuationChar(char c) // <-- NEW HELPER METHOD
   {
      return IsAlphaNumeric(c) || c == ':' || c == '.';
   }

   #endregion
}