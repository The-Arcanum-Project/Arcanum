using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
   public static void Parse<TTarget>(Eu5FileObj fileObj,
                                     List<StatementNode> statements,
                                     ref ParsingContext pc,
                                     Pdh.PropertyParser<TTarget> propsParser,
                                     Dictionary<string, TTarget> globals,
                                     object? lockObject,
                                     bool allowUnknownBlocks = false) where TTarget : IEu5Object<TTarget>, new()
   {
      using var scope = pc.PushScope();
      foreach (var sn in statements)
      {
         if (!ValidateAndCreateInstance(fileObj,
                                        ref pc,
                                        sn,
                                        out var bn,
                                        out TTarget? instance,
                                        out var irType) ||
             instance == null)
            continue;

         // We check if we have an inject/replace and if so custom handling applies
         if (irType != InjRepType.None)
         {
            propsParser(bn, instance, ref pc, allowUnknownBlocks);
            // we already have a valid object created, so depending on the irType we now take action
            switch (irType)
            {
               case InjRepType.INJECT:
               case InjRepType.TRY_INJECT:
               case InjRepType.INJECT_OR_CREATE:
                  if (!globals.TryGetValue(instance.UniqueId, out var injTarget))
                  {
                     if (irType == InjRepType.TRY_INJECT)
                        break;

                     if (irType == InjRepType.INJECT_OR_CREATE)
                     {
                        instance.InjRepType = InjRepType.INJECT_OR_CREATE;
                        if (lockObject != null)
                           lock (lockObject)
                              globals[instance.UniqueId] = instance;
                        else
                           globals[instance.UniqueId] = instance;
                        break;
                     }

                     pc.SetContext(bn);
                     De.Warning(ref pc,
                                ParsingError.Instance.InjectReplaceTargetNotFound,
                                instance.UniqueId);
                     pc.Fail();
                     continue;
                  }

                  var injectTarget = (IEu5Object)injTarget;
                  var injectObj = InjectManager.CreateAndRegisterInjectObj(injectTarget, instance, irType);
                  injectTarget.MergeInjects(injectObj.InjectedProperties);

                  break;
               case InjRepType.REPLACE:
               case InjRepType.TRY_REPLACE:
               case InjRepType.REPLACE_OR_CREATE:
                  if (!globals.ContainsKey(instance.UniqueId) && irType != InjRepType.REPLACE_OR_CREATE)
                  {
                     if (irType == InjRepType.TRY_REPLACE)
                        break;

                     pc.SetContext(bn);
                     De.Warning(ref pc,
                                ParsingError.Instance.InjectReplaceTargetNotFound,
                                instance.UniqueId);
                     pc.Fail();
                     continue;
                  }

                  instance.InjRepType = irType;
                  if (lockObject != null)
                     lock (lockObject)
                        globals[instance.UniqueId] = instance;
                  else
                     globals[instance.UniqueId] = instance;
                  break;
               default:
                  Debug.Fail("Unhandled InjRepType in SimpleObjectParser.Parse");
                  break;
            }

            continue;
         }

         // Otherwise we just add to globals as normal
         if (lockObject != null)
            lock (lockObject)
            {
               if (!globals.TryAdd(instance.UniqueId, instance))
               {
                  pc.SetContext(bn);
                  DiagnosticException.LogWarning(ref pc,
                                                 ParsingError.Instance.DuplicateObjectDefinition,
                                                 instance.UniqueId,
                                                 typeof(TTarget),
                                                 "UniqueId");
                  pc.Fail();
                  continue;
               }
            }
         else if (!globals.TryAdd(instance.UniqueId, instance))
         {
            pc.SetContext(bn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.DuplicateObjectDefinition,
                                           instance.UniqueId,
                                           typeof(TTarget),
                                           "UniqueId");
            pc.Fail();
            continue;
         }

         propsParser(bn, instance, ref pc, allowUnknownBlocks);
      }
   }

   public static void Parse<TTarget>(Eu5FileObj fileObj,
                                     RootNode rn,
                                     ref ParsingContext pc,
                                     Pdh.PropertyParser<TTarget> propsParser,
                                     Dictionary<string, TTarget> globals,
                                     object? lockObject,
                                     bool allowUnknownBlocks = false) where TTarget : IEu5Object<TTarget>, new()
   {
      Parse(fileObj,
            rn.Statements,
            ref pc,
            propsParser,
            globals,
            lockObject,
            allowUnknownBlocks);
   }

   public static bool Parse<TTarget>(Eu5FileObj fileObj,
                                     StatementNode sn,
                                     ref ParsingContext pc,
                                     Pdh.PropertyParser<TTarget> propsParser,
                                     out TTarget? instance,
                                     bool allowUnknownBlocks = false) where TTarget : IEu5Object<TTarget>, new()
   {
      using var scope = pc.PushScope();
      if (!ValidateAndCreateInstance(fileObj,
                                     ref pc,
                                     sn,
                                     out var bn,
                                     out instance,
                                     out _) ||
          instance == null)
         return false;

      propsParser(bn, instance, ref pc, allowUnknownBlocks);
      return true;
   }

   private static bool ValidateAndCreateInstance<TTarget>(Eu5FileObj fileObj,
                                                          ref ParsingContext pc,
                                                          StatementNode sn,
                                                          [MaybeNullWhen(false)] out BlockNode bn,
                                                          out TTarget? eu5Obj,
                                                          out InjRepType irType)
      where TTarget : IEu5Object<TTarget>, new()
   {
      using var scope = pc.PushScope();
      if (!sn.IsBlockNode(ref pc, out bn))
      {
         eu5Obj = default;
         irType = InjRepType.None;
         return false;
      }

      UppercaseWordChecker.IsMatch(pc.SliceString(bn), out irType, out var trueKey);
      eu5Obj = Eu5Activator.CreateInstance<TTarget>(trueKey, fileObj, bn);
      return true;
   }

   /// <summary>
   /// Discover object declarations in a list of statements and add them to the provided dictionary.
   /// This method only creates instances of the objects without parsing their properties.
   /// </summary>
   public static void DiscoverObjectDeclarations<TTarget>(List<StatementNode> statements,
                                                          Eu5FileObj fileObj,
                                                          ref ParsingContext pc,
                                                          Dictionary<string, TTarget> globals,
                                                          object? lockObject) where TTarget : IEu5Object<TTarget>, new()
   {
      using var scope = pc.PushScope();
      foreach (var sn in statements)
      {
         if (!sn.IsBlockNode(ref pc, out var bn))
            continue;

         var instance = Eu5Activator.CreateInstance<TTarget>(pc.SliceString(bn), fileObj, bn);
         Debug.Assert(instance.Source != null);

         if (lockObject != null)
         {
            lock (lockObject)
               if (!globals.TryAdd(instance.UniqueId, instance))
               {
                  pc.SetContext(bn);
                  DiagnosticException.LogWarning(ref pc,
                                                 ParsingError.Instance.DuplicateObjectDefinition,
                                                 pc.SliceString(bn),
                                                 typeof(TTarget),
                                                 "UniqueId");
               }
         }
         else if (!globals.TryAdd(instance.UniqueId, instance))
         {
            pc.SetContext(bn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.DuplicateObjectDefinition,
                                           pc.SliceString(bn),
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
                                                               ref ParsingContext pc,
                                                               Pdh.PropertyParser<TTarget> propsParser,
                                                               Dictionary<string, TTarget> globals)
      where TTarget : INexus
   {
      ParseDiscoveredObjectProperties(rn.Statements,
                                      ref pc,
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
                                                               ref ParsingContext pc,
                                                               Pdh.PropertyParser<TTarget> propsParser,
                                                               Dictionary<string, TTarget> globals)
      where TTarget : INexus
   {
      using var scope = pc.PushScope();
      foreach (var sn in statements)
      {
         if (!sn.IsBlockNode(ref pc, out var bn))
            continue;

         var key = pc.SliceString(bn);
         if (!globals.TryGetValue(key, out var discoveredObj))
         {
            pc.SetContext(bn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidObjectKey,
                                           key,
                                           typeof(TTarget));
            continue;
         }

         propsParser(bn, discoveredObj, ref pc, false);
      }
   }

   /// <summary>
   /// If the provided <see cref="RootNode"/> contains only a single grouping node (e.g. "topographies"),
   /// this method will strip that node and return its children as a list of statements.
   /// </summary>
   public static bool StripGroupingNodes(RootNode rn,
                                         ref ParsingContext pc,
                                         string groupingNodeKey,
                                         out List<StatementNode> statements)
   {
      using var scope = pc.PushScope();
      statements = rn.Statements;
      if (rn.Statements.Count == 1 &&
          rn.Statements[0].IsBlockNode(ref pc, out var bn) &&
          groupingNodeKey == pc.SliceString(bn))
      {
         statements = bn.Children;
         return true;
      }

      pc.SetContext(rn.Statements[0]);
      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.InvalidGroupingNode,
                                     groupingNodeKey);
      return pc.Fail();
   }

   /// <summary>
   /// If the provided <see cref="BlockNode"/> contains only a single grouping node (e.g. "topographies"),
   /// this method will strip that node and return its children as a list of statements.
   /// </summary>
   public static bool StripGroupingNodes(BlockNode rn,
                                         ref ParsingContext pc,
                                         string groupingNodeKey,
                                         out List<StatementNode> statements)
   {
      using var scope = pc.PushScope();
      statements = rn.Children;
      if (rn.Children.Count == 1 &&
          rn.Children[0].IsBlockNode(ref pc, out var bn) &&
          groupingNodeKey == pc.SliceString(bn))
      {
         statements = bn.Children;
         return true;
      }

      pc.SetContext(rn.Children[0]);
      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.InvalidGroupingNode,
                                     groupingNodeKey);
      return pc.Fail();
   }
}