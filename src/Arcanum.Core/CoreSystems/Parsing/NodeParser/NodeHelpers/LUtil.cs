using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class LUtil
{
   public static List<Location> LocationsFromStatementNodes(List<StatementNode> nodes,
                                                            ref ParsingContext pc)
   {
      using var scope = pc.PushScope();
      var locations = new List<Location>(nodes.Count);

      foreach (var node in nodes)
      {
         if (node is not KeyOnlyNode kNode)
            continue;

         if (!ParseLocation(kNode, ref pc, out var location))
            continue;

         locations.Add(location);
      }

      return locations;
   }

   public static bool ParseLocation(KeyOnlyNode kNode,
                                    ref ParsingContext pc,
                                    out Location location)
   {
      using var scope = pc.PushScope();
      var key = pc.SliceString(kNode);
      if (!Globals.Locations.TryGetValue(key, out location!))
      {
         pc.SetContext(kNode);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidLocationKey,
                                        key);
         location = Location.Empty;
         return false;
      }

      return true;
   }

   public static bool ValidateNodeSeparatorAndNumberValue(ContentNode node,
                                                          ref ParsingContext pc,
                                                          [MaybeNullWhen(false)] out string value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator, TokenType.Equals, ref pc))
      {
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNodeOptionalUnary(ref pc,
                                                      out value,
                                                      out _))
         return false;

      return true;
   }

   public static bool TryGetFromGlobalsAndLog<T>(
      Token token,
      ref ParsingContext pc,
      Dictionary<string, T> globals,
      [MaybeNullWhen(false)] out T value) where T : IEu5Object
   {
      using var scope = pc.PushScope();
      var lexeme = pc.SliceString(token);
      if (!globals.TryGetValue(lexeme, out value))
      {
         pc.SetContext(token);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.UnknownKey,
                                        pc.SliceString(token),
                                        typeof(T).Name);
         return pc.Fail();
      }

      return true;
   }

   public static bool TryGetFromGlobalsAndLog<T>(
      ContentNode node,
      ref ParsingContext pc,
      Dictionary<string, T> globals,
      [MaybeNullWhen(false)] out T value) where T : IEu5Object
   {
      using var scope = pc.PushScope();
      if (SeparatorHelper.IsSeparatorOfType(node.Separator,
                                            TokenType.Equals,
                                            ref pc) &&
          node.Value.IsLiteralValueNode(ref pc, out var lvn))
         return TryGetFromGlobalsAndLog(lvn.Value,
                                        ref pc,
                                        globals,
                                        out value);

      value = default;
      return pc.Fail();
   }

   public static bool TryGetFromGlobalsAndLog<T>(
      KeyNodeBase node,
      ref ParsingContext pc,
      Dictionary<string, T> globals,
      [MaybeNullWhen(false)] out T value) where T : IEu5Object
   {
      using var scope = pc.PushScope();
      var lexeme = pc.SliceString(node);
      if (!globals.TryGetValue(lexeme, out value))
      {
         pc.SetContext(node);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.UnknownKey,
                                        lexeme,
                                        typeof(T).Name);
         return pc.Fail();
      }

      return true;
   }

   public static bool TryAddToGlobals<T>(Token token,
                                         ref ParsingContext pc,
                                         T value) where T : IEu5Object
   {
      return TryAddToGlobals(token,
                             ref pc,
                             value,
                             (Dictionary<string, T>)value.GetGlobalItemsNonGeneric());
   }

   public static bool TryAddToGlobals<T>(Token token,
                                         ref ParsingContext pc,
                                         T value,
                                         Dictionary<string, T> globals) where T : IEu5Object
   {
      using var scope = pc.PushScope();
      var key = pc.SliceString(token);
      if (globals.TryAdd(key, value))
         return true;

      pc.SetContext(token);
      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.DuplicateObjectDefinition,
                                     key,
                                     typeof(T).Name,
                                     "UniqueId");
      return pc.Fail();
   }

   public static bool TryAddToGlobals<T>(string kue,
                                         ref ParsingContext pc,
                                         T value,
                                         Dictionary<string, T> globals) where T : IEu5Object
   {
      using var scope = pc.PushScope();
      if (globals.TryAdd(kue, value))
         return true;

      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.DuplicateObjectDefinition,
                                     kue,
                                     typeof(T).Name,
                                     "UniqueId");
      return pc.Fail();
   }

   public static int Eu5FileObjFileNameComparer(Eu5FileObj x, Eu5FileObj y) => string.CompareOrdinal(x.Path.Filename, y.Path.Filename);
}