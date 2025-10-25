using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class KnHelper
{
   public static bool IsSimpleKeyNode(this KeyNodeBase node,
                                      LocationContext ctx,
                                      string source,
                                      string actionName,
                                      [MaybeNullWhen(false)] out SimpleKeyNode value)
   {
      if (node is SimpleKeyNode skn)
      {
         value = skn;
         return true;
      }

      ctx.SetPosition(node);
      DiagnosticException.LogWarning(ctx.GetInstance(),
                                     ParsingError.Instance.InvalidNodeType,
                                     actionName,
                                     $"{node.GetType().Name}({node.GetLexeme(source)})",
                                     $"{nameof(BlockNode)}",
                                     node.GetLexeme(source));
      value = null!;
      return false;
   }
}