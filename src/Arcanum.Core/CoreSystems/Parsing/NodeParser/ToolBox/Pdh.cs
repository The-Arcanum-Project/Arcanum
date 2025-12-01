using System.Diagnostics;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.GameObjects.BaseTypes;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;

public static class Pdh
{
   public delegate bool ContentParser<in TTarget>(ContentNode cn,
                                                  TTarget target,
                                                  ref ParsingContext pc)
      where TTarget : INexus;

   public delegate bool ContentItemParser<TItem>(
      ContentNode node,
      ref ParsingContext pc,
      out TItem value);

   public delegate bool KeyOnlyItemParser<TItem>(
      KeyOnlyNode node,
      ref ParsingContext pc,
      out TItem value);

   public delegate bool BlockItemParser<in TTarget>(
      BlockNode block,
      TTarget target,
      ref ParsingContext pc)
      where TTarget : INexus;

   public delegate void EmbeddedItemParser<in TTarget>(
      BlockNode block,
      TTarget target,
      ref ParsingContext pc,
      bool allowUnknownNodes)
      where TTarget : INexus;

   public delegate bool DynamicParserPredicate(string key);

   public delegate bool BlockParser<in TTarget>(BlockNode bn,
                                                TTarget target,
                                                ref ParsingContext pc)
      where TTarget : INexus;

   public delegate bool StatementParser<in TTarget>(StatementNode bn,
                                                    TTarget target,
                                                    ref ParsingContext pc)
      where TTarget : INexus;

   public delegate void PropertyParser<in TTarget>(BlockNode block,
                                                   TTarget target,
                                                   ref ParsingContext pc,
                                                   bool complainOnUnknownNodes)
      where TTarget : INexus;

