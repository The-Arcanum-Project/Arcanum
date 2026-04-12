using System.Windows.Input;
using Arcanum.Core.ApplicationContext;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.UI.AppFeatures;
using Arcanum.UI.AppFeatures.FeatureInitializers;
using Arcanum.UI.Commands;

namespace Arcanum.UI.Components.UserControls.Map;

public class MapViewModel : IAppFeatureProvider, IAppContext
{
   public AppFeature FeatureMetadata { get; } = EditorFeatures.MapFeature;
}