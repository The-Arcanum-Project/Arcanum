using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class KnHelper
{
   public static bool IsSimpleKeyNode(this KeyNodeBase node,
                                      ref ParsingContext pc,
                                      [MaybeNullWhen(false)] out SimpleKeyNode value)
   {
      if (node is SimpleKeyNode skn)
      {
         value = skn;
         return true;
      }

      pc.SetContext(node);
      var key = pc.SliceString(node);
      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.InvalidNodeType,
                                     $"{node.GetType().Name}({key})",
                                     $"{nameof(BlockNode)}",
                                     key);
      value = null!;
      return false;
   }
}