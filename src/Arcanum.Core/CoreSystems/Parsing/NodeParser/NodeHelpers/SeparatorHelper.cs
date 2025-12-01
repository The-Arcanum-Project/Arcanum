using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class SeparatorHelper
{
   /// <summary>
   /// Checks if the token is of the expected separator type. <br/>
   /// Logs a warning if it is not.
   /// </summary>
   public static bool IsSeparatorOfType(Token token,
                                        TokenType type,
                                        ref ParsingContext pc)
   {
      if (token.Type == type)
         return true;

      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.InvalidSeparator,
                                     token.Type.ToString(),
                                     type.ToString());
      return pc.Fail();
   }

   /// <summary>
   /// Returns true if the token is of any of the supported types. <br/>
   /// Logs a warning if it is not.
   /// </summary>
   public static bool IsAnySupportedSeparator(Token token,
                                              ref ParsingContext pc,
                                              params TokenType[] supportedTypes)
   {
      if (supportedTypes.Contains(token.Type))
         return true;

      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.InvalidSeparator,
                                     token.Type.ToString(),
                                     string.Join(" or ", supportedTypes.Select(t => t.ToString())));

      return pc.Fail();
   }
}