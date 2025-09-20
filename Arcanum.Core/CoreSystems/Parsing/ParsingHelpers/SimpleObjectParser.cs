using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;

public static class SimpleObjectParser
{
   public static void Parse<TTarget>(FileObj fileObj,
                                     RootNode rn,
                                     LocationContext ctx,
                                     string actionStack,
                                     string source,
                                     ref bool validation,
                                     Pdh.PropertyParser<TTarget> propsParser,
                                     List<TTarget> globals,
                                     object? lockObject,
                                     bool allowUnknownBlocks = true) where TTarget : IEu5Object<TTarget>, new()
   {
      foreach (var sn in rn.Statements)
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
         {
            lock (lockObject)
               if (globals.Contains(instance))
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
         else if (globals.Contains(instance))
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

         globals.Add(instance);
         propsParser(bn!, instance, ctx, source, ref validation, allowUnknownBlocks);
      }
   }

   public static void Parse<TTarget>(FileObj fileObj,
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

   public static void Parse<TTarget>(FileObj fileObj,
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

   private static bool ValidateAndCreateInstance<TTarget>(FileObj fileObj,
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

      eu5Obj = new() { UniqueId = bn.KeyNode.GetLexeme(source), Source = fileObj };
      return true;
   }
}