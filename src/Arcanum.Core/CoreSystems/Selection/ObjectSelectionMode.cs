namespace Arcanum.Core.CoreSystems.Selection;

/// <summary>
/// Describes how we are currently selecting objects
/// </summary>
public enum ObjectSelectionMode
{
   /// <summary>
   /// Locations are being directly selected
   /// </summary>
   LocationSelection,

   /// <summary>
   /// Depending on the map mode we are inferring what is being selected
   /// </summary>
   InferSelection,

   /// <summary>
   /// Objects are only added from searching
   /// </summary>
   FromSearch,
}