namespace Arcanum.Core.CoreSystems.NUI;

public class NUINavigation : INUINavigation
{
   public NUINavigation(INUI target, string toolStripString)
   {
      Target = target;
      ToolStripString = toolStripString;
   }

   public INUI Target { get; }
   public string ToolStripString { get; }
}