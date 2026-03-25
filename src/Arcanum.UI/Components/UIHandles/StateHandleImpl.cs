using Arcanum.Core.GlobalStates;
using Arcanum.UI.Documentation;
using Common.UI.State;

namespace Arcanum.UI.Components.UIHandles;

public class StateHandleImpl : IStateHandle
{
   public void ApplicationLoadResources()
   {
      DocuPathResolver.LoadDocumentation(DebugConfig.Settings.UseExternalDocumentation,
                                         true,
                                         DebugConfig.Settings.ExternalDocumentationPath);
   }
}