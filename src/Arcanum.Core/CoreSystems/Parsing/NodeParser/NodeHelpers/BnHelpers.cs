using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Nexus;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.GameObjects.BaseTypes;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class BnHelpers
{
   public static bool HasOnlyXBlocksAsChildren(this BlockNode bn,
                                               int count,
                                               ref ParsingContext pc,
                                               [MaybeNullWhen(false)] out List<BlockNode> blocks)
   {
      using var scope = pc.PushScope();
      if (bn.Children.Count != count)
      {
         pc.SetContext(bn);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidBlockCount,
                                        pc.SliceString(bn),
                                        count,
                                        bn.Children.Count);
         blocks = null;
         return pc.Fail();
      }

      blocks = new(count);
      foreach (var child in bn.Children)
      {
         if (child is not BlockNode block)
         {
            pc.SetContext(bn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidNodeType,
                                           child.GetType().Name,
                                           nameof(BlockNode),
                                           pc.SliceString(bn));
            blocks = null;
            return pc.Fail();
         }

         blocks.Add(block);
      }

      return true;
   }

   /// <summary>
   /// !!!!!!! WARNING !!!!!!! <br/>
   /// This is a placeholder for which validates the given content. <br/>
   /// Only use if the objects needed to validate are not yet parsed! 
   /// </summary>
   public static void SetIdentifierList(this BlockNode bn,
                                        ref ParsingContext pc,
                                        INexus target,
                                        Enum nxProp)
   {
      using var scope = pc.PushScope();
      foreach (var sn in bn.Children)
      {
         if (!sn.IsKeyOnlyNode(ref pc, out var kn))
            continue;

         var identifier = pc.SliceString(kn);
         Nx.AddToCollection(target, nxProp, identifier);
      }
   }

   public static Eu5ObjectLocation GetFileLocation(this BlockNode bn)
   {
      return new(bn.KeyNode.Column, bn.KeyNode.Line, bn.GetEndLocation().charPos, bn.KeyNode.Start);
   }
}