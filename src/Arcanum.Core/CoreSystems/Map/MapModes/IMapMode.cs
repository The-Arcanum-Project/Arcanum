using Arcanum.Core.GameObjects.LocationCollections;
using Vortice.Mathematics;

namespace Arcanum.Core.CoreSystems.Map.MapModes;

public interface IMapMode
{
   /// <summary>
   /// An internal name for this map mode.
   /// This should be unique among all map modes. <br/>
   /// This is not necessarily user-facing, but it should be descriptive enough to identify the mapmode
   /// </summary>
   public string Name { get; }

   /// <summary>
   /// A brief description of what this map mode does.
   /// </summary>
   public string Description { get; }

   /// <summary>
   /// Points to the icon resource for this map mode.
   /// This should be a path that the UI can use to load the icon. <br/>
   /// The optimal size for the icon is 20x20 pixels.
   /// </summary>
   public string? IconSource => null;

   /// <summary>
   /// If data is only being shown for land locations.
   /// </summary>
   public bool IsLandOnly => true;

   /// <summary>
   /// Whether this map mode has a legend to display.
   /// </summary>
   public bool HasLegend => true;

   /// <summary>
   /// The enum type of this map mode.
   /// </summary>
   public MapModeManager.MapModeType Type { get; }

   /// <summary>
   /// The data type being displayed by this map mode. <br/>
   /// !!THE FIRST ENTRY IS THE MAIN ENTRY WHICH IS USED FOR INFERING!!
   /// </summary>
   public Type[] DisplayTypes { get; }

   /// <summary>
   /// Renders the map mode
   /// </summary>
   /// <returns></returns>
   public void Render(Color4[] colorBuffer);

   /// <summary>
   /// Returns a tooltip string for the given location on the map. <br/>
   /// Tooltips follow the following format: 
   /// <code>Location Name (Localisation)
   /// Additional Info[0]
   /// Additional Info[1]
   /// ...</code>
   /// </summary>
   /// <returns>An Array of lines for the tooltip</returns>
   public string[] GetTooltip(Location location);

   /// <summary>
   /// Returns a text string for the given location on the map. 
   /// <code>E.g. "(Population: )1000"</code>
   /// </summary>
   public string? GetLocationText(Location location);

   /// <summary>
   /// Returns an array of visual objects for the given location on the map. <br/>
   /// These objects will be rendered on top of the location. <br/>
   /// The array can contain different types of objects, e.g. strings, icons, etc
   /// </summary>
   /// <param name="location"></param>
   /// <returns></returns>
   public object?[]? GetVisualObject(Location location);

   // ################# Methods ##################

   /// <summary>
   /// Is called when this map mode is activated.
   /// </summary>
   public void OnActivateMode();

   /// <summary>
   /// Is called when this map mode is deactivated.
   /// </summary>
   public void OnDeactivateMode();
}