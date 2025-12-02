using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class RnHelpers
{
   /// <summary>
   /// Creates a diagnostic if the RootNode is empty.
   /// </summary>
   public static bool IsNodeEmptyDiagnostic(this RootNode rn, ref ParsingContext pc)
   {
      using var scope = pc.PushScope();
      if (rn.Statements.Count == 0)
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.EmptyRootNode,
                                        pc.Context.FilePath);

      return rn.Statements.Count == 0;
   }

   /// <summary>
   /// Creates a diagnostic if the RootNode does not have exactly <paramref name="count"/> statements.
   /// </summary>
   public static bool HasXStatements(this RootNode rn, ref ParsingContext pc, int count)
   {
      using var scope = pc.PushScope();
      if (rn.Statements.Count == count)
         return true;

      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.InvalidStatementCount,
                                     count,
                                     rn.Statements.Count);
      return false;
   }

   public static bool GetBlockNodes(this RootNode rn,
                                    ref ParsingContext pc,
                                    string[] keys,
                                    out BlockNode[] nodes,
                                    bool onlyBlocks = true)
   {
      using var scope = pc.PushScope();
      nodes = new BlockNode[keys.Length];

      foreach (var sn in rn.Statements)
      {
         BlockNode bn;
         if (onlyBlocks)
         {
            if (!sn.IsBlockNode(ref pc, out bn!))
               continue;
         }
         else
         {
            if (sn is not BlockNode node)
               continue;

            bn = node;
         }

         for (var i = 0; i < keys.Length; i++)
            if (pc.SliceString(bn) == keys[i])
               nodes[i] = bn;
      }

      return nodes.Any(n => n != null!);
   }
}