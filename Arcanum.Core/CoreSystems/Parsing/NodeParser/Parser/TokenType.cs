namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

/// <summary>
/// Defines the different types of tokens that the Lexer can recognize.
/// </summary>
public enum TokenType
{
   // Single-character tokens
   LeftBrace, // {
   RightBrace, // }
   RightBracket, // ]

   // Math Operators
   Plus, // +
   Minus, // -
   Multiply, // *
   Divide, // /

   // One or two character tokens
   Less, // < 
   Greater, // > 
   Equals, // =
   NotEquals, // !=
   LessOrEqual, // <=
   GreaterOrEqual, // >=
   QuestionEquals, // ?=
   LeftBracket, // @[

   // Literals
   Identifier, // my_variable, width, rgb
   ScopeSeparator, // :
   AtIdentifier, // @my_variable
   String, // "Hello, World!"
   Number, // 123, 45.67
   Date, // 2023.10.05

   // Keywords
   Yes, // yes
   No, // no

   // Control
   Comment, // # this is a comment (usually skipped)
   EndOfFile, // EOF
   Unexpected, // For error handling
   Whitespace,
   NewLine,
}