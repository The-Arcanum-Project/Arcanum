namespace Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;

using System.IO;

public class FileChangedEventArgs(WatcherChangeTypes changeType, string fullPath, string? oldFullPath = null)
   : EventArgs
{
   public WatcherChangeTypes ChangeType { get; } = changeType;
   public string FullPath { get; } = fullPath;
   public string? OldFullPath { get; } = oldFullPath; // Used for rename events
}