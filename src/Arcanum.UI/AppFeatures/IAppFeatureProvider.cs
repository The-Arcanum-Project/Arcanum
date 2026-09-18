#region

using Arcanum.UI.Documentation.Implementation;

#endregion

namespace Arcanum.UI.AppFeatures;

public interface IAppFeatureProvider
{
   public FeatureId FeatureId { get; }
   public FeatureDoc? Feature => DocuRegistry.GetPage(FeatureId);
}