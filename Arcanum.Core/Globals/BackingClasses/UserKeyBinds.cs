using Arcanum.Core.Globals.BackingClasses.WindowKeyBinds;

namespace Arcanum.Core.Globals.BackingClasses;

public class UserKeyBinds
{
   // Only used for serialization purposes.
   public UserKeyBinds()
   {
   }

   public MainWindowKeyBinds MainWindowKeyBinds { get; set; } = new();
}