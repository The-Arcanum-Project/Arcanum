#region

using Arcanum.UI.AppFeatures;
using Arcanum.UI.AppFeatures.FeatureInitializers;
using CommunityToolkit.Mvvm.ComponentModel;

#endregion

namespace Arcanum.UI.Components.Views.MainWindow;

public class MainWindowView : ObservableObject, IAppFeatureProvider
{
   public AppFeature FeatureMetadata { get; } = EditorFeatures.MainWindowFeature;

   public EasterEgg2026 EasterEgg2026 { get; } = new();
}