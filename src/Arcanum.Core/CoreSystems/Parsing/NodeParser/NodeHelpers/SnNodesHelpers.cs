using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

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
   [Obsolete("Use the overload that takes a className and a ref bool validationResult instead.")]
   public static bool IsBlockNode(this StatementNode node,
                                  LocationContext ctx,
                                  string source,
                                  string actionName,
                                  [MaybeNullWhen(false)] out BlockNode value)
   {
      if (node is not BlockNode bn)
      {
         ctx.SetPosition(node.KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        actionName,
                                        $"{node.GetType().Name}({node.KeyNode.GetLexeme(source)})",
                                        $"{nameof(BlockNode)}",
                                        node.KeyNode.GetLexeme(source));
         value = null!;
         return false;
      }

      value = bn;
      return true;
   }

   public static bool IsUnaryStatementNode(this StatementNode node,
                                           LocationContext ctx,
                                           string source,
                                           string actionName,
                                           ref bool validation,
                                           [MaybeNullWhen(false)] out UnaryStatementNode value)
   {
      if (node is not UnaryStatementNode usn)
      {
         ctx.SetPosition(node.KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        actionName,
                                        $"{node.GetType().Name}({node.KeyNode.GetLexeme(source)})",
                                        $"{nameof(UnaryStatementNode)}",
                                        node.KeyNode.GetLexeme(source));
         value = null!;
         validation = false;
         return false;
      }

      value = usn;
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
#pragma warning disable CS0618 // Type or member is obsolete
      var returnVal = node.IsBlockNode(ctx, source, className + ".StatementNode.IsBlockNode", out value);
#pragma warning restore CS0618 // Type or member is obsolete
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
   /// <param name="actionStack"></param>
   /// <param name="validationResult"></param>
   /// <param name="value"></param>
   /// <returns></returns>
   public static bool IsContentNode(this StatementNode node,
                                    LocationContext ctx,
                                    string source,
                                    string actionStack,
                                    ref bool validationResult,
                                    [MaybeNullWhen(false)] out ContentNode value)
   {
      if (node is not ContentNode cn)
      {
         ctx.SetPosition(node.KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        $"{actionStack}.StatementNode.IsContentNode",
                                        $"{node.GetType().Name}({node.KeyNode.GetLexeme(source)})",
                                        nameof(ContentNode),
                                        node.KeyNode.GetLexeme(source));
         value = null!;
         validationResult = false;
         return false;
      }

      value = cn;
      return true;
   }

   /// <summary>
   /// Returns true if the StatementNode is a KeyOnlyNode. <br/>
   /// Logs a warning if it is not.
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="source"></param>
   /// <param name="className"></param>
   /// <param name="validationResult"></param>
   /// <param name="value"></param>
   /// <returns></returns>
   public static bool IsKeyOnlyNode(this StatementNode node,
                                    LocationContext ctx,
                                    string source,
                                    string className,
                                    ref bool validationResult,
                                    [MaybeNullWhen(false)] out KeyOnlyNode value)
   {
      if (node is not KeyOnlyNode kon)
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        $"{className}.StatementNode.IsKeyOnlyNode",
                                        $"{node.GetType().Name}({node.KeyNode.GetLexeme(source)})",
                                        nameof(KeyOnlyNode),
                                        node.KeyNode.GetLexeme(source));
         value = null!;
         validationResult = false;
         return false;
      }

      value = kon;
      return true;
   }
}