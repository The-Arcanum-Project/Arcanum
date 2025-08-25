namespace Arcanum.Core.CoreSystems.NUI;

public class NUINavigation(INUI target, string toolStripString) : INUINavigation
{
   public INUI Target { get; } = target;
   public string ToolStripString { get; } = toolStripString;
}