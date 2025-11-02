using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.NUI;

/// <summary>
/// Defines a contract for the NUI to navigate to another NUI.
/// </summary>
public interface INUINavigation
{
   /// <summary>
   /// The target NUI to navigate to.
   /// </summary>
   public INUI? Target { get; }
   /// <summary>
   /// The string to display in the tool strip for this navigation option.
   /// </summary>
   public string ToolStripString { get; }
   public bool IsEnabled { get; set; }
}

public interface IEu5Navigation
{
   public IEu5Object? Target { get; }
   public string ContextMenuString { get; }
   public bool IsEnabled { get; set; }
}