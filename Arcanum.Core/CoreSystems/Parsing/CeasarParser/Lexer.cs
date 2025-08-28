namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser;

/// <summary>
/// Scans a source code string and produces a sequence of tokens. This implementation
/// is optimized to avoid string allocations during the scanning process.
/// </summary>
public class Lexer
{
    private readonly string _source;
    private readonly List<Token> _tokens = new();

    private int _start = 0;
    private int _current = 0;
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

        _tokens.Add(new Token(TokenType.EndOfFile, _source.Length, 0, _line, _column));
        return new LexerResult(_source, _tokens);
    }

    private void ScanToken()
    {
        var c = Advance();
        switch (c)
        {
            // The logic in this switch remains largely the same, but the AddToken calls are now much cheaper.
            case '{': AddToken(TokenType.LeftBrace); break;
            case '}': AddToken(TokenType.RightBrace); break;
            case ']': AddToken(TokenType.RightBracket); break;
            case '+': AddToken(TokenType.Plus); break;
            case '-': AddToken(TokenType.Minus); break;
            case '*': AddToken(TokenType.Multiply); break;
            case '=': AddToken(TokenType.Equals); break;
            case '<': AddToken(Match('=') ? TokenType.LessOrEqual : TokenType.Unexpected); break;
            case '>': AddToken(Match('=') ? TokenType.GreaterOrEqual : TokenType.Unexpected); break;
            case '?': AddToken(Match('=') ? TokenType.QuestionEquals : TokenType.Unexpected); break;
            case '@':
                if (Match('[')) AddToken(TokenType.AtLeftBracket);
                else ScanIdentifier(isAtIdentifier: true);
                break;
            case '#':
                while (Peek() != '\n' && !IsAtEnd()) Advance();
                break;
            case '/': AddToken(TokenType.Divide); break;
            case ' ': case '\r': case '\t': break; // Ignore whitespace
            case '\n':
                _line++;
                _column = 0;
                break;
            case '"': ScanString(); break;
            default:
                if (IsDigit(c)) ScanNumber();
                else if (IsAlpha(c)) ScanIdentifier();
                else AddToken(TokenType.Unexpected);
                break;
        }
    }

    private void ScanIdentifier(bool isAtIdentifier = false)
    {
        while (IsIdentifierContinuationChar(Peek())) Advance();

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
    
    // This helper replaces the Keywords dictionary for better performance.
    private static TokenType LookupIdentifierType(ReadOnlySpan<char> textSpan)
    {
        return textSpan switch
        {
            "yes" => TokenType.Yes,
            "no" => TokenType.No,
            _ => TokenType.Identifier
        };
    }

    private void ScanNumber()
    {
        while (IsDigit(Peek())) Advance();
        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            Advance();
            while (IsDigit(Peek())) Advance();
        }
        AddToken(TokenType.Number);
    }

    private void ScanString()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n') _line++;
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
        _tokens.Add(new Token(type, start, length, _line, _column - consumedChars));
    }

    private bool Match(char expected)
    {
        if (IsAtEnd() || _source[_current] != expected) return false;
        _current++;
        _column++;
        return true;
    }

    private char Peek() => IsAtEnd() ? '\0' : _source[_current];
    private char PeekNext() => _current + 1 >= _source.Length ? '\0' : _source[_current + 1];

    private static bool IsDigit(char c) => char.IsDigit(c);
    private static bool IsAlpha(char c) => char.IsLetter(c) || c == '_';
    private static bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);
    private static bool IsIdentifierContinuationChar(char c) => IsAlphaNumeric(c) || c == ':' || c == '.';

    #endregion
}