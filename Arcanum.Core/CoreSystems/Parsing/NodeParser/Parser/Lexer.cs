using System.Runtime.CompilerServices;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

/// <summary>
/// Scans a source code string and produces a sequence of tokens. This implementation
/// is highly optimized for performance, minimizing allocations, method calls in hot loops,
/// and branching.
/// </summary>
public ref struct Lexer
{
   private readonly ReadOnlySpan<char> _source;
   private Span<Token> _tokens;

   private int _tokenCount;

   private int _start;
   private int _current;
   private int _line = 1;
   private int _column = 1;

   // A lookup table to classify characters. This is much faster
   // than a switch statement for single-character tokens. It avoids branching.
   private static readonly TokenType[] CharToTokenMap = new TokenType[128];

   static Lexer()
   {
      // Initialize all to unexpected by default
      Array.Fill(CharToTokenMap, TokenType.Unexpected);

      // Simple one-char tokens
      CharToTokenMap['{'] = TokenType.LeftBrace;
      CharToTokenMap['}'] = TokenType.RightBrace;
      CharToTokenMap[']'] = TokenType.RightBracket;
      CharToTokenMap['+'] = TokenType.Plus;
      CharToTokenMap['-'] = TokenType.Minus;
      CharToTokenMap['*'] = TokenType.Multiply;
      CharToTokenMap['='] = TokenType.Equals;
      CharToTokenMap['<'] = TokenType.Less;
      CharToTokenMap['>'] = TokenType.Greater;
      CharToTokenMap['/'] = TokenType.Divide;
      CharToTokenMap['!'] = TokenType.NotEquals; // Special case with '='
      CharToTokenMap['?'] = TokenType.QuestionEquals; // Special case with '='

      // Whitespace
      CharToTokenMap[' '] = TokenType.Whitespace;
      CharToTokenMap['\r'] = TokenType.Whitespace;
      CharToTokenMap['\t'] = TokenType.Whitespace;
      CharToTokenMap['\n'] = TokenType.NewLine;

      // Others that require special handling
      CharToTokenMap['"'] = TokenType.String;
      CharToTokenMap['#'] = TokenType.Comment;
      CharToTokenMap['@'] = TokenType.AtIdentifier;

      // Identifiers & Numbers
      for (var c = 'a'; c <= 'z'; c++)
         CharToTokenMap[c] = TokenType.Identifier;
      for (var c = 'A'; c <= 'Z'; c++)
         CharToTokenMap[c] = TokenType.Identifier;
      CharToTokenMap['_'] = TokenType.Identifier;

      for (var c = '0'; c <= '9'; c++)
         CharToTokenMap[c] = TokenType.Number;
   }

   public Lexer(ReadOnlySpan<char> source, Span<Token> tokenBuffer)
   {
      _source = source;
      _tokens = tokenBuffer;
   }

   public Span<Token> ScanTokens()
   {
      while (_current < _source.Length)
      {
         _start = _current;
         ScanToken();
      }

      if (_tokenCount < _tokens.Length)
         _tokens[_tokenCount++] = new(TokenType.EndOfFile, _source.Length, 0, _line, _column);

      return _tokens[.._tokenCount];
   }

   private void ScanToken()
   {
      var c = Advance();

      // For ASCII, the lookup table is faster than a switch.
      // We check if the char is in our fast path map.
      if (c < 128)
      {
         var tokenType = CharToTokenMap[c];
         switch (tokenType)
         {
            case TokenType.Whitespace:
               break; // Ignore
            case TokenType.NewLine:
               _line++;
               _column = 0;
               break;

            // Simple, single-character tokens
            case TokenType.LeftBrace:
            case TokenType.RightBrace:
            case TokenType.RightBracket:
            case TokenType.Plus:
            case TokenType.Minus:
            case TokenType.Multiply:
            case TokenType.Divide:
            case TokenType.Equals:
               AddToken(tokenType);
               break;

            // Two-character tokens
            case TokenType.Less:
               AddToken(Match('=') ? TokenType.LessOrEqual : TokenType.Less);
               break;
            case TokenType.Greater:
               AddToken(Match('=') ? TokenType.GreaterOrEqual : TokenType.Greater);
               break;
            case TokenType.NotEquals: // Mapped from '!'
               AddToken(Match('=') ? TokenType.NotEquals : TokenType.Unexpected);
               break;
            case TokenType.QuestionEquals: // Mapped from '?'
               AddToken(Match('=') ? TokenType.QuestionEquals : TokenType.Unexpected);
               break;

            // Tokens requiring dedicated scan methods
            case TokenType.Comment: // Mapped from '#'
               ScanComment();
               break;
            case TokenType.String: // Mapped from '"'
               ScanString();
               break;
            case TokenType.Number: // Mapped from '0'-'9'
               ScanNumberOrDate();
               break;
            case TokenType.Identifier: // Mapped from 'a'-'z', 'A'-'Z', '_'
               ScanIdentifier();
               break;
            case TokenType.AtIdentifier: // Mapped from '@'
               if (Match('['))
                  AddToken(TokenType.LeftBracket);
               else
                  ScanIdentifier(isAtIdentifier: true);
               break;

            case TokenType.Unexpected:
            default:
               AddToken(TokenType.Unexpected);
               break;
         }
      }
      // Fallback for non-ASCII characters, which could be identifiers.
      else if (IsAlpha(c))
      {
         ScanIdentifier();
      }
      else
      {
         AddToken(TokenType.Unexpected);
      }
   }

   private void ScanComment()
   {
      while (_current < _source.Length && _source[_current] != '\n')
      {
         _current++;
         _column++;
      }
   }

   private void ScanIdentifier(bool isAtIdentifier = false)
   {
      while (_current < _source.Length && IsIdentifierContinuationChar(_source[_current]))
      {
         _current++;
         _column++;
      }

      if (isAtIdentifier)
      {
         AddToken(TokenType.AtIdentifier, _start + 1, _current - _start - 1);
      }
      else
      {
         var textSpan = _source[_start.._current];
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
      while (_current < _source.Length && IsAsciiDigit(_source[_current]))
      {
         _current++;
         _column++;
      }

      if (_current < _source.Length && IsAlpha(_source[_current]))
      {
         while (_current < _source.Length && IsIdentifierContinuationChar(_source[_current]))
         {
            _current++;
            _column++;
         }

         AddToken(TokenType.Identifier);
         return;
      }

      var dotCount = 0;
      while (_current + 1 < _source.Length && _source[_current] == '.' && IsAsciiDigit(_source[_current + 1]))
      {
         dotCount++;
         _current++; // Consume '.'
         _column++;
         do
         {
            _current++;
            _column++;
         }
         while (_current < _source.Length && IsAsciiDigit(_source[_current]));
      }

      var type = (dotCount == 2 && !IsIdentifierContinuationChar(Peek()))
                    ? TokenType.Date
                    : TokenType.Number;

      AddToken(type);
   }

   private void ScanString()
   {
      while (_current < _source.Length && _source[_current] != '"')
      {
         if (_source[_current] == '\n')
         {
            _line++;
            _column = 0;
         }

         _current++;
         _column++;
      }

      if (_current >= _source.Length)
      {
         AddToken(TokenType.Unexpected); // Unterminated string
         return;
      }

      _current++; // Consume the closing quote.
      _column++;

      var stringContentStart = _start + 1;
      var stringContentLength = _current - _start - 2;
      AddToken(TokenType.String, stringContentStart, stringContentLength);
   }

   #region Helper Methods

   private char Advance()
   {
      _column++;
      return _source[_current++];
   }

   private void AddToken(TokenType type)
   {
      var consumedChars = _current - _start;
      AddToken(type, _start, consumedChars);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private void AddToken(TokenType type, int start, int length)
   {
      if (_tokenCount >= _tokens.Length)
         // Handle buffer full error - maybe throw or have a return code
         // For simplicity, we'll just stop adding tokens.
         return;

      _tokens[_tokenCount++] = new(type, start, length, _line, _column - length);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private bool Match(char expected)
   {
      if (_current >= _source.Length || _source[_current] != expected)
         return false;

      _current++;
      _column++;
      return true;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private char Peek() => _current >= _source.Length ? '\0' : _source[_current];

   // Use faster, non-Unicode (ASCII) checks where appropriate.
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static bool IsAsciiDigit(char c) => c is >= '0' and <= '9';

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static bool IsAlpha(char c) => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static bool IsAlphaNumeric(char c) => IsAlpha(c) || IsAsciiDigit(c);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static bool IsIdentifierContinuationChar(char c)
   {
      // This is a great candidate for another small lookup table if you want to push it further,
      // but a series of ORs is usually compiled efficiently.
      return IsAlphaNumeric(c) || c == ':' || c == '.' || c == '|' || c == '-';
   }

   #endregion
}