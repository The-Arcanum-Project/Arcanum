using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace UnitTests.CoreSystems.NodeParser;

[TestFixture]
public class LexerTests
{
   // The main entry point for our tests. It runs the lexer.
   private Span<Token> Lex(string source, out Token[] tokenArray)
   {
      // We create an array that is guaranteed to be large enough.
      // For testing, a new array is simpler than pooling.
      tokenArray = new Token[source.Length + 1];
      var lexer = new Lexer(source.AsSpan(), tokenArray.AsSpan());
      return lexer.ScanTokens();
   }

   // Our powerful assertion helper.
   // It verifies the token type and the text it represents.
   private void AssertToken(Token token, TokenType expectedType, string expectedText, string source)
   {
      // NUnit has a powerful constraint-based model with Assert.That,
      // but Assert.AreEqual is often simpler for direct comparisons.
      Assert.That(token.Type, Is.EqualTo(expectedType), "Token type mismatch.");

      var tokenText = source.AsSpan(token.Start, token.Length).ToString();
      Assert.That(tokenText, Is.EqualTo(expectedText), "Token text mismatch.");
   }

   [TestCase("{", TokenType.LeftBrace)]
   [TestCase("}", TokenType.RightBrace)]
   [TestCase("]", TokenType.RightBracket)]
   [TestCase("+", TokenType.Plus)]
   [TestCase("-", TokenType.Minus)]
   [TestCase("*", TokenType.Multiply)]
   [TestCase("/", TokenType.Divide)]
   [TestCase("=", TokenType.Equals)]
   [TestCase("<", TokenType.Less)]
   [TestCase(">", TokenType.Greater)]
   [TestCase("<=", TokenType.LessOrEqual)]
   [TestCase(">=", TokenType.GreaterOrEqual)]
   [TestCase("!=", TokenType.NotEquals)]
   [TestCase("?=", TokenType.QuestionEquals)]
   [TestCase("@[", TokenType.LeftBracket)] // Special case for @
   public void Scans_Single_And_Compound_Tokens(string source, TokenType expectedType)
   {
      var tokens = Lex(source, out _);

      Assert.That(tokens.Length, Is.EqualTo(2), "Expected one token plus EOF.");
      Assert.That(tokens[0].Type, Is.EqualTo(expectedType));
      Assert.That(tokens[1].Type, Is.EqualTo(TokenType.EndOfFile));
   }

   [Test]
   public void Scans_String_Literals()
   {
      var source = "\"hello world\"";
      var tokens = Lex(source, out _);

      Assert.That(tokens.Length, Is.EqualTo(2));
      // Note: We assert the *content* of the string, without quotes
      AssertToken(tokens[0], TokenType.String, "hello world", source);
   }

   [Test]
   public void Scans_String_Literal_With_Escaped_Quotes()
   {
      var source = "\"hello \\\"world\\\"\"";
      var tokens = Lex(source, out _);

      Assert.That(tokens.Length, Is.EqualTo(2));
      AssertToken(tokens[0], TokenType.String, "hello \\\"world\\\"", source);
   }

   [TestCase("123", TokenType.Number, "123")]
   [TestCase("987.65", TokenType.Number, "987.65")]
   [TestCase("2023.10.27", TokenType.Date, "2023.10.27")]
   [TestCase("1.2.3.4", TokenType.Number, "1.2.3.4")] // More than 2 dots is a number
   public void Scans_Numeric_And_Date_Literals(string source, TokenType expectedType, string expectedText)
   {
      var tokens = Lex(source, out _);
      Assert.That(tokens.Length, Is.EqualTo(2));
      AssertToken(tokens[0], expectedType, expectedText, source);
   }

   [TestCase("variable", TokenType.Identifier)]
   [TestCase("_private", TokenType.Identifier)]
   [TestCase("with_nums123", TokenType.Identifier)]
   [TestCase("kebab-case:dot.path|pipe", TokenType.Identifier)]
   [TestCase("yes", TokenType.Yes)]
   [TestCase("no", TokenType.No)]
   [TestCase("Yes", TokenType.Identifier)] // Keywords are case-sensitive
   [TestCase("@myvar", TokenType.AtIdentifier)]
   public void Scans_Identifiers_And_Keywords(string source, TokenType expectedType)
   {
      var tokens = Lex(source, out _);

      Assert.That(tokens.Length, Is.EqualTo(2));
      var token = tokens[0];
      Assert.That(token.Type, Is.EqualTo(expectedType));

      // For @identifier, the token text doesn't include the '@'
      var expectedText = source.StartsWith("@") ? source.Substring(1) : source;
      AssertToken(token, expectedType, expectedText, source);
   }

   [Test]
   public void Ignores_Whitespace_And_Comments()
   {
      var source = @"
        # this is a comment
        { # another comment
        }
        ";
      var tokens = Lex(source, out _);

      // We expect { and } and EOF, and nothing else.
      Assert.That(tokens.Length, Is.EqualTo(3));
      Assert.That(tokens[0].Type, Is.EqualTo(TokenType.LeftBrace));
      Assert.That(tokens[1].Type, Is.EqualTo(TokenType.RightBrace));
      Assert.That(tokens[2].Type, Is.EqualTo(TokenType.EndOfFile));
   }

   [Test]
   public void Correctly_Tracks_Lines_And_Columns()
   {
      var source = "{\n  id = 1\n}";
      var tokens = Lex(source, out _);

      // Expected: { (1,1), id (2,3), = (2,6), 1 (2,8), } (3,1), EOF
      Assert.That(tokens.Length, Is.EqualTo(6));

      // { at Line 1, Col 1
      Assert.That(tokens[0].Line, Is.EqualTo(1));
      Assert.That(tokens[0].Column, Is.EqualTo(1));

      // id at Line 2, Col 3
      Assert.That(tokens[1].Line, Is.EqualTo(2));
      Assert.That(tokens[1].Column, Is.EqualTo(2));

      // = at Line 2, Col 6
      Assert.That(tokens[2].Line, Is.EqualTo(2));
      Assert.That(tokens[2].Column, Is.EqualTo(5));

      // 1 at Line 2, Col 8
      Assert.That(tokens[3].Line, Is.EqualTo(2));
      Assert.That(tokens[3].Column, Is.EqualTo(7));

      // } at Line 3, Col 1
      Assert.That(tokens[4].Line, Is.EqualTo(3));
      Assert.That(tokens[4].Column, Is.EqualTo(0));
   }

   [TestCase("$", TokenType.Unexpected)]
   [TestCase("!", TokenType.Unexpected)] // Incomplete compound token
   [TestCase("?", TokenType.Unexpected)]
   public void Scans_Unexpected_Characters(string source, TokenType expectedType)
   {
      var tokens = Lex(source, out _);
      Assert.That(tokens.Length, Is.EqualTo(2));
      Assert.That(tokens[0].Type, Is.EqualTo(expectedType));
   }

   [Test]
   public void Handles_Unterminated_String()
   {
      var source = "\"hello";
      var tokens = Lex(source, out _);

      // Expect an 'Unexpected' token at the start of the string
      Assert.That(tokens.Length, Is.EqualTo(2));
      Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Unexpected));
      Assert.That(tokens[0].Column, Is.EqualTo(1));
   }

   [Test]
   public void Handles_Empty_Input()
   {
      var source = "";
      var tokens = Lex(source, out _);

      Assert.That(tokens.Length, Is.EqualTo(1));
      Assert.That(tokens[0].Type, Is.EqualTo(TokenType.EndOfFile));
   }
}