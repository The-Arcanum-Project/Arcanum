using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;

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
         location = (Location)Location.Empty;
         return false;
      }

      return true;
   }
}