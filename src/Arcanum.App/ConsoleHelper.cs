#region

using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

#endregion

namespace Arcanum.App;

internal static class ConsoleHelper
{
   private const int ATTACH_PARENT_PROCESS = -1;
   private const int STD_OUTPUT_HANDLE = -11;
   private const int STD_ERROR_HANDLE = -12;

   private const uint GENERIC_READ = 0x80000000;
   private const uint GENERIC_WRITE = 0x40000000;
   private const uint FILE_SHARE_READ = 1;
   private const uint FILE_SHARE_WRITE = 2;
   private const uint OPEN_EXISTING = 3;

   // --- P/Invokes ---
   [DllImport("kernel32.dll")]
   private static extern bool AttachConsole(int dwProcessId);

   [DllImport("kernel32.dll")]
   private static extern bool AllocConsole();

   [DllImport("kernel32.dll")]
   private static extern bool FreeConsole();

   [DllImport("kernel32.dll")]
   private static extern IntPtr GetStdHandle(int nStdHandle);

   [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
   private static extern IntPtr CreateFile(
      string lpFileName,
      uint dwDesiredAccess,
      uint dwShareMode,
      IntPtr lpSecurityAttributes,
      uint dwCreationDisposition,
      uint dwFlagsAndAttributes,
      IntPtr hTemplateFile);

   [DllImport("kernel32.dll", SetLastError = true)]
   private static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput, out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

   [DllImport("kernel32.dll", SetLastError = true)]
   private static extern bool FillConsoleOutputCharacter(IntPtr hConsoleOutput,
                                                         char cCharacter,
                                                         uint nLength,
                                                         COORD dwWriteCoord,
                                                         out uint lpNumberOfCharsWritten);

   [DllImport("kernel32.dll", SetLastError = true)]
   private static extern bool SetConsoleCursorPosition(IntPtr hConsoleOutput, COORD dwCursorPosition);

   public static void InitConsole()
   {
      if (!AttachConsole(ATTACH_PARENT_PROCESS))
         AllocConsole();

      SyncConsole();
   }

   public static void SafeClear()
   {
      var hConsole = CreateFile("CONOUT$", GENERIC_READ | GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

      if (hConsole == new IntPtr(-1))
         return;

      try
      {
         if (GetConsoleScreenBufferInfo(hConsole, out var bf))
         {
            // Calculate the number of cells in the current buffer
            var nChars = (uint)(bf.dwSize.X * bf.dwSize.Y);
            var origin = new COORD { X = 0, Y = 0 };

            // Fill with spaces
            if (FillConsoleOutputCharacter(hConsole, ' ', nChars, origin, out _))
               // Reset cursor
               SetConsoleCursorPosition(hConsole, origin);
         }
         else
            // Fallback for modern terminals: Send the ANSI VT clear sequence
            // This works in Windows Terminal, VS Code, and Win10+ CMD
            Console.Write("\e[2J\e[H");
      }
      finally
      {
         new SafeFileHandle(hConsole, true).Dispose();
      }
   }

   private static void SyncConsole()
   {
      var stdOutPtr = GetStdHandle(STD_OUTPUT_HANDLE);
      var safeOutHandle = new SafeFileHandle(stdOutPtr, false);
      var fsOut = new FileStream(safeOutHandle, FileAccess.Write);
      var swOut = new StreamWriter(fsOut) { AutoFlush = true };
      Console.SetOut(swOut);

      var stdErrPtr = GetStdHandle(STD_ERROR_HANDLE);
      var safeErrHandle = new SafeFileHandle(stdErrPtr, false);
      var fsErr = new FileStream(safeErrHandle, FileAccess.Write);
      var swErr = new StreamWriter(fsErr) { AutoFlush = true };
      Console.SetError(swErr);
   }

   public static void ReleaseConsole() => FreeConsole();

   [StructLayout(LayoutKind.Sequential)]
   private struct COORD
   {
      public short X;
      public short Y;
   }

   [StructLayout(LayoutKind.Sequential)]
   private struct SMALL_RECT
   {
      public short Left;
      public short Top;
      public short Right;
      public short Bottom;
   }

   [StructLayout(LayoutKind.Sequential)]
   private struct CONSOLE_SCREEN_BUFFER_INFO
   {
      public COORD dwSize;
      public COORD dwCursorPosition;
      public ushort wAttributes;
      public SMALL_RECT srWindow;
      public COORD dwMaximumWindowSize;
   }
}