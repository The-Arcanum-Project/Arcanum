namespace Arcanum.Core.CoreSystems.Selection;

/// <summary>
/// The higher the value, the higher the priority. <br/>
/// Used to determine which border to show when multiple borders are active at the same time. 
/// </summary>
[Flags]
public enum BorderModifier
{
   /// <summary>
   /// For normal selection of locations. <br/>
   /// Shows a bright red border around the province(s)
   /// </summary>
   Selection = 1,

   /// <summary>
   /// The hover state when hovering over the map directly. <br/>
   /// Does make the interior of the province slightly brighter, but does not show any borders or outlines. 
   /// </summary>
   Hover = 2,

   /// <summary>
   /// When hovering over a ui object that relates to a location or smth that can infer locations (e.g. a country). <br/>
   /// Shows a yellow border around the province(s)
   /// </summary>
   Highlight = 4,
}