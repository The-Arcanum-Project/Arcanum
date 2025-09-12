namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

public static class TokenExtensions
{
   /// <summary>
   /// Gets the string value of a token by slicing the provided source text.
   /// This is an extension method for the Token struct.
   /// </summary>
   /// <param name="token">The token instance.</param>
   /// <param name="source">The original source string the token was parsed from.</param>
   /// <returns>The string value of the token's lexeme.</returns>
   public static string GetValue(this Token token, string source)
   {
      // We can simply call the existing GetLexeme method, or reimplement the logic.
      // Calling the existing method is cleaner.
      return token.GetLexeme(source);

      /* Or, the direct implementation:
      if (token.Length == 0 || token.Start + token.Length > source.Length)
      {
          return string.Empty;
      }
      return source.Substring(token.Start, token.Length);
      */
   }

   /// <summary>
   /// A helper that gets the value directly from a LexerResult.
   /// </summary>
   public static string GetValue(this Token token, LexerResult lexerResult)
   {
      return token.GetLexeme(lexerResult.Source);
   }
}