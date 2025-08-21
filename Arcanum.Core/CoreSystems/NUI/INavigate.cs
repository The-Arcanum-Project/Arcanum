namespace Arcanum.Core.CoreSystems.NUI;

/// <summary>
/// This interface defines a navigation system for NUI which will be displayed as contextual navigation options.
/// </summary>
public interface INavigate
{
   /// <summary>
   /// The target control that this navigation system will navigate to.
   /// </summary>
   /// <returns></returns>
   public UserControl GetTargetControl();

   /// <summary>
   /// The string that will be displayed in a navigation tool strip.
   /// </summary>
   public string ToolStripString { get; }

   /// <summary>
   /// The command that will be executed when this navigation is triggered.
   /// </summary>
   public NavigationCommand Command { get; }
}