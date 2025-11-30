using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class LUtil
{
   public static List<Location> LocationsFromStatementNodes(List<StatementNode> nodes,
                                                            LocationContext ctx,
                                                            string actionName,
                                                            string source)
   {
      var locations = new List<Location>(nodes.Count);

      foreach (var node in nodes)
      {
         if (node is not KeyOnlyNode kNode)
            continue;

         if (!ParseLocation(kNode, ctx, actionName, source, out var location))
            continue;

         locations.Add(location);
      }

      return locations;
   }

   public static bool ParseLocation(KeyOnlyNode kNode,
                                    LocationContext ctx,
                                    string actionName,
                                    string source,
                                    out Location location)
   {
      var key = kNode.KeyNode.GetLexeme(source);
      if (!Globals.Locations.TryGetValue(key, out location!))
      {
         ctx.SetPosition(kNode.KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidLocationKey,
                                        actionName,
                                        key);
         location = Location.Empty;
         return false;
      }

      return true;
   }

   public static bool ValidateNodeSeparatorAndNumberValue(ContentNode node,
                                                          LocationContext ctx,
                                                          string actionName,
                                                          string source,
                                                          ref bool validationResult,
                                                          [MaybeNullWhen(false)] out string value)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator, TokenType.Equals, ctx, actionName, ref validationResult))
      {
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNodeOptionalUnary(ctx,
                                                      actionName,
                                                      source,
                                                      ref validationResult,
                                                      out value,
                                                      out _))
         return false;

      return true;
   }

   public static bool TryGetFromGlobalsAndLog<T>(
      LocationContext ctx,
      Token token,
      string source,
      string actionStack,
      ref bool validationResult,
      Dictionary<string, T> globals,
      [MaybeNullWhen(false)] out T value) where T : IEu5Object
   {
      var lexeme = token.GetLexeme(source);
      if (!globals.TryGetValue(lexeme, out value))
      {
         ctx.SetPosition(token);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.UnknownKey,
                                        $"{actionStack}.TryGetFromGlobals",
                                        token.GetLexeme(source),
                                        typeof(T).Name);
         validationResult = false;
         return false;
      }

      return true;
   }

   public static bool TryGetFromGlobalsAndLog<T>(
      LocationContext ctx,
      ContentNode node,
      string source,
      string actionStack,
      ref bool validation,
      Dictionary<string, T> globals,
      [MaybeNullWhen(false)] out T value) where T : IEu5Object
   {
      if (SeparatorHelper.IsSeparatorOfType(node.Separator,
                                            TokenType.Equals,
                                            ctx,
                                            $"{actionStack}.{nameof(TryGetFromGlobalsAndLog)}") &&
          node.Value.IsLiteralValueNode(ctx, actionStack, ref validation, out var lvn))
         return TryGetFromGlobalsAndLog(ctx,
                                        lvn.Value,
                                        source,
                                        actionStack,
                                        ref validation,
                                        globals,
                                        out value);

      validation = false;
      value = default;
      return false;
   }

   public static bool TryGetFromGlobalsAndLog<T>(
      LocationContext ctx,
      KeyNodeBase node,
      string source,
      string actionStack,
      ref bool validationResult,
      Dictionary<string, T> globals,
      [MaybeNullWhen(false)] out T value) where T : IEu5Object
   {
      var lexeme = node.GetLexeme(source);
      if (!globals.TryGetValue(lexeme, out value))
      {
         ctx.SetPosition(node);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.UnknownKey,
                                        $"{actionStack}.TryGetFromGlobals",
                                        node.GetLexeme(source),
                                        typeof(T).Name);
         validationResult = false;
         return false;
      }

      return true;
   }

   public static bool TryAddToGlobals<T>(LocationContext ctx,
                                         Token token,
                                         string key,
                                         string actionStack,
                                         ref bool validationResult,
                                         T value) where T : IEu5Object
   {
      return TryAddToGlobals(ctx,
                             token,
                             key,
                             actionStack,
                             ref validationResult,
                             value,
                             (Dictionary<string, T>)value.GetGlobalItemsNonGeneric());
   }

   public static bool TryAddToGlobals<T>(LocationContext ctx,
                                         Token token,
                                         string key,
                                         string actionStack,
                                         ref bool validationResult,
                                         T value,
                                         Dictionary<string, T> globals) where T : IEu5Object
   {
      if (globals.TryAdd(key, value))
         return true;

      ctx.SetPosition(token);
      DiagnosticException.LogWarning(ctx.GetInstance(),
                                     ParsingError.Instance.DuplicateObjectDefinition,
                                     $"{actionStack}.TryAddToGlobals",
                                     key,
                                     typeof(T).Name,
                                     "UniqueId");
      validationResult = false;
      return false;
   }
}