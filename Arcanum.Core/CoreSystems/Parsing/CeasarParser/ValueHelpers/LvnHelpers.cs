using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;

public static class LvnHelpers
{
   public static bool IsLiteralValueNode(this ValueNode node, LocationContext ctx, string actionName, out LiteralValueNode? value)
   {
      if (node is not LiteralValueNode lvn)
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        actionName,
                                        node.GetType().Name,
                                        nameof(LiteralValueNode));
         value = null!;
         return false;
      }
      value = lvn;
      return true;
   }
   
   public static bool TryParseLocationFromLvn(this LiteralValueNode lvn,
                                           LocationContext ctx,
                                           string actionName,
                                           string source,
                                           out Location location)
   {
      var lexeme = lvn.Value.GetLexeme(source);
      if (!Globals.Locations.TryGetValue(lexeme, out location!))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidLocationKey,
                                        actionName,
                                        lexeme);
         location = Location.Empty;
         return false;
      }

      return true;
   }
}