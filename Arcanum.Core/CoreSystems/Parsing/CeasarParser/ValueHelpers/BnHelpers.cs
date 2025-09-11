using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;

public static class BnHelpers
{
   public static bool HasOnlyXBlocksAsChildren(this BlockNode bn,
                                               LocationContext ctx,
                                               string source,
                                               int count,
                                               string callStack,
                                               ref bool validationResult,
                                               [MaybeNullWhen(false)] out List<BlockNode> blocks)
   {
      if (bn.Children.Count != count)
      {
         ctx.SetPosition(bn.KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidBlockCount,
                                        "Parsing BlockNode",
                                        bn.KeyNode.GetLexeme(source),
                                        count,
                                        bn.Children.Count);
         validationResult = false;
         blocks = null;
         return false;
      }

      blocks = new(count);
      foreach (var child in bn.Children)
      {
         if (child is not BlockNode block)
         {
            ctx.SetPosition(child.KeyNode);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.InvalidNodeType,
                                           $"{callStack}.HasOnlyXBlocksAsChildren",
                                           child.GetType().Name,
                                           nameof(BlockNode),
                                           child.KeyNode.GetLexeme(source));
            validationResult = false;
            blocks = null;
            return false;
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
   /// <param name="bn"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="validation"></param>
   /// <param name="target"></param>
   /// <param name="nxProp"></param>
   public static void SetIdentifierList(this BlockNode bn,
                                        LocationContext ctx,
                                        string actionName,
                                        string source,
                                        ref bool validation,
                                        INexus target,
                                        Enum nxProp)
   {
      foreach (var sn in bn.Children)
      {
         if (!sn.IsKeyOnlyNode(ctx, source, actionName, ref validation, out var kn))
            continue;

         var identifier = kn.KeyNode.GetLexeme(source);
         Nx.AddToCollection(target, nxProp, identifier);
      }
   }
}