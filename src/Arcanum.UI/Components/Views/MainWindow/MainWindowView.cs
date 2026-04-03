#region

using Arcanum.UI.AppFeatures;
using CommunityToolkit.Mvvm.ComponentModel;

#endregion

namespace Arcanum.UI.Components.Views.MainWindow;

public class MainWindowView : ObservableObject, IAppFeatureProvider
{
   public FeatureId FeatureId => FeatureIds.Editor.MainWindow;
}