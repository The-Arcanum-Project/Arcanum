using System.Diagnostics;
using System.IO;
using System.Windows;
using Common.UI;
using Common.UI.MBox;
using Microsoft.Win32;

namespace Common;

public static class ProcessHelper
{
   /// <summary>
   /// Opens a file using notepad++ at a specific line. <br/><br/>
   /// If notepad++ is not installed, it will show a message box with the error
   /// </summary>
   /// <param name="path"></param>
   /// <param name="line"></param>
   /// <returns></returns>
   public static bool OpenNotePadPlusPlusAtLineOfFile(string path, int line)
   {
      try
      {
         // see if notepad++ is installed on user's machine
#pragma warning disable CA1416
         var nppDir = (string?)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Notepad++", null, null);
#pragma warning restore CA1416
         if (nppDir != null)
         {
            var nppExePath = Path.Combine(nppDir, "Notepad++.exe");
            Process.Start(nppExePath, $"\"{path}\" -n{line}");
         }
      }
      catch (Exception e)
      {
         UIHandle.Instance.PopUpHandle
                 .ShowMBox($"Failed to open file in Notepad++: {e.Message}{Environment.NewLine}Please open the url yourself {path}");
         return false;
      }

      return true;
   }

   /// <summary>
   /// Opens a file using VS-Code at a specific line and character index. <br/><br/>
   /// If VS-Code is not installed, it will show a message box with the error
   /// </summary>
   /// <param name="path"></param>
   /// <param name="line"></param>
   /// <param name="charIndex"></param>
   /// <returns></returns>
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
         UIHandle.Instance.PopUpHandle
                 .ShowMBox($"Failed to open file in VS-Code: {ex.Message}{Environment.NewLine}Please open the url yourself {path}");
         return false;
      }
   }

   /// <summary>
   /// Opens a file at a specific line and character index in the preferred editor. <br/><br/>
   /// If the editor is not installed, it will show a message box with the error.
   /// </summary>
   /// <param name="path"></param>
   /// <param name="line"></param>
   /// <param name="charIndex"></param>
   /// <param name="preferredEditor"></param>
   /// <exception cref="ArgumentOutOfRangeException"></exception>
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

   /// <summary>
   /// Opens a Discord link if Discord is running, otherwise opens the link in the default browser. <br/><br/>
   /// If opening the link in the default browser fails,
   /// it will show a message box with the error and ask the user to open the link manually.
   /// </summary>
   /// <param name="link"></param>
   /// <returns></returns>
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
         UIHandle.Instance.PopUpHandle
                 .ShowMBox($"Failed to open the browser: {ex.Message}{Environment.NewLine}Please open the url yourself {link}");
         return false;
      }
   }

   /// <summary>
   /// Opens a link in the default browser. <br/><br/>
   /// If opening the link fails, it will show a message box with the error and ask the user to open the link manually.
   /// </summary>
   /// <param name="link"></param>
   public static void OpenLink(string link)
   {
      try
      {
         Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = link });
      }
      catch (Exception ex)
      {
         UIHandle.Instance.PopUpHandle
                 .ShowMBox($"Failed to open the browser: {ex.Message}{Environment.NewLine}Please open the url yourself {link}");
      }
   }

   private static bool IsDiscordRunning => Process.GetProcessesByName("Discord").Length > 0;

   /// <summary>
   /// Opens an application with the specified arguments. <br/><br/>
   /// </summary>
   /// <param name="app"></param>
   /// <param name="args"></param>
   /// <param name="newWindow"></param>
   public static void Open(string app, string args, bool newWindow)
   {
      using var myProcess = new Process();
      myProcess.StartInfo.UseShellExecute = true;
      myProcess.StartInfo.FileName = app;
      myProcess.StartInfo.Arguments = args;
      myProcess.StartInfo.CreateNoWindow = newWindow;
      myProcess.Start();
   }

   /// <summary>
   /// Opens a folder or file in the file explorer. <br/><br/>
   /// If the path is a file, it will open the folder containing the file. <br/>
   /// If the path is invalid, it will show a message box with the error
   /// </summary>
   /// <param name="path"></param>
   /// <returns></returns>
   public static bool OpenFolder(string path)
   {
      string validPath;
      if (Directory.Exists(path))
         validPath = path;
      else if (File.Exists(path))
         validPath = Path.GetDirectoryName(path) ?? string.Empty;
      else
      {
         UIHandle.Instance.PopUpHandle.ShowMBox($"The path {path} can not be opened",
                                                "Folder can not be opened",
                                                MBoxButton.OK,
                                                MessageBoxImage.Warning);
         return false;
      }

      Process.Start(new ProcessStartInfo { FileName = validPath, UseShellExecute = true });
      return true;
   }

   /// <summary>
   /// Opens a file in the default application associated with the file type. <br/><br/>
   /// If the file does not exist, it will show a message box with the error.
   /// </summary>
   /// <param name="path"></param>
   /// <returns></returns>
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

      UIHandle.Instance.PopUpHandle.ShowMBox($"The path {path} can not be opened",
                                             "File can not be opened",
                                             MBoxButton.OK,
                                             MessageBoxImage.Warning);
      return false;
   }

   /// <summary>
   /// Opens a file or folder in the file explorer. <br/>
   /// If the path is a file, it will open the file in the default application associated
   /// </summary>
   /// <param name="path"></param>
   /// <returns></returns>
   public static bool OpenPathIfFileOrFolder(string path)
   {
      if (OpenFile(path))
         return true;
      if (OpenFolder(path))
         return true;

      return false;
   }
}

/// <summary>
/// The preferred editor for opening files. 
/// </summary>
public enum PreferredEditor
{
   /// <summary>
   /// VS Code editor.
   /// </summary>
   VsCode,

   /// <summary>
   /// Notepad++ editor.
   /// </summary>
   NotepadPlusPlus,

   /// <summary>
   /// The Default editor or any other editor that is not specified.
   /// </summary>
   Other,
}