   /// <summary>
   /// Dispatches the content node to the appropriate parser based on the key.
   /// </summary>
   public static void DispatchContentNode<TTarget>(
      ContentNode cn,
      TTarget target,
      ref ParsingContext pc,
      Dictionary<string, ContentParser<TTarget>> parsers) where TTarget : INexus
   {
      using var scope = pc.PushScope();
      var key = pc.SliceString(cn);
      if (parsers.TryGetValue(key, out var parser))
      {
         parser(cn, target, ref pc);
      }
      else
      {
         pc.SetContext(cn);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidContentKeyOrType,
                                        key,
                                        string.Join(", ", parsers.Keys));
      }
   }

   /// <summary>
   /// Dispatches the block node to the appropriate parser based on the key.
   /// </summary>
   /// 
   public static void DispatchBlockNode<TTarget>(
      BlockNode bn,
      TTarget target,
      ref ParsingContext pc,
      Dictionary<string, BlockParser<TTarget>> parsers) where TTarget : INexus
   {
      using var scope = pc.PushScope();
      var key = pc.SliceString(bn);
      if (parsers.TryGetValue(key, out var parser))
      {
         parser(bn, target, ref pc);
      }
      else
      {
         pc.SetContext(bn);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidBlockName,
                                        key,
                                        string.Join(", ", parsers.Keys));
      }
   }

   public static void DispatchStatementNode<TTarget>(
      StatementNode sn,
      TTarget target,
      ref ParsingContext pc,
      Dictionary<string, ContentParser<TTarget>> contentParsers,
      Dictionary<string, BlockParser<TTarget>> blockParsers) where TTarget : INexus
   {
      using var scope = pc.PushScope();
      switch (sn)
      {
         case ContentNode cn:
            DispatchContentNode(cn, target, ref pc, contentParsers);
            break;
         case BlockNode bn:
            DispatchBlockNode(bn, target, ref pc, blockParsers);
            break;
         default:
            pc.Fail();
            pc.SetContext(sn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidNodeType,
                                           sn.GetType().Name,
                                           "ContentNode or BlockNode",
                                           pc.SliceString(sn));
            break;
      }
   }

   /// <summary>
   /// Parses all properties of a target object by iterating through the children of a BlockNode
   /// and dispatching them to the appropriate parsers.
   /// </summary>
   /// <returns>A list of all child StatementNodes that were not handled by any parser.</returns>
   public static void ParseProperties<TTarget>(BlockNode block,
                                               TTarget target,
                                               ref ParsingContext pc,
                                               IReadOnlyDictionary<string, ContentParser<TTarget>> contentParsers,
                                               IReadOnlyDictionary<string, BlockParser<TTarget>> blockParsers,
                                               IReadOnlyDictionary<string, StatementParser<TTarget>> statementParsers,
                                               List<BlockParser<TTarget>> dynamicBlockParsers,
                                               List<ContentParser<TTarget>> dynamicContentParsers,
                                               HashSet<string> ignoredBlockKeys,
                                               HashSet<string> ignoredContentKeys,
                                               bool allowUnknownNodes = false) where TTarget : INexus
   {
      using var scope = pc.PushScope();
      foreach (var sn in block.Children)
      {
         var key = pc.SliceString(sn);
         var wasHandled = false;
         if (sn is ContentNode cn)
         {
            if (contentParsers.TryGetValue(key, out var parser))
            {
               parser(cn, target, ref pc);
               wasHandled = true;
            }
            else
            {
               foreach (var dcParser in dynamicContentParsers)
               {
                  if (!dcParser(cn, target, ref pc))
                     continue;

                  wasHandled = true;
                  break;
               }
            }
         }
         else if (sn is BlockNode bn)
         {
            if (blockParsers.TryGetValue(key, out var parser))
            {
               parser(bn, target, ref pc);
               wasHandled = true;
            }
            else
            {
               foreach (var dbParser in dynamicBlockParsers)
               {
                  if (!dbParser(bn, target, ref pc))
                     continue;

                  wasHandled = true;
                  break;
               }
            }
         }

         if (wasHandled)
            continue;

         if (statementParsers.TryGetValue(pc.SliceString(sn), out var snParser))
         {
            snParser(sn, target, ref pc);
            continue;
         }

         if (allowUnknownNodes)
            continue;

         if (sn is ContentNode cNode && ignoredContentKeys.Contains(pc.SliceString(cNode)))
            continue;
         if (sn is BlockNode bNode && ignoredBlockKeys.Contains(pc.SliceString(bNode)))
            continue;

         pc.SetContext(sn);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidNodeType,
                                        sn.GetType().Name,
                                        "ContentNode or BlockNode or is node type is correct no parse in the dictionaries was found.",
                                        pc.SliceString(sn));
      }
   }

   public static ObservableRangeCollection<T> ParseContentCollection<T>(BlockNode node,
                                                                        ref ParsingContext pc,
                                                                        ContentItemParser<T> itemParser)
   {
      using var scope = pc.PushScope();
      var results = new ObservableRangeCollection<T>();
      if (node.Children.Count == 0)
         return results;

      foreach (var sn in node.Children)
      {
         if (!sn.IsContentNode(ref pc, out var cn))
            continue;

         if (itemParser(cn, ref pc, out var item))
            results.Add(item);
      }

      return results;
   }

   public static ObservableRangeCollection<T> ParseKeyOnlyCollection<T>(BlockNode node,
                                                                        ref ParsingContext pc,
                                                                        KeyOnlyItemParser<T> itemParser)
   {
      using var scope = pc.PushScope();
      var results = new ObservableRangeCollection<T>();
      if (node.Children.Count == 0)
         return results;

      foreach (var sn in node.Children)
      {
         if (!sn.IsKeyOnlyNode(ref pc, out var kon))
            continue;

         if (itemParser(kon, ref pc, out var item))
            results.Add(item);
      }

      return results;
   }

   public static ObservableRangeCollection<T> ParseBlockCollection<T>(BlockNode node,
                                                                      ref ParsingContext pc,
                                                                      BlockItemParser<T> itemParser) where T : INexus, new()
   {
      using var scope = pc.PushScope();
      var results = new ObservableRangeCollection<T>();
      if (node.Children.Count == 0)
         return results;

      foreach (var sn in node.Children)
      {
         if (!sn.IsBlockNode(ref pc, out var bn))
            continue;

         var newInstance = new T();
         Debug.Assert(newInstance != null, "newInstance != null");

         if (itemParser(bn, newInstance, ref pc))
            results.Add(newInstance);
      }

      return results;
   }

   public static ObservableRangeCollection<T> ParseEmbeddedCollection<T>(BlockNode node,
                                                                         ref ParsingContext pc,
                                                                         EmbeddedItemParser<T> itemParser,
                                                                         bool allowUnknownNodes)
      where T : INexus, IEu5Object<T>, new()
   {
      using var scope = pc.PushScope();
      var results = new ObservableRangeCollection<T>();
      if (node.Children.Count == 0)
         return results;

      foreach (var sn in node.Children)
      {
         if (!sn.IsBlockNode(ref pc, out var bn))
            continue;

         var newInstance = new T { UniqueId = pc.SliceString(sn) };
         Debug.Assert(newInstance != null, "newInstance != null");
         itemParser(bn, newInstance, ref pc, allowUnknownNodes);
         results.Add(newInstance);
      }

      return results;
   }

   public static ObservableHashSet<T> ParseKeyOnlyObservableHashSet<T>(BlockNode node,
                                                                       ref ParsingContext pc,
                                                                       KeyOnlyItemParser<T> itemParser)
   {
      using var scope = pc.PushScope();
      var results = new ObservableHashSet<T>();
      if (node.Children.Count == 0)
         return results;

      foreach (var sn in node.Children)
      {
         if (!sn.IsKeyOnlyNode(ref pc, out var kon))
            continue;

         if (itemParser(kon, ref pc, out var item))
            if (!results.Add(item))
            {
               pc.SetContext(kon);
               DiagnosticException.LogWarning(ref pc,
                                              ParsingError.Instance.DuplicateItemInCollection,
                                              pc.SliceString(kon),
                                              pc.SliceString(node));
            }
      }

      return results;
   }

   public static ObservableHashSet<T> ParseEmbeddedObservableHashSet<T>(BlockNode node,
                                                                        ref ParsingContext pc,
                                                                        EmbeddedItemParser<T> itemParser,
                                                                        bool allowUnknownNodes)
      where T : INexus, IEu5Object<T>, new()
   {
      using var scope = pc.PushScope();
      var results = new ObservableHashSet<T>();
      if (node.Children.Count == 0)
         return results;

      foreach (var sn in node.Children)
      {
         if (!sn.IsBlockNode(ref pc, out var bn))
            continue;

         var newInstance = new T { UniqueId = pc.SliceString(sn) };
         Debug.Assert(newInstance != null, "newInstance != null");
         itemParser(bn, newInstance, ref pc, allowUnknownNodes);
         if (!results.Add(newInstance))
         {
            pc.SetContext(bn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.DuplicateItemInCollection,
                                           pc.SliceString(bn),
                                           pc.SliceString(node));
         }
      }

      return results;
   }

   public static IEu5Object ParseDynamicObject<TObject>(string key,
                                                        BlockNode bn,
                                                        ref ParsingContext pc,
                                                        PropertyParser<TObject> propsParser) where TObject : IEu5Object, new()
   {
      var instance = new TObject { UniqueId = key };
      propsParser(bn, instance, ref pc, false);
      return instance;
   }
}