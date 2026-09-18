#region

using Arcanum.UI.AppFeatures;
using CommunityToolkit.Mvvm.ComponentModel;

#endregion

namespace Arcanum.UI.Components.Views.MainWindow;

public class MainWindowView : ObservableObject, IAppFeatureProvider
{
   public FeatureId FeatureId => FeatureIds.Editor.MainWindow;

   public EasterEgg2026 EasterEgg2026 { get; } = new();
}