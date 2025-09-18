using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;

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
}