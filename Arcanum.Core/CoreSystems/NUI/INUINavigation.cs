using System.Windows.Controls;

namespace Arcanum.Core.CoreSystems.NUI;

public interface INUINavigation
{
   public INUI Target { get; }
   public string ToolStripString { get; }
}