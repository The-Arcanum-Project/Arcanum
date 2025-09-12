using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.GameObjects;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;

public static class SimpleObjectParser
{
   public static void Parse<TTarget>(RootNode rn,
                                     LocationContext ctx,
                                     string actionStack,
                                     string source,
                                     ref bool validation,
                                     Pdh.PropertyParser<TTarget> propsParser,
                                     List<TTarget> globals,
                                     bool allowUnknownBlocks = true) where TTarget : NameKeyDefined, INexus
   {
      foreach (var sn in rn.Statements)
      {
         if (!ValidateAndCreateInstance(ctx,
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
                                           instance.Name,
                                           typeof(TTarget),
                                           "Name");
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

   public static void Parse<TTarget>(RootNode rn,
                                     LocationContext ctx,
                                     string actionStack,
                                     string source,
                                     ref bool validation,
                                     Pdh.PropertyParser<TTarget> propsParser,
                                     Dictionary<string, TTarget> globals,
                                     bool allowUnknownBlocks = true) where TTarget : NameKeyDefined, INexus
   {
      foreach (var sn in rn.Statements)
      {
         if (!ValidateAndCreateInstance(ctx,
                                        actionStack,
                                        source,
                                        ref validation,
                                        sn,
                                        out var bn,
                                        out TTarget? instance) ||
             instance == null)
            continue;

         if (!globals.TryAdd(instance.Name, instance))
         {
            ctx.SetPosition(bn!.KeyNode);
            DiagnosticException.LogWarning(ctx,
                                           ParsingError.Instance.DuplicateObjectDefinition,
                                           actionStack,
                                           instance.Name,
                                           typeof(TTarget),
                                           "Name");
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
                                                        TTarget age) where TTarget : NameKeyDefined, INexus
   {
      var unknownNodes = propsParser(bn!, age, ctx, source, ref validation);

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

   private static bool ValidateAndCreateInstance<TTarget>(LocationContext ctx,
                                                          string actionStack,
                                                          string source,
                                                          ref bool validation,
                                                          StatementNode sn,
                                                          out BlockNode? bn,
                                                          out TTarget? nameKeyDefined)
      where TTarget : NameKeyDefined, INexus
   {
      if (!sn.IsBlockNode(ctx, source, actionStack, ref validation, out bn))
      {
         nameKeyDefined = null;
         return false;
      }

      var ageName = bn.KeyNode.GetLexeme(source);
      if (Activator.CreateInstance(typeof(TTarget), ageName) is not TTarget age)
         throw new
            InvalidOperationException($"Failed to create instance of type {typeof(TTarget)} with name key {ageName}");

      nameKeyDefined = age;
      return true;
   }
}