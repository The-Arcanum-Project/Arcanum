using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class SnNodesHelpers
{
   extension(StatementNode node)
   {
      public bool IsUnaryStatementNode(ref ParsingContext pc,
                                       [MaybeNullWhen(false)] out UnaryStatementNode value)
      {
         using var scope = pc.PushScope();
         if (node is not UnaryStatementNode usn)
         {
            pc.SetContext(node);
            var key = pc.SliceString(node);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidNodeType,
                                           $"{node.GetType().Name}({key})",
                                           $"{nameof(UnaryStatementNode)}",
                                           key);
            value = null!;
            return pc.Fail();
         }

         value = usn;
         return true;
      }

      public bool ParseFloatFromUnaryOrKeyOnlyNode(ref ParsingContext pc,
                                                   out float value)
      {
         using var scope = pc.PushScope();
         if (node is UnaryStatementNode usn)
         {
            if (usn.TryParseFloatValue(ref pc, out value))
               return true;

            return pc.Fail();
         }

         if (node is KeyOnlyNode kon)
         {
            if (kon.TryParseFloatValue(ref pc, out value))
               return true;

            return pc.Fail();
         }

         value = 0;
         pc.SetContext(node);
         var key = pc.SliceString(node);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidNodeType,
                                        $"{node.GetType().Name}({key})",
                                        $"{nameof(UnaryStatementNode)} or {nameof(KeyOnlyNode)}",
                                        key);
         return pc.Fail();
      }
   }

   extension(StatementNode node)
   {
      /// <summary>
      /// Logs a warning if the StatementNode is not a BlockNode. 
      /// </summary>
      public bool IsBlockNode(ref ParsingContext pc,
                              [MaybeNullWhen(false)] out BlockNode value)
      {
         using var scope = pc.PushScope();
         if (node is not BlockNode bn)
         {
            pc.SetContext(node);
            var key = pc.SliceString(node);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidNodeType,
                                           $"{node.GetType().Name}({key})",
                                           $"{nameof(BlockNode)}",
                                           key);
            value = null!;
            return pc.Fail();
         }

         value = bn;
         return true;
      }
   }

   extension(StatementNode node)
   {
      /// <summary>
      /// Verifies if the StatementNode is a ContentNode. <br/>
      /// If not, logs a warning.
      /// </summary>
      public bool IsContentNode(ref ParsingContext pc,
                                [MaybeNullWhen(false)] out ContentNode value)
      {
         using var scope = pc.PushScope();
         if (node is not ContentNode cn)
         {
            pc.SetContext(node);
            var key = pc.SliceString(node);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidNodeType,
                                           $"{node.GetType().Name}({key})",
                                           nameof(ContentNode),
                                           key);
            value = null!;
            return pc.Fail();
         }

         value = cn;
         return true;
      }

      /// <summary>
      /// Returns true if the StatementNode is a KeyOnlyNode. <br/>
      /// Logs a warning if it is not.
      /// </summary>
      public bool IsKeyOnlyNode(ref ParsingContext pc,
                                [MaybeNullWhen(false)] out KeyOnlyNode value)
      {
         using var scope = pc.PushScope();
         if (node is not KeyOnlyNode kon)
         {
            pc.SetContext(node);
            var key = pc.SliceString(node);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidNodeType,
                                           $"{node.GetType().Name}({key})",
                                           nameof(KeyOnlyNode),
                                           key);
            value = null!;
            return pc.Fail();
         }

         value = kon;
         return true;
      }
   }
}