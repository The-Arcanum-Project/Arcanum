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

         if (globals.Contains(instance))
         {
            ctx.SetPosition(bn!.KeyNode);
            DiagnosticException.LogWarning(ctx,
                                           ParsingError.Instance.DuplicateObjectDefinition,
                                           actionStack,
                                           instance.UniqueKey,
                                           typeof(TTarget),
                                           "UniqueKey");
            validation = false;
            continue;
         }

         globals.Add(instance);
         SetPropsAndProcessNodes(ctx,
                                 actionStack,
                                 source,
                                 ref validation,
                                 propsParser,
                                 allowUnknownBlocks,
                                 bn,
                                 instance);
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

         if (!globals.TryAdd(instance.UniqueKey, instance))
         {
            ctx.SetPosition(bn!.KeyNode);
            DiagnosticException.LogWarning(ctx,
                                           ParsingError.Instance.DuplicateObjectDefinition,
                                           actionStack,
                                           instance.UniqueKey,
                                           typeof(TTarget),
                                           "UniqueKey");
            validation = false;
            continue;
         }

         SetPropsAndProcessNodes(ctx,
                                 actionStack,
                                 source,
                                 ref validation,
                                 propsParser,
                                 allowUnknownBlocks,
                                 bn,
                                 instance);
      }
   }

   private static void SetPropsAndProcessNodes<TTarget>(LocationContext ctx,
                                                        string actionStack,
                                                        string source,
                                                        ref bool validation,
                                                        Pdh.PropertyParser<TTarget> propsParser,
                                                        bool allowUnknownBlocks,
                                                        BlockNode? bn,
                                                        TTarget eu5Obj) where TTarget : IEu5Object<TTarget>, new()
   {
      var unknownNodes = propsParser(bn!, eu5Obj, ctx, source, ref validation);

      if (allowUnknownBlocks)
         foreach (var ukn in unknownNodes)
            ukn.IsBlockNode(ctx, source, actionStack, out _);
      else
         foreach (var ukn in unknownNodes)
         {
            if (!ukn.IsBlockNode(ctx, source, actionStack, ref validation, out var dbn))
               continue;

            ctx.SetPosition(dbn.KeyNode);
            DiagnosticException.LogWarning(ctx,
                                           ParsingError.Instance.UnknownKey,
                                           actionStack,
                                           dbn.KeyNode.GetLexeme(source),
                                           dbn.Children.Count);
            validation = false;
         }
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

      eu5Obj = new() { UniqueKey = bn.KeyNode.GetLexeme(source), Source = fileObj };
      return true;
   }
}