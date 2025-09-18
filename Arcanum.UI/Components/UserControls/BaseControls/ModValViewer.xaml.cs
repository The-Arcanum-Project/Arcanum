using Arcanum.Core.CoreSystems.Jomini.ModifierSystem;

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