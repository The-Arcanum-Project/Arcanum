using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;

public static class SeparatorHelper
{
   /// <summary>
   /// Checks if the token is of the expected separator type. <br/>
   /// Logs a warning if it is not.
   /// </summary>
   /// <param name="token"></param>
   /// <param name="type"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <returns></returns>
   public static bool IsSeparatorOfType(Token token,
                                        TokenType type,
                                        LocationContext ctx,
                                        string actionName)
   {
      if (token.Type == type)
         return true;

      DiagnosticException.LogWarning(ctx,
                                     ParsingError.Instance.InvalidSeparator,
                                     actionName,
                                     token.Type.ToString(),
                                     type.ToString());
      return false;
   }

   public static bool IsSeparatorOfType(Token token,
                                        TokenType type,
                                        LocationContext ctx,
                                        string actionName,
                                        ref bool validationResult)
   {
      var returnVal = IsSeparatorOfType(token, type, ctx, actionName);
      validationResult &= returnVal;
      return returnVal;
   }

   /// <summary>
   /// Returns true if the token is of any of the supported types. <br/>
   /// Logs a warning if it is not.
   /// </summary>
   /// <param name="token"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="supportedTypes"></param>
   /// <returns></returns>
   public static bool IsAnySupportedSeparator(Token token,
                                              LocationContext ctx,
                                              string actionName,
                                              params TokenType[] supportedTypes)
   {
      if (supportedTypes.Contains(token.Type))
         return true;

      DiagnosticException.LogWarning(ctx,
                                     ParsingError.Instance.InvalidSeparator,
                                     actionName,
                                     token.Type.ToString(),
                                     string.Join(" or ", supportedTypes.Select(t => t.ToString())));

      return false;
   }
}