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
      DocuPathResolver.LoadDocumentation(
#if DEBUG
                                         DebugConfig.Settings.UseExternalDocumentation,
#else
                                         false
#endif
                                         true,
#if DEBUG
                                         DebugConfig.Settings.ExternalDocumentationPath
#else
                                         null
#endif
                                        );
   }
}