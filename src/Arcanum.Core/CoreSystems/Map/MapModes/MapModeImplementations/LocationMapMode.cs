#region

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

#endregion

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public sealed class LocationMapMode : LocationBasedMapMode
{
   private const string FIRST_STR = "Create a land adjacency between the previously set location and the currently selected location.";
   private Vector2 _fristAdjCoords = Vector2.Zero;
   private Location _firstAdjLocation = Location.Empty;

   public override string Name => "Locations";
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.Locations;
   public override Type[] DisplayTypes => [typeof(Location)];
   public override string Description => "The default map mode.";
   public string? IconSource => null;

   public override int GetColorForLocation(Location location)
   {
      return location.Color.AsInt();
   }

   public override bool IsLandOnly => false;

   public override string[] GetTooltip(Location location) => [$"Color: {location.Color}"];

   public override string? GetLocationText(Location location) => null;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }

   public override object GetLocationRelatedData(Location location) => location;

   public override MapContexMenuConfig[] GetContextMenuOptions() =>
   [
      new()
      {
         IsEnabled = HasValidSelectionForContextMenu(out _, out _),
         OptionName = "Copy ports.csv Data or Cursor Position",
         Tooltip = _ => "Copy valid port data of the selected sea and land location to the clipboard in the format: LandLocationId;SeaLocationId;X;Y;x",
         OptionAction = ContextCopyAction,
      },
      new()
      {
         IsEnabled = true,
         OptionName = "Set adjacency location 1",
         Tooltip = GetAdjToolTip,
         OptionAction = SetFirstAdj,
      },
      new()
      {
         IsEnabled = IsAdjOneSetAndSelectionFor2nd(),
         OptionName = "Create land adjacency",
         Tooltip = GetLandAdjToolTip,
         OptionAction = position => System.Windows.Clipboard.SetText(ExportAdj(true, position)),
      },
      new()
      {
         IsEnabled = IsAdjOneSetAndSelectionFor2nd(),
         OptionName = "Create sea adjacency",
         Tooltip = GetSeaAdjToolTip,
         OptionAction = position => System.Windows.Clipboard.SetText(ExportAdj(false, position)),
      },
   ];

   private bool IsAdjOneSetAndSelectionFor2nd()
   {
      if (!IsAdjOneSet())
         return false;

      return Selection.Selection.GetSelectedLocations.Count == 1 && Selection.Selection.GetSelectedLocations[0] != _firstAdjLocation;
   }

   private string ExportAdj(bool isLand, Vector2 position2)
   {
      var location2 = Selection.Selection.GetSelectedLocations[0];
      var typeStr = isLand ? "land" : "sea";
      return
         $"{_firstAdjLocation.UniqueId};{location2.UniqueId};{typeStr};{_firstAdjLocation.UniqueId}-{location2.UniqueId};{_fristAdjCoords.X:#};{_fristAdjCoords.Y:#};{position2.X:#};{position2.Y:#};xxx";
   }

   private string GetLandAdjToolTip(Vector2 position2) => $"{FIRST_STR}\nClipboard preview of the adjacency data to be added:\n{ExportAdj(true, position2)}";

   private string GetSeaAdjToolTip(Vector2 position2) => $"{FIRST_STR}\nClipboard preview of the adjacency data to be added:\n{ExportAdj(false, position2)}";

   private string GetAdjToolTip(Vector2 _)
   {
      const string firstStr = "Set adjacency location 1 for the selected location and clicked position.";
      if (IsAdjOneSet())
         return $"{firstStr}\nCurrently set: Location {_firstAdjLocation.UniqueId} at coordinates {_fristAdjCoords}";

      return firstStr;
   }

   private void SetFirstAdj(Vector2 position)
   {
      if (Selection.Selection.GetSelectedLocations.Count != 1)
         return;

      _firstAdjLocation = Selection.Selection.GetSelectedLocations[0];
      _fristAdjCoords = position;
   }

   private bool IsAdjOneSet() => _firstAdjLocation != Location.Empty;

   private static bool HasValidSelectionForContextMenu([MaybeNullWhen(false)] out Location land1, [MaybeNullWhen(false)] out Location land2)
   {
      land1 = null;
      land2 = null;
      var locs = Selection.Selection.GetSelectedLocations;

      if (locs.Count != 2)
         return false;

      var dmd = Globals.DefaultMapDefinition;
      if (!dmd.IsLand(locs[0]) || !dmd.IsLand(locs[1]))
         return false;

      land1 = locs[0];
      land2 = locs[1];
      return true;
   }

   private static void ContextCopyAction(Vector2 position)
   {
      if (!HasValidSelectionForContextMenu(out var land1, out var land2))
         return;

      // Set text to clipboard
      System.Windows.Clipboard.SetText($"{land2.UniqueId};{land1.UniqueId};{position.X:#};{position.Y:#};x");
   }
}