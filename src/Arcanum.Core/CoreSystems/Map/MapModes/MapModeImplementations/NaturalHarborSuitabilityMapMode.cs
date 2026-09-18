#region

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Arcanum.Core.GameObjects.InGame.Map;
using Arcanum.Core.Utils.Colors;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

#endregion

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public sealed class NaturalHarborSuitabilityMapMode : LocationBasedMapMode
{
   public override string Name { get; } = "Natural Harbor Suitability";
   public override string Description { get; } = "Displays the natural harbor suitability of each coastal location on the map.";
   public override MapModeManager.MapModeType Type { get; } = MapModeManager.MapModeType.NaturalHarborSuitability;
   public override Type[] DisplayTypes { get; } = [typeof(LocationTemplateData)];

   public override int GetColorForLocation(Location location)
      => ColorGenerator.GetRedGreenGradientInverse(location.TemplateData.NaturalHarborSuitability).AsAbgrInt();

   public override string[] GetTooltip(Location location) => [$"Natural Harbor Suitability: {location.TemplateData.NaturalHarborSuitability * 100:F2}%"];

   public override string? GetLocationText(Location location) => null;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }

   public override object GetLocationRelatedData(Location location) => location.TemplateData.NaturalHarborSuitability.ToString("####.###");

   public override MapContexMenuConfig[]? GetContextMenuOptions() =>
   [
      new()
      {
         IsEnabled = HasValidSelectionForContextMenu(out _, out _),
         OptionName = "Copy ports.csv Data or Cursor Position",
         Tooltip = _ => "Copy valid port data of the selected sea and land location to the clipboard in the format: LandLocationId;SeaLocationId;X;Y;x",
         OptionAction = ContextCopyAction,
      },
   ];

   private static bool HasValidSelectionForContextMenu([MaybeNullWhen(false)] out Location sea, [MaybeNullWhen(false)] out Location land)
   {
      sea = null;
      land = null;
      var locs = Selection.Selection.GetSelectedLocations;
      if (locs.Count != 2)
         return false;

      var dmd = Globals.DefaultMapDefinition;

      if (dmd.SeaZones.Contains(locs[0]) && dmd.IsLand(locs[1]))
      {
         sea = locs[0];
         land = locs[1];
         return true;
      }

      if (dmd.SeaZones.Contains(locs[1]) && dmd.IsLand(locs[0]))
      {
         sea = locs[1];
         land = locs[0];
         return true;
      }

      return false;
   }

   private static void ContextCopyAction(Vector2 position)
   {
      if (!HasValidSelectionForContextMenu(out var sea, out var land))
         return;

      System.Windows.Clipboard.SetText($"{land.UniqueId};{sea.UniqueId};{position.X:#};{position.Y:#};x");
   }
}