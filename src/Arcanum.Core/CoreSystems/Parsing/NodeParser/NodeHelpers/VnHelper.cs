using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class VnHelper
{
   /// <summary>
   /// Returns a string containing the literal value, including a leading minus sign if the value is negative.
   /// If the node is not a literal value or an optional unary minus, logs a warning and returns false.
   /// </summary>
   public static bool IsLiteralValueNodeOptionalUnary(this ValueNode node,
                                                      ref ParsingContext pc,
                                                      [MaybeNullWhen(false)] out string value,
                                                      [MaybeNullWhen(false)] out LiteralValueNode lvn)
   {
      if (node is UnaryNode { Value: LiteralValueNode lvn1, Operator.Type: TokenType.Minus } unary)
      {
         var unLoc = unary.GetLocation();
         value = lvn1.Value.GetLexeme(pc.Source, unLoc.Item2);
         lvn = lvn1;
         return true;
      }

      if (node.IsLiteralValueNode(ref pc, out lvn))
      {
         value = pc.SliceString(lvn);
         return true;
      }

      value = null;
      lvn = null;
      return pc.Fail();
   }

   public static bool IsLiteralValueNode(this ValueNode node,
                                         ref ParsingContext pc,
                                         [MaybeNullWhen(false)] out LiteralValueNode value)
   {
      using var scope = pc.PushScope();
      if (node is not LiteralValueNode lvn)
      {
         pc.SetContext(node);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidNodeType,
                                        node.GetType().Name,
                                        nameof(LiteralValueNode),
                                        "N/A");
         value = null!;
         return pc.Fail();
      }

      value = lvn;
      return true;
   }
}