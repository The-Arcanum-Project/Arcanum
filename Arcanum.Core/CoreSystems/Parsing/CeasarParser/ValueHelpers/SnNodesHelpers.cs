using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;

public static class SnNodesHelpers
{
   /// <summary>
   /// Verifies if the StatementNode is a BlockNode. <br/>
   /// If not, logs a warning. 
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="source"></param>
   /// <param name="actionName"></param>
   /// <param name="value"></param>
   /// <returns></returns>
   public static bool IsBlockNode(this StatementNode node,
                                  LocationContext ctx,
                                  string source,
                                  string actionName,
                                  out BlockNode? value)
   {
      if (node is not BlockNode bn)
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        actionName,
                                        node.GetType().Name,
                                        nameof(BlockNode),
                                        node.KeyNode.GetLexeme(source));
         value = null!;
         return false;
      }

      value = bn;
      return true;
   }

   /// <summary>
   /// Logs a warning if the StatementNode is not a BlockNode. <br/>
   /// Updates the <paramref name="validationResult"/> with the result of the check.
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="source"></param>
   /// <param name="className"></param>
   /// <param name="validationResult"></param>
   /// <param name="value"></param>
   /// <returns></returns>
   public static bool IsBlockNode(this StatementNode node,
                                  LocationContext ctx,
                                  string source,
                                  string className,
                                  ref bool validationResult,
                                  [MaybeNullWhen(false)] out BlockNode value)
   {
      var returnVal = node.IsBlockNode(ctx, source, className + ".StatementNode.IsBlockNode", out value);
      validationResult &= returnVal;
      return returnVal;
   }

   /// <summary>
   /// Verifies if the StatementNode is a ContentNode. <br/>
   /// If not, logs a warning.
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="source"></param>
   /// <param name="className"></param>
   /// <param name="validationResult"></param>
   /// <param name="value"></param>
   /// <returns></returns>
   public static bool IsContentNode(this StatementNode node,
                                    LocationContext ctx,
                                    string source,
                                    string className,
                                    ref bool validationResult,
                                    [MaybeNullWhen(false)] out ContentNode value)
   {
      if (node is not ContentNode cn)
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        $"{className}.StatementNode.IsContentNode",
                                        node.GetType().Name,
                                        nameof(ContentNode),
                                        node.KeyNode.GetLexeme(source));
         value = null!;
         validationResult = false;
         return false;
      }

      value = cn;
      return true;
   }
}