using System.Diagnostics;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Parsing.ToolBox;

public static class SimpleObjectParser
{
   public static void Parse<TTarget>(Eu5FileObj fileObj,
                                     List<StatementNode> statements,
                                     LocationContext ctx,
                                     string actionStack,
                                     string source,
                                     ref bool validation,
                                     Pdh.PropertyParser<TTarget> propsParser,
                                     Dictionary<string, TTarget> globals,
                                     object? lockObject,
                                     bool allowUnknownBlocks = false) where TTarget : IEu5Object<TTarget>, new()
   {
      foreach (var sn in statements)
      {
         if (!ValidateAndCreateInstance(fileObj,
                                        ctx,
                                        actionStack,
                                        source,
                                        ref validation,
                                        sn,
                                        out var bn,
                                        out TTarget? instance) ||
             instance == null)
            continue;

         if (lockObject != null)
            lock (lockObject)
            {
               if (!globals.TryAdd(instance.UniqueId, instance))
               {
                  ctx.SetPosition(bn!.KeyNode);
                  DiagnosticException.LogWarning(ctx,
                                                 ParsingError.Instance.DuplicateObjectDefinition,
                                                 actionStack,
                                                 instance.UniqueId,
                                                 typeof(TTarget),
                                                 "UniqueId");
                  validation = false;
                  continue;
               }
            }
         else if (!globals.TryAdd(instance.UniqueId, instance))
         {
            ctx.SetPosition(bn!.KeyNode);
            DiagnosticException.LogWarning(ctx,
                                           ParsingError.Instance.DuplicateObjectDefinition,
                                           actionStack,
                                           instance.UniqueId,
                                           typeof(TTarget),
                                           "UniqueId");
            validation = false;
            continue;
         }

         propsParser(bn!, instance, ctx, source, ref validation, allowUnknownBlocks);
      }
   }

   public static void Parse<TTarget>(Eu5FileObj fileObj,
                                     RootNode rn,
                                     LocationContext ctx,
                                     string actionStack,
                                     string source,
                                     ref bool validation,
                                     Pdh.PropertyParser<TTarget> propsParser,
                                     Dictionary<string, TTarget> globals,
                                     object? lockObject,
                                     bool allowUnknownBlocks = false) where TTarget : IEu5Object<TTarget>, new()
   {
      Parse(fileObj,
            rn.Statements,
            ctx,
            actionStack,
            source,
            ref validation,
            propsParser,
            globals,
            lockObject,
            allowUnknownBlocks);
   }

   public static bool Parse<TTarget>(Eu5FileObj fileObj,
                                     StatementNode sn,
                                     LocationContext ctx,
                                     string actionStack,
                                     string source,
                                     ref bool validation,
                                     Pdh.PropertyParser<TTarget> propsParser,
                                     out TTarget? instance,
                                     bool allowUnknownBlocks = false) where TTarget : IEu5Object<TTarget>, new()
   {
      if (!ValidateAndCreateInstance(fileObj,
                                     ctx,
                                     actionStack,
                                     source,
                                     ref validation,
                                     sn,
                                     out var bn,
                                     out instance) ||
          instance == null)
         return false;

      propsParser(bn!, instance, ctx, source, ref validation, allowUnknownBlocks);
      return true;
   }

   private static bool ValidateAndCreateInstance<TTarget>(Eu5FileObj fileObj,
                                                          LocationContext ctx,
                                                          string actionStack,
                                                          string source,
                                                          ref bool validation,
                                                          StatementNode sn,
                                                          out BlockNode? bn,
                                                          out TTarget? eu5Obj)
      where TTarget : IEu5Object<TTarget>, new()
   {
      if (!sn.IsBlockNode(ctx, source, actionStack, ref validation, out bn))
      {
         eu5Obj = default;
         return false;
      }

      eu5Obj = IEu5Object<TTarget>.CreateInstance(bn.KeyNode.GetLexeme(source), fileObj);
      return true;
   }

   /// <summary>
   /// Discover object declarations in a list of statements and add them to the provided dictionary.
   /// This method only creates instances of the objects without parsing their properties.
   /// </summary>
   public static void DiscoverObjectDeclarations<TTarget>(List<StatementNode> statements,
                                                          LocationContext ctx,
                                                          Eu5FileObj fileObj,
                                                          string actionStack,
                                                          string source,
                                                          ref bool validation,
                                                          Dictionary<string, TTarget> globals,
                                                          object? lockObject) where TTarget : IEu5Object<TTarget>, new()
   {
      foreach (var sn in statements)
      {
         if (!sn.IsBlockNode(ctx, source, actionStack, ref validation, out var bn))
            continue;

         var instance = IEu5Object<TTarget>.CreateInstance(bn.KeyNode.GetLexeme(source), fileObj);

         Debug.Assert(instance.Source != null, "instance.Source != null");

         if (lockObject != null)
         {
            lock (lockObject)
               if (!globals.TryAdd(instance.UniqueId, instance))
               {
                  ctx.SetPosition(bn.KeyNode);
                  DiagnosticException.LogWarning(ctx,
                                                 ParsingError.Instance.DuplicateObjectDefinition,
                                                 actionStack,
                                                 bn.KeyNode.GetLexeme(source),
                                                 typeof(TTarget),
                                                 "UniqueId");
               }
         }
         else if (!globals.TryAdd(instance.UniqueId, instance))
         {
            ctx.SetPosition(bn.KeyNode);
            DiagnosticException.LogWarning(ctx,
                                           ParsingError.Instance.DuplicateObjectDefinition,
                                           actionStack,
                                           bn.KeyNode.GetLexeme(source),
                                           typeof(TTarget),
                                           "UniqueId");
         }
      }
   }

