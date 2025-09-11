using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Parsing.ToolBox;

public static class Pdh
{
   public delegate bool ContentParser<in TTarget>(ContentNode cn, TTarget target, LocationContext ctx, string source)
      where TTarget : INexus;

   public delegate bool BlockParser<in TTarget>(BlockNode bn, TTarget target, LocationContext ctx, string source)
      where TTarget : INexus;

   /// <summary>
   /// Dispatches the content node to the appropriate parser based on the key.
   /// </summary>
   /// <param name="cn"></param>
   /// <param name="target"></param>
   /// <param name="ctx"></param>
   /// <param name="source"></param>
   /// <param name="actionName"></param>
   /// <param name="parsers"></param>
   /// <typeparam name="TTarget"></typeparam>
   public static void DispatchContentNode<TTarget>(
      ContentNode cn,
      TTarget target,
      LocationContext ctx,
      string source,
      string actionName,
      IReadOnlyDictionary<string, ContentParser<TTarget>> parsers) where TTarget : INexus
   {
      var key = cn.KeyNode.GetLexeme(source);
      if (parsers.TryGetValue(key, out var parser))
      {
         parser(cn, target, ctx, source);
      }
      else
      {
         ctx.SetPosition(cn.KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidContentKeyOrType,
                                        actionName,
                                        key,
                                        string.Join(", ", parsers.Keys));
      }
   }

   /// <summary>
   /// Dispatches the block node to the appropriate parser based on the key.
   /// </summary>
   /// <param name="bn"></param>
   /// <param name="target"></param>
   /// <param name="ctx"></param>
   /// <param name="source"></param>
   /// <param name="actionName"></param>
   /// <param name="parsers"></param>
   /// <typeparam name="TTarget"></typeparam>
   public static void DispatchBlockNode<TTarget>(
      BlockNode bn,
      TTarget target,
      LocationContext ctx,
      string source,
      string actionName,
      IReadOnlyDictionary<string, BlockParser<TTarget>> parsers) where TTarget : INexus
   {
      var key = bn.KeyNode.GetLexeme(source);
      if (parsers.TryGetValue(key, out var parser))
      {
         parser(bn, target, ctx, source);
      }
      else
      {
         ctx.SetPosition(bn.KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidBlockName,
                                        actionName,
                                        key,
                                        string.Join(", ", parsers.Keys));
      }
   }
}