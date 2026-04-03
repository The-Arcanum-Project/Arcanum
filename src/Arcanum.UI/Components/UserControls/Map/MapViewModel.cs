#region

using Arcanum.UI.AppFeatures;

#endregion

namespace Arcanum.UI.Components.UserControls.Map;

public class MapViewModel : IAppFeatureProvider
{
   public FeatureId FeatureId => FeatureIds.Editor.Map;
}