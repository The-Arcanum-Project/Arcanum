using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;

public static class SeparatorHelper
{
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
}