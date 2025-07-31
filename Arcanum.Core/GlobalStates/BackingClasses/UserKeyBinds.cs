using Arcanum.API.Attributes;
using Arcanum.Core.GlobalStates.BackingClasses.WindowKeyBinds;

namespace Arcanum.Core.GlobalStates.BackingClasses;

public class UserKeyBinds
{
   [SettingsForceInlinePropertyGrid]
   public MainWindowKeyBinds MainWindowKeyBinds { get; set; } = new();
}