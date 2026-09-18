#region

using Arcanum.UI.AppFeatures;
using Arcanum.UI.Documentation.Implementation;

#endregion

namespace Arcanum.UI.Components.Windows.HelpWindow.ViewModels;

public class MainDocumentationViewModel : HelpPageViewModelBase
{
   public override string Title => "Documentation";
   public FeatureDoc? MainDocuPage { get; } = DocuRegistry.GetPage(FeatureIds.Documentation.Main);
}