using System.Diagnostics;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.GameObjects.BaseTypes;
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

   public delegate List<StatementNode> PropertyParser<in TTarget>(BlockNode block,
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

   /// <summary>
   /// Parses all properties of a target object by iterating through the children of a BlockNode
   /// and dispatching them to the appropriate parsers.
   /// </summary>
   /// <typeparam name="TTarget">The type of the object being populated.</typeparam>
   /// <param name="block">The BlockNode containing the properties to parse.</param>
   /// <param name="target">The target object instance to populate.</param>
   /// <param name="ctx">The location context for error reporting.</param>
   /// <param name="source">The source code string.</param>
   /// <param name="validation">A reference to the overall validation flag.</param>
   /// <param name="contentParsers">The dictionary of parsers for ContentNodes.</param>
   /// <param name="blockParsers">The dictionary of parsers for BlockNodes.</param>
   /// <returns>A list of all child StatementNodes that were not handled by any parser.</returns>
   public static List<StatementNode> ParseProperties<TTarget>(
      BlockNode block,
      TTarget target,
      LocationContext ctx,
      string source,
      ref bool validation,
      IReadOnlyDictionary<string, ContentParser<TTarget>> contentParsers,
      IReadOnlyDictionary<string, BlockParser<TTarget>> blockParsers) where TTarget : INexus
   {
      var unhandledNodes = new List<StatementNode>();

      foreach (var propertyNode in block.Children)
      {
         var wasHandled = false;
         if (propertyNode is ContentNode cn)
         {
            var key = cn.KeyNode.GetLexeme(source);
            if (contentParsers.TryGetValue(key, out var parser))
            {
               parser(cn, target, ctx, source, ref validation);
               wasHandled = true;
            }
         }
         else if (propertyNode is BlockNode bn)
         {
            var key = bn.KeyNode.GetLexeme(source);
            if (blockParsers.TryGetValue(key, out var parser))
            {
               parser(bn, target, ctx, source, ref validation);
               wasHandled = true;
            }
         }

         if (!wasHandled)
            unhandledNodes.Add(propertyNode);
      }

      return unhandledNodes;
   }
}