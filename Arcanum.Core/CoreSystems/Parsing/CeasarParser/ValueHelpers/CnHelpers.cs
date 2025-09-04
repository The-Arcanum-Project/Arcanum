using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;

public static class CnHelpers
{
   /// <summary>
   /// Checks if the ContentNode has an Equals separator and a LiteralValueNode as value.
   /// If so, it extracts the string content from the LiteralValueNode.
   /// Logs warnings if the checks fail.
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="value"></param>
   /// <returns></returns>
   public static bool TryGetStringContentNode(this ContentNode node,
                                              LocationContext ctx,
                                              string actionName,
                                              string source,
                                              out string value)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator, TokenType.Equals, ctx, actionName))
      {
         value = string.Empty;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, out var lvn))
      {
         value = string.Empty;
         return false;
      }

      value = lvn!.Value.GetLexeme(source);
      return true;
   }

   public static bool TryGetIdentifierNode(this ContentNode node,
                                           LocationContext ctx,
                                           string actionName,
                                           string source,
                                           out string identifier)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator, TokenType.Equals, ctx, actionName))
      {
         identifier = string.Empty;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, out var lvn))
      {
         identifier = string.Empty;
         return false;
      }

      identifier = lvn!.Value.GetLexeme(source);
      return true;
   }

   public static bool TryGetIntegerContentNode(this ContentNode node,
                                               LocationContext ctx,
                                               string actionName,
                                               string source,
                                               out int value)
   {
      if (!SeparatorHelper.IsAnySupportedSeparator(node.Separator,
                                                   ctx,
                                                   actionName,
                                                   TokenType.Equals,
                                                   TokenType.GreaterOrEqual,
                                                   TokenType.LessOrEqual))
      {
         value = 0;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, out var lvn))
      {
         value = 0;
         return false;
      }

      var lexeme = lvn!.Value.GetLexeme(source);
      if (!int.TryParse(lexeme, out value))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidIntegerValue,
                                        actionName,
                                        lexeme);
         value = 0;
         return false;
      }

      return true;
   }
}