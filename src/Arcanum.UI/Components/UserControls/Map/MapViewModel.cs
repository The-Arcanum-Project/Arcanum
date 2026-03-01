using Arcanum.UI.AppFeatures;
using Arcanum.UI.AppFeatures.FeatureInitializers;

namespace Arcanum.UI.Components.UserControls.Map;

public class MapViewModel : IAppFeatureProvider
{
   public AppFeature FeatureMetadata { get; } = EditorFeatures.MapFeature;
}