using ModValInstance = Arcanum.Core.CoreSystems.Jomini.Modifiers.ModValInstance;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class ModValViewer
{
   public ModValInstance ViewModel { get; set; }

   public ModValViewer(ModValInstance instance)
   {
      ViewModel = instance;
      InitializeComponent();
   }
}