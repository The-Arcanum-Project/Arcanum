using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class TkHelper
{
   public static bool TryGetLocationFromToken(this Token token,
                                              ref ParsingContext pc,
                                              [MaybeNullWhen(false)] out Location location)
   {
      using var scope = pc.PushScope();
      location = null;
      if (!Globals.Locations.TryGetValue(pc.SliceString(token), out location))
      {
         pc.SetContext(token);
         De.Warning(ref pc,
                    ParsingError.Instance.InvalidLocationKey,
                    pc.SliceString(token));
         return pc.Fail();
      }

      return true;
   }
}