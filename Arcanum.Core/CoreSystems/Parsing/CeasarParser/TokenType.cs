namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser;

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
   LessOrEqual, // <=
   GreaterOrEqual, // >=
   QuestionEquals, // ?=
   AtLeftBracket, // @[

   // Literals
   Identifier, // my_variable, width, rgb
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
}