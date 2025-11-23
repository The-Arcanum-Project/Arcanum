using System.Windows;
using System.Windows.Controls;

namespace Arcanum.UI.SpecializedEditors.Management;

/// <summary>
/// Represents a specialized editor for a specific object type or property type in the NUI.
/// </summary>
public interface ISpecializedEditor
{
   /// <summary>
   /// The display name of the editor tab.
   /// </summary>
   public string DisplayName { get; }

   /// <summary>
   /// The resource path to the icon representing the editor. <br/>
   /// <c>null</c> if no icon is set. 
   /// </summary>
   public string? IconResource { get; }

   /// <summary>
   /// A priority value determining the order of the editor tabs. <br/>
   /// Higher values indicate higher priority, meaning the tab will be displayed more to the left.
   /// </summary>
   public int Priority { get; }

   /// <summary>
   /// Whether the editor supports editing multiple target objects simultaneously.
   /// </summary>
   public bool SupportsMultipleTargets { get; }

   /// <summary>
   /// Some precondition check to determine if this editor can be used in the current state or if e.g. errors have
   /// to be resolved first.
   /// </summary>
   public bool CanEdit(object[] targets, Enum? prop);

   /// <summary>
   /// Resets the editor to its initial state.
   /// This is called when the target object changes.
   /// </summary>
   public void Reset();

   /// <summary>
   /// Resets the editor for a new target object.
   /// </summary>
   public void ResetFor(object[] targets);

   /// <summary>
   /// Returns the TabItem control representing the editor UI.
   /// </summary>
   public FrameworkElement GetEditorControl();

   /// <summary>
   /// Custom context menu actions which will use available in NUI when right-clicking on the object being edited. <br/>
   /// This can be just some shortcuts or presets for the specialized editor. 
   /// </summary>
   public IEnumerable<MenuItem> GetContextMenuActions();
}