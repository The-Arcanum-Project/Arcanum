using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Parsing.ToolBox;

public static class Pdh
{
   public delegate bool ContentParser<in TTarget>(ContentNode cn,
                                                  TTarget target,
                                                  LocationContext ctx,
                                                  string source,
                                                  ref bool validation)
      where TTarget : INexus;

   public delegate bool BlockParser<in TTarget>(BlockNode bn,
                                                TTarget target,
                                                LocationContext ctx,
                                                string source,
                                                ref bool validation)
      where TTarget : INexus;

   /// <summary>
   /// Dispatches the content node to the appropriate parser based on the key.
   /// </summary>
   /// <param name="cn"></param>
   /// <param name="target"></param>
   /// <param name="ctx"></param>
   /// <param name="source"></param>
   /// <param name="actionName"></param>
   /// <param name="validation"></param>
   /// <param name="parsers"></param>
   /// <typeparam name="TTarget"></typeparam>
   public static void DispatchContentNode<TTarget>(
      ContentNode cn,
      TTarget target,
      LocationContext ctx,
      string source,
      string actionName,
      Dictionary<string, ContentParser<TTarget>> parsers,
      ref bool validation) where TTarget : INexus
   {
      var key = cn.KeyNode.GetLexeme(source);
      if (parsers.TryGetValue(key, out var parser))
      {
         parser(cn, target, ctx, source, ref validation);
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
   /// <param name="validation"></param>
   /// <param name="parsers"></param>
   /// <typeparam name="TTarget"></typeparam>
   public static void DispatchBlockNode<TTarget>(
      BlockNode bn,
      TTarget target,
      LocationContext ctx,
      string source,
      string actionName,
      Dictionary<string, BlockParser<TTarget>> parsers,
      ref bool validation) where TTarget : INexus
   {
      var key = bn.KeyNode.GetLexeme(source);
      if (parsers.TryGetValue(key, out var parser))
      {
         parser(bn, target, ctx, source, ref validation);
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

   public static void DispatchStatementNode<TTarget>(
      StatementNode sn,
      TTarget target,
      LocationContext ctx,
      string source,
      string actionName,
      Dictionary<string, ContentParser<TTarget>> contentParsers,
      Dictionary<string, BlockParser<TTarget>> blockParsers,
      ref bool validation) where TTarget : INexus
   {
      if (sn is ContentNode cn)
         DispatchContentNode(cn, target, ctx, source, actionName, contentParsers, ref validation);
      else if (sn is BlockNode bn)
         DispatchBlockNode(bn, target, ctx, source, actionName, blockParsers, ref validation);
      else
      {
         validation = false;
         ctx.SetPosition(sn.KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        actionName,
                                        sn.GetType().Name,
                                        "ContentNode or BlockNode",
                                        sn.KeyNode.GetLexeme(source));
      }
   }
}