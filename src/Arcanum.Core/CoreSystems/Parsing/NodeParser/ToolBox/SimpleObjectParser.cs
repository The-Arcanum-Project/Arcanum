using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;

public static class SimpleObjectParser
{
   private static readonly UppercaseWordChecker _irChecker = new([
      "INJECT", "TRY_INJECT", "INJECT_OR_CREATE", "REPLACE", "TRY_REPLACE", "REPLACE_OR_CREATE",
   ]);

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

         // We check if we have an inject/replace and if so custom handling applies
         // todo: optimize by not calling GetLexeme twice
         if (_irChecker.IsMatch(bn.KeyNode.GetLexeme(source), out var irType))
         {
            // we already have a valid object created, so depending on the irType we now take action
            switch (irType)
            {
               case InjRepType.Inject:
               case InjRepType.TryInject:
               case InjRepType.InjectOrCreate:
                  if (!globals.TryGetValue(instance.UniqueId, out var injTarget))
                  {
                     if (irType == InjRepType.TryInject)
                        break;

                     if (irType == InjRepType.InjectOrCreate)
                     {
                        instance.InjRepType = InjRepType.InjectOrCreate;
                        if (lockObject != null)
                           lock (lockObject)
                              globals[instance.UniqueId] = instance;
                        else
                           globals[instance.UniqueId] = instance;
                        break;
                     }

                     ctx.SetPosition(bn.KeyNode);
                     De.Warning(ctx,
                                ParsingError.Instance.InjectReplaceTargetNotFound,
                                actionStack,
                                instance.UniqueId);
                     validation = false;
                     continue;
                  }

                  var injectTarget = (IEu5Object)injTarget;
                  var injectObj = InjectManager.CreateAndRegisterInjectObj(injectTarget, instance, irType);
                  injectTarget.MergeInjects(injectObj.InjectedProperties);

                  break;
               case InjRepType.Replace:
               case InjRepType.TryReplace:
               case InjRepType.ReplaceOrCreate:
                  if (!globals.ContainsKey(instance.UniqueId) && irType != InjRepType.ReplaceOrCreate)
                  {
                     if (irType == InjRepType.TryReplace)
                        break;

                     ctx.SetPosition(bn.KeyNode);
                     De.Warning(ctx,
                                ParsingError.Instance.InjectReplaceTargetNotFound,
                                actionStack,
                                instance.UniqueId);
                     validation = false;
                     continue;
                  }

                  instance.InjRepType = irType;
                  if (lockObject != null)
                     lock (lockObject)
                        globals[instance.UniqueId] = instance;
                  else
                     globals[instance.UniqueId] = instance;
                  break;
               case InjRepType.None:
               default:
                  Debug.Fail("Unhandled InjRepType in SimpleObjectParser.Parse");
                  break;
            }

            return;
         }

         // Otherwise we just add to globals as normal
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

   internal static bool ValidateAndCreateInstance<TTarget>(Eu5FileObj fileObj,
                                                           LocationContext ctx,
                                                           string actionStack,
                                                           string source,
                                                           ref bool validation,
                                                           StatementNode sn,
                                                           [MaybeNullWhen(false)] out BlockNode bn,
                                                           out TTarget? eu5Obj)
      where TTarget : IEu5Object<TTarget>, new()
   {
      if (!sn.IsBlockNode(ctx, source, actionStack, ref validation, out bn))
      {
         eu5Obj = default;
         return false;
      }

      eu5Obj = Eu5Activator.CreateInstance<TTarget>(bn.KeyNode.GetLexeme(source), fileObj, bn);
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

         var instance = Eu5Activator.CreateInstance<TTarget>(bn.KeyNode.GetLexeme(source), fileObj, bn);
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