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
   /// <param name="actionName"></param>
   /// <param name="value"></param>
   /// <returns></returns>
   public static bool IsBlockNode(this StatementNode node, LocationContext ctx, string actionName, out BlockNode? value)
   {
      if (node is not BlockNode bn)
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        actionName,
                                        node.GetType().Name,
                                        nameof(BlockNode));
         value = null!;
         return false;
      }
      value = bn;
      return true;
   }
   
   /// <summary>
   /// Verifies if the StatementNode is a ContentNode. <br/>
   /// If not, logs a warning.
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="value"></param>
   /// <returns></returns>
   public static bool IsContentNode(this StatementNode node, LocationContext ctx, string actionName, out ContentNode? value)
   {
      if (node is not ContentNode cn)
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        actionName,
                                        node.GetType().Name,
                                        nameof(ContentNode));
         value = null!;
         return false;
      }
      value = cn;
      return true;
   }
}