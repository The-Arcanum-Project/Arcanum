#define IS_DEBUG

using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Common;
using Common.Logger;
using Common.UI;
using Common.UI.MBox;

namespace Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;

public static class FileStateManager
{
   public const string LOG_SOURCE = "FSM";
   public static event EventHandler<FileChangedEventArgs>? FileChanged;

   private static readonly object Lock = new();

   // Manages the underlying watchers, one per directory. Key is the directory path.
   private static readonly Dictionary<string, FileSystemWatcher> Watchers = new();

   // Stores the full paths of all files and folders the user has explicitly registered.
   private static readonly HashSet<PathObj> RegisteredPaths = [];

   public static void ReloadFile(FileChangedEventArgs e)
   {
      ArcLog.WriteLine(LOG_SOURCE,
                       LogLevel.INF,
                       $"File change detected: {e.ChangeType} - {FileManager.SanitizePath(e.FullPath, '/')}");

      var fo = GetEu5FileObjFromPath(e.FullPath);
      if (fo == Eu5FileObj.Empty)
      {
         ArcLog.WriteLine(LOG_SOURCE,
                          LogLevel.WRN,
                          $"No registered file object found for path: {FileManager.SanitizePath(e.FullPath, '/')}. Reload aborted.");
         UIHandle.Instance.PopUpHandle.ShowMBox("The changed file is not recognized by the system. " +
                                                "Reload aborted to prevent potential issues.",
                                                "Reload Aborted\nPlease save any progress and restart the application.",
                                                MBoxButton.OK,
                                                MessageBoxImage.Warning);
         return;
      }

      var validation = false;
      fo.Descriptor.LoadingService[0]
        .ReloadSingleFile(fo, null, actionStack: "FileStateManager.ReloadFile", ref validation);
   }

   private static Eu5FileObj GetEu5FileObjFromPath(string fullPath)
   {
      foreach (var fo in DescriptorDefinitions.FileDescriptors.SelectMany(descriptor
                                                                             => descriptor.Files.Where(fo => string
                                                                               .Equals(fo.Path.FullPath,
                                                                                 fullPath,
                                                                                 StringComparison
                                                                                   .OrdinalIgnoreCase))))
         return fo;

      return Eu5FileObj.Empty;
   }

   /// <summary>
   /// Registers a file or a directory to be watched. This method is thread-safe.
   /// </summary>
   /// <param name="pathObj">The Path Object to the file</param>
   /// <exception cref="ArgumentException">Thrown if the path is null, empty, or does not exist.</exception>
   public static void RegisterPath(PathObj pathObj)
   {
      ArgumentNullException.ThrowIfNull(pathObj);
      var path = pathObj.FullPath;
      if (string.IsNullOrEmpty(path))
         throw new ArgumentException("Path cannot be null or empty.", nameof(pathObj));

      var fullPath = Path.GetFullPath(path);

      if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
         throw new ArgumentException($"The path '{fullPath}' does not exist.", nameof(pathObj));

      lock (Lock)
      {
         if (!RegisteredPaths.Add(pathObj))
            return;

         // Determine the directory to watch. For a file, it's its parent directory.
         var directoryToWatch = Directory.Exists(fullPath) ? fullPath : Path.GetDirectoryName(fullPath)!;

         if (Watchers.ContainsKey(directoryToWatch))
            return; // Already watching this directory.

         var watcher = new FileSystemWatcher(directoryToWatch)
         {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
            IncludeSubdirectories = false,
         };

         watcher.Changed += OnAnyEvent;
         watcher.Created += OnAnyEvent;
         watcher.Deleted += OnAnyEvent;
         watcher.Renamed += OnRenamedEvent;
         watcher.Error += OnError;

         Watchers[directoryToWatch] = watcher;
         watcher.EnableRaisingEvents = true;

         ArcLog.WriteLine(LOG_SOURCE,
                          LogLevel.INF,
                          $"Started monitoring directory: {FileManager.SanitizePath(directoryToWatch, '/')}");
      }
   }

