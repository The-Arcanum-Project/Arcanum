#region

using Arcanum.Core.ApplicationContext;

#endregion

namespace Arcanum.UI.AppFeatures.Contexts.SpecializedEditors;

public class IInstitutionEditor : IAppContext, IAppFeatureProvider
{
   public FeatureId FeatureId => FeatureIds.Editor.SpecializedEditors.InstitutionEditor;
}