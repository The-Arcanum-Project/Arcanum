using System.Runtime.CompilerServices;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.GameObjects.LocationCollections;
using Vortice.Mathematics;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class PoliticalMapMode : IMapMode
{
   public string Name => "Political";
   public string Description => "Displays the owner of each location.";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.Political;
   public Type[] DisplayTypes { get; } = [typeof(Country)];

   public void Render(Color4[] colorBuffer)
   {
      var unassignedMarker = new JominiColor.Rgb(32, 32, 32).ToColor4();
      Array.Fill(colorBuffer, unassignedMarker);

      Parallel.ForEach(Globals.Countries.Values,
                       country =>
                       {
                          var color = country.Definition.Color.ToColor4();

                          PaintCollections(colorBuffer,
                                           color,
                                           country.OwnControlCores,
                                           country.OwnColony,
                                           country.OwnConquered,
                                           country.OwnCores,
                                           country.OwnIntegrated,
                                           country.OwnControlConquered,
                                           country.OwnControlColony,
                                           country.OwnControlIntegrated);
                       });

      if (!Config.Settings.MapSettings.UseShadeOfColorOnWater)
         return;

      foreach (var location in Globals.DefaultMapDefinition.SeaZones)
         colorBuffer[location.ColorIndex] = MapModeManager.GetWaterColorForLocation(location);

      foreach (var location in Globals.DefaultMapDefinition.Lakes)
         colorBuffer[location.ColorIndex] = MapModeManager.GetWaterColorForLocation(location);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static void PaintCollections(Color4[] buffer, Color4 color, params IEnumerable<Location>[] locationLists)
   {
      for (var i = 0; i < locationLists.Length; i++)
         foreach (var loc in locationLists[i])
            if (loc.ColorIndex < buffer.Length)
               buffer[loc.ColorIndex] = color;
   }

   public string[] GetTooltip(Location location)
   {
      return [$"{location.UniqueId}: Not implemented"];
   }

   public string? GetLocationText(Location location) => null;

   public object?[]? GetVisualObject(Location location) => null;

   public void OnActivateMode()
   {
   }

   public void OnDeactivateMode()
   {
   }
}