   /// <summary>
   /// ONLY USE AFTER <see cref="DiscoverObjectDeclarations{TTarget}"/> HAS BEEN CALLED!
   /// This method parses properties of already discovered objects.
   /// It will log a warning if an object key is found that does not exist in the provided dictionary.
   /// This method does NOT create new instances of objects.
   /// </summary>
   public static void ParseDiscoveredObjectProperties<TTarget>(RootNode rn,
                                                               LocationContext ctx,
                                                               string actionStack,
                                                               string source,
                                                               ref bool validation,
                                                               Pdh.PropertyParser<TTarget> propsParser,
                                                               Dictionary<string, TTarget> globals)
      where TTarget : INexus
   {
      ParseDiscoveredObjectProperties(rn.Statements,
                                      ctx,
                                      actionStack,
                                      source,
                                      ref validation,
                                      propsParser,
                                      globals);
   }

   /// <summary>
   /// ONLY USE AFTER <see cref="DiscoverObjectDeclarations{TTarget}"/> HAS BEEN CALLED!
   /// This method parses properties of already discovered objects.
   /// It will log a warning if an object key is found that does not exist in the provided dictionary.
   /// This method does NOT create new instances of objects.
   /// </summary>
   public static void ParseDiscoveredObjectProperties<TTarget>(List<StatementNode> statements,
                                                               LocationContext ctx,
                                                               string actionStack,
                                                               string source,
                                                               ref bool validation,
                                                               Pdh.PropertyParser<TTarget> propsParser,
                                                               Dictionary<string, TTarget> globals)
      where TTarget : INexus
   {
      foreach (var sn in statements)
      {
         if (!sn.IsBlockNode(ctx, source, actionStack, ref validation, out var bn))
            continue;

         var key = bn.KeyNode.GetLexeme(source);
         if (!globals.TryGetValue(key, out var discoveredObj))
         {
            ctx.SetPosition(bn.KeyNode);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.InvalidObjectKey,
                                           actionStack,
                                           key,
                                           typeof(TTarget));
            continue;
         }

         propsParser(bn, discoveredObj, ctx, source, ref validation, false);
      }
   }

   /// <summary>
   /// If the provided <see cref="RootNode"/> contains only a single grouping node (e.g. "topographies"),
   /// this method will strip that node and return its children as a list of statements.
   /// </summary>
   public static bool StripGroupingNodes(RootNode rn,
                                         LocationContext ctx,
                                         string actionStack,
                                         string source,
                                         ref bool validation,
                                         string groupingNodeKey,
                                         out List<StatementNode> statements)
   {
      statements = rn.Statements;
      if (rn.Statements.Count == 1 &&
          rn.Statements[0].IsBlockNode(ctx, source, actionStack, ref validation, out var bn) &&
          groupingNodeKey == bn.KeyNode.GetLexeme(source))
      {
         statements = bn.Children;
         return true;
      }

      ctx.SetPosition(rn.Statements[0].KeyNode);
      DiagnosticException.LogWarning(ctx.GetInstance(),
                                     ParsingError.Instance.InvalidGroupingNode,
                                     actionStack,
                                     groupingNodeKey);
      validation = false;
      return false;
   }

   /// <summary>
   /// If the provided <see cref="BlockNode"/> contains only a single grouping node (e.g. "topographies"),
   /// this method will strip that node and return its children as a list of statements.
   /// </summary>
   public static bool StripGroupingNodes(BlockNode rn,
                                         LocationContext ctx,
                                         string actionStack,
                                         string source,
                                         ref bool validation,
                                         string groupingNodeKey,
                                         out List<StatementNode> statements)
   {
      statements = rn.Children;
      if (rn.Children.Count == 1 &&
          rn.Children[0].IsBlockNode(ctx, source, actionStack, ref validation, out var bn) &&
          groupingNodeKey == bn.KeyNode.GetLexeme(source))
      {
         statements = bn.Children;
         return true;
      }

      ctx.SetPosition(rn.Children[0].KeyNode);
      DiagnosticException.LogWarning(ctx.GetInstance(),
                                     ParsingError.Instance.InvalidGroupingNode,
                                     actionStack,
                                     groupingNodeKey);
      validation = false;
      return false;
   }
}