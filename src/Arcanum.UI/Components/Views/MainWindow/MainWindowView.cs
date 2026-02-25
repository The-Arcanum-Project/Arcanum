using Arcanum.UI.AppFeatures;
using Arcanum.UI.AppFeatures.FeatureInitializers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arcanum.UI.Components.Views.MainWindow;

public class MainWindowView : ObservableObject, IAppFeatureProvider
{
   public AppFeature FeatureMetadata { get; } = EditorFeatures.MainWindowFeature;
}