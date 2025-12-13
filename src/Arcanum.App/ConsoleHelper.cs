using System.Runtime.InteropServices;

namespace Arcanum.App;

internal static class ConsoleHelper
{
   [DllImport("kernel32.dll")]
   private static extern bool AttachConsole(int dwProcessId);

   [DllImport("kernel32.dll")]
   private static extern bool AllocConsole();

   [DllImport("kernel32.dll")]
   private static extern bool FreeConsole();

   private const int ATTACH_PARENT_PROCESS = -1;

   public static void InitConsole()
   {
      // Try to attach to the CMD/PowerShell that launched us
      if (!AttachConsole(ATTACH_PARENT_PROCESS))
      {
         // If launched from Explorer, create a new window
         AllocConsole();
      }
   }

   public static void ReleaseConsole()
   {
      FreeConsole();
   }
}