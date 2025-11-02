using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class TkHelper
{
   public static bool TryGetLocationFromToken(this Token token,
                                              LocationContext ctx,
                                              string source,
                                              string actionStack,
                                              ref bool validation,
                                              [MaybeNullWhen(false)] out Location location)
   {
      location = null;
      if (!Globals.Locations.TryGetValue(token.GetLexeme(source), out location))
      {
         ctx.SetPosition(token);
         De.Warning(ctx,
                    ParsingError.Instance.InvalidLocationKey,
                    actionStack,
                    token.GetLexeme(source));
         validation = false;
         return false;
      }

      return true;
   }
}