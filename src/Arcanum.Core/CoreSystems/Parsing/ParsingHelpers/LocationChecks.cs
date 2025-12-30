using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;

public static class LocationChecks
{
   public static bool IsValidLocation(LocationContext context, string? str, out Location? location)
   {
      if (string.IsNullOrWhiteSpace(str) || !Globals.Locations.TryGetValue(str, out location))
      {
         DiagnosticException.LogWarning(context.GetInstance(),
                                        ParsingError.Instance.InvalidLocationKey,
                                        nameof(IsValidLocation).GetType().FullName!,
                                        str ?? "null");
         location = Location.Empty;
         return false;
      }

      return true;
   }
}