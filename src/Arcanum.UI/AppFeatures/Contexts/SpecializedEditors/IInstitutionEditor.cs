using Arcanum.Core.ApplicationContext;
using Arcanum.UI.AppFeatures.FeatureInitializers;

namespace Arcanum.UI.AppFeatures.Contexts.SpecializedEditors;

public class IInstitutionEditor : IAppContext, IAppFeatureProvider
{
   public AppFeature FeatureMetadata => SpecializedEditorFeatures.InstitutionEditor;
}