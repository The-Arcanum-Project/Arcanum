using System.Runtime.CompilerServices;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

/// <summary>
/// Scans a source code string and produces a sequence of tokens. This implementation
/// is optimized to avoid string allocations during the scanning process.
/// </summary>
public class Lexer
{
   private readonly string _source;
   private readonly List<Token> _tokens = [];

   private int _start;
   private int _current;
   private int _line = 1;
   private int _column = 1;

   public Lexer(string source)
   {
      _source = source;
   }

   /// <summary>
   /// Scans the entire source text and returns a LexerResult containing the source and tokens.
   /// </summary>
   public LexerResult ScanTokens()
   {
      while (!IsAtEnd())
      {
         _start = _current;
         ScanToken();
      }

      _tokens.Add(new(TokenType.EndOfFile, _source.Length, 0, _line, _column));
      return new(_source, _tokens);
   }

   // ReSharper disable once CyclomaticComplexity
   private void ScanToken()
   {
      var c = Advance();
      switch (c)
      {
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
         case '=':
            AddToken(TokenType.Equals);
            break;
         case '!':
            AddToken(Match('=') ? TokenType.NotEquals : TokenType.Unexpected);
            break;
         case '<':
            AddToken(Match('=') ? TokenType.LessOrEqual : TokenType.Less);
            break;
         case '>':
            AddToken(Match('=') ? TokenType.GreaterOrEqual : TokenType.Greater);
            break;
         case '?':
            AddToken(Match('=') ? TokenType.QuestionEquals : TokenType.Unexpected);
            break;
         case '@':
            if (Match('['))
               AddToken(TokenType.LeftBracket);
            else
               ScanIdentifier(isAtIdentifier: true);
            break;
         case '#':
            while (Peek() != '\n' && !IsAtEnd())
               Advance();
            break;
         case '/':
            AddToken(TokenType.Divide);
            break;
         case ' ':
         case '\r':
         case '\t':
            break; // Ignore whitespace
         case '\n':
            _line++;
            _column = 0;
            break;
         case '"':
            ScanString();
            break;
         default:
            if (IsDigit(c))
               ScanNumberOrDate();
            else if (IsAlpha(c))
               ScanIdentifier();
            else
               AddToken(TokenType.Unexpected);
            break;
      }
   }

   private void ScanIdentifier(bool isAtIdentifier = false)
   {
      while (IsIdentifierContinuationChar(Peek()))
         Advance();

      if (isAtIdentifier)
      {
         // For @identifier, we store the position of the content after the '@'
         AddToken(TokenType.AtIdentifier, _start + 1, _current - _start - 1);
      }
      else
      {
         var textSpan = _source.AsSpan(_start, _current - _start);
         var type = LookupIdentifierType(textSpan);
         AddToken(type);
      }
   }

   private static TokenType LookupIdentifierType(ReadOnlySpan<char> textSpan)
   {
      return textSpan switch
      {
         "yes" => TokenType.Yes,
         "no" => TokenType.No,
         _ => TokenType.Identifier,
      };
   }

   private void ScanNumberOrDate()
   {
      // Scan the initial integer part
      while (IsDigit(Peek()))
         Advance();

      if (IsAlpha(Peek()))
      {
         while (IsIdentifierContinuationChar(Peek()))
            Advance();
         AddToken(TokenType.Identifier);
         return;
      }

      var dotCount = 0;

      // A loop to handle multiple dot-separated segments
      while (Peek() == '.' && IsDigit(PeekNext()))
      {
         dotCount++;
         Advance(); // Consume the '.'
         while (IsDigit(Peek()))
            Advance();
      }

      // After scanning, decide the token type based on the number of dots found.
      // We also check that the literal isn't followed by more characters that would
      // make it an identifier, e.g. "1444.11.11foo"
      if (dotCount == 2 && !IsIdentifierContinuationChar(Peek()))
         AddToken(TokenType.Date);
      else
         // This handles integers (dotCount = 0) and floats (dotCount = 1).
         // If dotCount > 2, it will be treated as a number, which is likely a syntax error
         // that the user or a later validation step can catch.
         AddToken(TokenType.Number);
   }

   private void ScanString()
   {
      while (Peek() != '"' && !IsAtEnd())
      {
         if (Peek() == '\n')
            _line++;
         Advance();
      }

      if (IsAtEnd())
      {
         AddToken(TokenType.Unexpected); // Unterminated string
         return;
      }

      Advance(); // The closing quote

      // Add a token for the *content* of the string, excluding the quotes.
      var stringContentStart = _start + 1;
      var stringContentLength = _current - _start - 2;
      AddToken(TokenType.String, stringContentStart, stringContentLength);
   }

   #region Helper Methods

   private bool IsAtEnd() => _current >= _source.Length;

   private char Advance()
   {
      _column++;
      return _source[_current++];
   }

   // Overload 1: Adds a token based on the current scanner position.
   private void AddToken(TokenType type)
   {
      AddToken(type, _start, _current - _start);
   }

   // Overload 2: Adds a token with a specific start/length.
   // This is the core method that creates the Token struct.
   private void AddToken(TokenType type, int start, int length)
   {
      var consumedChars = _current - _start;
      _tokens.Add(new(type, start, length, _line, _column - consumedChars));
   }

   private bool Match(char expected)
   {
      if (IsAtEnd() || _source[_current] != expected)
         return false;

      _current++;
      _column++;
      return true;
   }

   private char Peek() => IsAtEnd() ? '\0' : _source[_current];
   private char PeekNext() => _current + 1 >= _source.Length ? '\0' : _source[_current + 1];

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static bool IsDigit(char c) => char.IsDigit(c);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static bool IsAlpha(char c) => char.IsLetter(c) || c == '_';

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static bool IsIdentifierContinuationChar(char c)
      => IsAlphaNumeric(c) || c == ':' || c == '.' || c == '|' || c == '-';

   #endregion
}