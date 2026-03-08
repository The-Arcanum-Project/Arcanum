using Arcanum.Core.ApplicationContext;

namespace Arcanum.UI.AppFeatures.Contexts.SpecializedEditors;

public interface IPoliticalEditor : IAppContext
{
   public void ToggleSyncState();
}