using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

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
}