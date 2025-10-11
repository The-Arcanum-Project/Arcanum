#define IS_DEBUG

using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;

public static class FileStateManager
{
   public static event EventHandler<FileChangedEventArgs>? FileChanged;

   private static readonly object Lock = new();

   // Manages the underlying watchers, one per directory. Key is the directory path.
   private static readonly Dictionary<string, FileSystemWatcher> Watchers = new();

   // Stores the full paths of all files and folders the user has explicitly registered.
   private static readonly HashSet<string> RegisteredPaths = [];

   // debouncing to avoid duplicate events for a single file save.
   private static readonly ConcurrentDictionary<string, DateTime> DebounceCache = new();
   private static readonly TimeSpan DebounceTime = TimeSpan.FromMilliseconds(250);

   public static void RegisterPath(PathObj po)
   {
      ArgumentNullException.ThrowIfNull(po);
      RegisterPath(po.FullPath);
   }

   /// <summary>
   /// Registers a file or a directory to be watched. This method is thread-safe.
   /// </summary>
   /// <param name="path">The full path to the file or directory.</param>
   /// <exception cref="ArgumentException">Thrown if the path is null, empty, or does not exist.</exception>
   public static void RegisterPath(string path)
   {
      if (string.IsNullOrEmpty(path))
         throw new ArgumentException("Path cannot be null or empty.", nameof(path));

      var fullPath = Path.GetFullPath(path);

      if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
         throw new ArgumentException($"The path '{fullPath}' does not exist.", nameof(path));

      lock (Lock)
      {
         if (!RegisteredPaths.Add(fullPath))
            return;

         // Determine the directory to watch. For a file, it's its parent directory.
         var directoryToWatch = Directory.Exists(fullPath) ? fullPath : Path.GetDirectoryName(fullPath)!;

         if (Watchers.ContainsKey(directoryToWatch))
            return; // Already watching this directory.

         var watcher = new FileSystemWatcher(directoryToWatch)
         {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
            IncludeSubdirectories = true,
         };

         watcher.Changed += OnAnyEvent;
         watcher.Created += OnAnyEvent;
         watcher.Deleted += OnAnyEvent;
         watcher.Renamed += OnRenamedEvent;
         watcher.Error += OnError;

         Watchers[directoryToWatch] = watcher;
         watcher.EnableRaisingEvents = true;
#if IS_DEBUG
         Console.WriteLine($"[Watcher] Started monitoring directory: {directoryToWatch}");
#endif
      }
   }

   /// <summary>
   /// Unregisters a file or directory from being watched. This method is thread-safe.
   /// </summary>
   /// <param name="path">The full path to the file or directory.</param>
   public static void UnregisterPath(string path)
   {
      var fullPath = Path.GetFullPath(path);

      lock (Lock)
      {
         if (!RegisteredPaths.Remove(fullPath))
            return; // Was not registered.

         var directoryToWatch = Directory.Exists(fullPath) ? fullPath : Path.GetDirectoryName(fullPath)!;

         var isWatcherStillNeeded =
            RegisteredPaths.Any(p => p.StartsWith(directoryToWatch, StringComparison.OrdinalIgnoreCase));

         if (isWatcherStillNeeded || !Watchers.TryGetValue(directoryToWatch, out var watcher))
            return;

         watcher.EnableRaisingEvents = false;
         watcher.Dispose();
         Watchers.Remove(directoryToWatch);
#if IS_DEBUG
         Console.WriteLine($"[Watcher] Stopped monitoring directory: {directoryToWatch}");
#endif
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
#if IS_DEBUG
         Console.WriteLine("[Watcher] All file watchers have been shut down.");
#endif
      }
   }

   private static void OnAnyEvent(object sender, FileSystemEventArgs e)
   {
      HandleEvent(e.ChangeType, e.FullPath);
   }

   private static void OnRenamedEvent(object sender, RenamedEventArgs e)
   {
      HandleEvent(e.ChangeType, e.FullPath, e.OldFullPath);
   }

   private static void HandleEvent(WatcherChangeTypes changeType, string fullPath, string? oldFullPath = null)
   {
      if (DebounceCache.TryGetValue(fullPath, out var lastEventTime))
         if (DateTime.UtcNow - lastEventTime < DebounceTime)
            return;

      DebounceCache[fullPath] = DateTime.UtcNow;

      // No lock needed here because we are only reading from _registeredPaths.
      // Even if the collection changes on another thread, a stale read is acceptable
      // and won't cause a crash. A lock is primarily for protecting writes.
      var isPathRegistered = RegisteredPaths.Any(p => fullPath.StartsWith(p, StringComparison.OrdinalIgnoreCase) ||
                                                      (oldFullPath != null &&
                                                       oldFullPath.StartsWith(p, StringComparison.OrdinalIgnoreCase)));

      if (!isPathRegistered)
         return;

      var args = new FileChangedEventArgs(changeType, fullPath, oldFullPath);
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