using Arcanum.UI.SpecializedEditors.EditorControls.ViewModels;

namespace Arcanum.UI.SpecializedEditors.EditorControls;

public partial class InstitutionEditor
{
   private static readonly Lazy<InstitutionEditor> LazyInstance = new(() => new());
   public static InstitutionEditor Instance => LazyInstance.Value;
   public readonly InstitutionViewModel ViewModel = new();

   public InstitutionEditor()
   {
      DataContext = ViewModel;
      InitializeComponent();
   }
}