   /// <summary>
   /// Unregisters a file or directory from being watched. This method is thread-safe.
   /// </summary>
   public static void UnregisterPath(PathObj pathObj)
   {
      var path = pathObj.FullPath;
      var fullPath = Path.GetFullPath(path);

      lock (Lock)
      {
         if (!RegisteredPaths.Remove(pathObj))
            return; // Was not registered.

         var directoryToWatch = Directory.Exists(fullPath) ? fullPath : Path.GetDirectoryName(fullPath)!;

         var isWatcherStillNeeded =
            RegisteredPaths.Any(p => p.FullPath.StartsWith(directoryToWatch, StringComparison.OrdinalIgnoreCase));

         if (isWatcherStillNeeded || !Watchers.TryGetValue(directoryToWatch, out var watcher))
            return;

         watcher.EnableRaisingEvents = false;
         watcher.Dispose();
         Watchers.Remove(directoryToWatch);

         ArcLog.WriteLine(LOG_SOURCE,
                          LogLevel.INF,
                          $"Stopped monitoring directory: {FileManager.SanitizePath(directoryToWatch, '/')}");
      }
   }

   /// <summary>
   /// Shuts down all watchers and releases resources.
   /// Call this method when your application is exiting.
   /// </summary>
   public static void Shutdown()
   {
      lock (Lock)
      {
         foreach (var watcher in Watchers.Values)
         {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
         }

         Watchers.Clear();
         RegisteredPaths.Clear();

         ArcLog.WriteLine(LOG_SOURCE, LogLevel.INF, "FileStateManager has been shut down and all watchers disposed.");
      }
   }

   private static void OnAnyEvent(object sender, FileSystemEventArgs e)
   {
      HandleEvent(e.ChangeType, e.FullPath);
   }

   private static void OnRenamedEvent(object sender, RenamedEventArgs e)
   {
      HandleEvent(e.ChangeType, e.FullPath, e.OldFullPath);
      // TODO: @Minnator actually change the data structures to reflect the rename.
   }

   private static void HandleEvent(WatcherChangeTypes changeType, string fullPath, string? oldFullPath = null)
   {
      // No lock needed here because we are only reading from _registeredPaths.
      // Even if the collection changes on another thread, a stale read is acceptable
      // and won't cause a crash. A lock is primarily for protecting writes.
      var isPathRegistered =
         RegisteredPaths.Any(p => fullPath.StartsWith(p.FullPath, StringComparison.OrdinalIgnoreCase) ||
                                  (oldFullPath != null &&
                                   oldFullPath.StartsWith(p.FullPath, StringComparison.OrdinalIgnoreCase)));

      var args = new FileChangedEventArgs(changeType, fullPath, oldFullPath);
      if (!isPathRegistered)
      {
         //FileChangeHandler.HandleUnknownFileChange(args);
         UIHandle.Instance.PopUpHandle.ShowFileChangeWindow(args);
         return;
      }

      //FileChangeHandler.HandleFileChange(args);
      UIHandle.Instance.PopUpHandle.ShowFileChangeWindow(args);
      FileChanged?.Invoke(null, args);
   }

   private static void OnError(object sender, ErrorEventArgs e)
   {
#if IS_DEBUG
      Console.WriteLine($"[Error] FileSystemWatcher error: {e.GetException().Message}");
#endif
   }

   public static byte[] CalculateSha256(Eu5FileObj fo)
   {
      return CalculateSha256(fo.Path.FullPath);
   }

   public static byte[] CalculateSha256(string filePath)
   {
      using var sha256 = SHA256.Create();
      using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
      return sha256.ComputeHash(fileStream);
   }

   public static string CalculateSha256Hex(string filePath)
   {
      return BitConverter.ToString(CalculateSha256(filePath)).Replace("-", "").ToLowerInvariant();
   }
}