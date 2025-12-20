namespace Arcanum.Core.CoreSystems.ConsoleServices;

public static partial class DefaultCommands
{
   [Flags]
   public enum CommandCategory
   {
      None = 0, // No commands
      Basic = 1 << 0, // echo, help, clear, list...
      Alias = 1 << 1, // alias command...
      Macro = 1 << 2, // macro command...
      History = 1 << 3, // history command...
      FileSystem = 1 << 4, // pwd...
      Debug = 1 << 6, // set clearance (and other debug-specific commands)

      // Common combinations
      StandardUser = Basic | Alias | Macro | History | FileSystem,

      All = ~None, // Special value to include all defined categories (bitwise NOT of None)
   }
}