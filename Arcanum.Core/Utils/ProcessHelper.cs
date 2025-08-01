using System.Diagnostics;
using System.IO;
using System.Windows;
using Arcanum.API.UI;
using Arcanum.Core.GlobalStates;
using Microsoft.Win32;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Arcanum.Core.Utils;

public static class ProcessHelper
{
   // open the file in notepad++ at the given line
   public static bool OpenNotePadPlusPlusAtLineOfFile(string path, int line)
   {
      try
      {
         // see if notepad++ is installed on user's machine
         var nppDir = (string?)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Notepad++", null, null);
         if (nppDir != null)
         {
            var nppExePath = Path.Combine(nppDir, "Notepad++.exe");
            Process.Start(nppExePath, $"\"{path}\" -n{line}");
         }
      }
      catch (Exception e)
      {
         AppData.WindowLinker.ShowMBox($"Failed to open file in Notepad++: {e.Message}{Environment.NewLine}Please open the url yourself {path}");
         return false;
      }

      return true;
   }

   public static bool OpenVsCodeAtLineOfFile(string path, int line, int charIndex)
   {
      try
      {
         Process.Start(new ProcessStartInfo
         {
            UseShellExecute = true, FileName = $"vscode://file/{path}:{line}:{charIndex}",
         });
         return true;
      }
      catch (Exception ex)
      {
         MessageBox.Show($"Failed to open file in VS-Code: {ex.Message}{Environment.NewLine}Please open the url yourself {path}");
         return false;
      }
   }

   public static void OpenFileAtLine(string path, int line, int charIndex, PreferredEditor preferredEditor)
   {
      switch (preferredEditor)
      {
         case PreferredEditor.VsCode:
            OpenVsCodeAtLineOfFile(path, line, charIndex);
            break;
         case PreferredEditor.NotepadPlusPlus:
            OpenNotePadPlusPlusAtLineOfFile(path, line);
            break;
         case PreferredEditor.Other:
            OpenFile(path);
            break;
         default:
            throw new ArgumentOutOfRangeException(nameof(preferredEditor), preferredEditor, "Unknown preferred editor");
      }
   }

   public static bool OpenDiscordLinkIfDiscordRunning(string link)
   {
      try
      {
         Process.Start(new ProcessStartInfo
         {
            UseShellExecute = true, FileName = IsDiscordRunning ? "discord:" + link : link,
         });
         return true;
      }
      catch (Exception ex)
      {
         AppData.WindowLinker.ShowMBox($"Failed to open the browser: {ex.Message}{Environment.NewLine}Please open the url yourself {link}");
         return false;
      }
   }

   public static void OpenLink(string link)
   {
      try
      {
         Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = link });
      }
      catch (Exception ex)
      {
         AppData.WindowLinker.ShowMBox($"Failed to open the browser: {ex.Message}{Environment.NewLine}Please open the url yourself {link}");
      }
   }

   public static bool IsDiscordRunning => Process.GetProcessesByName("Discord").Length > 0;

   public static void Open(string app, string args, bool newWindow)
   {
      using var myProcess = new Process();
      myProcess.StartInfo.UseShellExecute = true;
      myProcess.StartInfo.FileName = app;
      myProcess.StartInfo.Arguments = args;
      myProcess.StartInfo.CreateNoWindow = newWindow;
      myProcess.Start();
   }

   public static bool OpenFolder(string path)
   {
      if (Directory.Exists(path))
      {
         Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
         return true;
      }

      AppData.WindowLinker.ShowMBox($"The path {path} can not be opened",
                                    "Folder can not be opened",
                                    MBoxButton.OK,
                                    MessageBoxImage.Warning);
      return false;
   }

   public static bool OpenFile(string path)
   {
      if (File.Exists(path))
      {
         Process.Start(new ProcessStartInfo
         {
            FileName = path, UseShellExecute = true,
         });
         return true;
      }

      AppData.WindowLinker.ShowMBox($"The path {path} can not be opened",
                                    "File can not be opened",
                                    MBoxButton.OK,
                                    MessageBoxImage.Warning);
      return false;
   }

   public static bool OpenPathIfFileOrFolder(string path)
   {
      if (OpenFile(path))
         return true;
      if (OpenFolder(path))
         return true;

      return false;
   }
}

public enum PreferredEditor
{
   VsCode,
   NotepadPlusPlus,
   Other,
}