#region

using Arcanum.Core.GlobalStates;
using Arcanum.UI.Documentation;
using Common.UI.State;

#endregion

namespace Arcanum.UI.Components.UIHandles;

public class StateHandleImpl : IStateHandle
{
   public void ApplicationLoadResources()
   {
#if DEBUG
      DocuPathResolver.LoadDocumentation(DebugConfig.Settings.UseExternalDocumentation,
                                         true,
                                         DebugConfig.Settings.ExternalDocumentationPath);
#endif
   }
}