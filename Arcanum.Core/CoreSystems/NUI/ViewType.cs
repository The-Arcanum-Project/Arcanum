namespace Arcanum.Core.CoreSystems.NUI;

/// <summary>
/// Defines the type of view that is being used in the NUI (<c>Navigatable User Interface</c>).
/// </summary>
public enum ViewType
{
   /// <summary>
   /// The view of an entire object. Should contain subViews of <see cref="Embedded"/> from other objects
   /// </summary>
   View,
   /// <summary>
   /// A compact view only showing context information about the object.
   /// </summary>
   Embedded,
   /// <summary>
   /// A short info view that displays only the most important information about an object.
   /// </summary>
   ShortInfoView,
}