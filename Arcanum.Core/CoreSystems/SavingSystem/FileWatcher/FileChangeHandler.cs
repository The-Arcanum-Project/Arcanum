using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Common.Logger;
using Common.UI;
using Common.UI.MBox;

namespace Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;

public class FileChangeHandler
{
    /*
           foreach (var fileDescriptor in DescriptorDefinitions.FileDescriptors)
           {
               if (fileDescriptor.Files.Count <= 0)
                   continue;
               var file = fileDescriptor.Files[0];
               if (!file.Path.FullPathWithoutFilename.Equals(pathWithoutFilename, StringComparison.OrdinalIgnoreCase))
                   continue;
               foreach (var candidateFile in fileDescriptor.Files.Where(candidateFile => candidateFile.Path.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase)))
               {
                   fileObj = candidateFile;
                   return true;
               }
           }
        */
    private static bool GetFileObjectFromPath(string path, [MaybeNullWhen(false)] out Eu5FileObj fileObj)
    {
        var pathWithoutFilename = Path.GetDirectoryName(path) ?? string.Empty;
        foreach (var candidateFile in from fileDescriptor in DescriptorDefinitions.FileDescriptors
                 where fileDescriptor.Files.Count > 0
                 let file = fileDescriptor.Files[0]
                 where file.Path.FullPathWithoutFilename.Equals(pathWithoutFilename, StringComparison.OrdinalIgnoreCase)
                 from candidateFile in fileDescriptor.Files
                 where candidateFile.Path.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase)
                 select candidateFile)
        {
            fileObj = candidateFile; return true;
        }

        fileObj = null;
        return false;
    }

    public static void HandleFileRename(FileChangedEventArgs args, Eu5FileObj fileObj)
    {
        fileObj.Path.Filename = Path.GetFileName(args.FullPath);
        ArcLog.WriteLine(FileStateManager.LOG_SOURCE, LogLevel.INF,
            $"File renamed from {args.OldFullPath} to {args.FullPath}");
    }

    private static void HandleModChange(FileChangedEventArgs args, Eu5FileObj fileObj)
    {
        switch (args.ChangeType)
        {
            case WatcherChangeTypes.Renamed:
                HandleFileRename(args, fileObj);
                break;
            case WatcherChangeTypes.Changed:
                ArcLog.WriteLine(FileStateManager.LOG_SOURCE, LogLevel.INF,
                    $"File changed: {args.FullPath}");
                break;
            case WatcherChangeTypes.Created:
                ArcLog.WriteLine(FileStateManager.LOG_SOURCE, LogLevel.INF,
                    $"File created: {args.FullPath}");
                break;
            case WatcherChangeTypes.Deleted:
                ArcLog.WriteLine(FileStateManager.LOG_SOURCE, LogLevel.INF,
                    $"File deleted: {args.FullPath}");
                break;
            case WatcherChangeTypes.All:
                break;
            default:
                ArcLog.WriteLine(FileStateManager.LOG_SOURCE, LogLevel.WRN,
                    $"Unhandled file change type: {args.ChangeType} for file {args.FullPath}");
                break;
        }
    }
    
    private static void HandleVanillaChange(FileChangedEventArgs args, Eu5FileObj fileObj)
    {
        switch (args.ChangeType)
        {
            case WatcherChangeTypes.Renamed:
                HandleFileRename(args, fileObj);
                UIHandle.Instance.PopUpHandle.ShowMBox(
                    $"The vanilla or base mod file '{args.OldFullPath}' has been renamed. Renaming files is supported, but might be unintended. Please verify that this change was intentional.",
                    "Vanilla or Base Mod File Renamed",
                    MBoxButton.OK, MessageBoxImage.Warning);
                break;
            case WatcherChangeTypes.Changed:
                ArcLog.WriteLine(FileStateManager.LOG_SOURCE, LogLevel.INF,
                    $"File changed: {args.FullPath}");
                break;
            case WatcherChangeTypes.Created:
                ArcLog.WriteLine(FileStateManager.LOG_SOURCE, LogLevel.INF,
                    $"File created: {args.FullPath}");
                break;
            case WatcherChangeTypes.Deleted:
                ArcLog.WriteLine(FileStateManager.LOG_SOURCE, LogLevel.INF,
                    $"File deleted: {args.FullPath}");
                break;
            case WatcherChangeTypes.All:
                break;
            default:
                ArcLog.WriteLine(FileStateManager.LOG_SOURCE, LogLevel.WRN,
                    $"Unhandled file change type: {args.ChangeType} for file {args.FullPath}");
                break;
        }
    }

    public static void HandleFileChange(FileChangedEventArgs args)
    {
        if (!GetFileObjectFromPath(args.OldFullPath ?? args.FullPath, out var fileObj))
        {
            UIHandle.Instance.PopUpHandle.ShowMBox(
                $"The file change handler could not find a matching file object for the path: {args.FullPath}. This indicates a desynchronization between the file system and the game's file descriptors. Please backup your changed file and save as soon as possible.",
                "File Watcher Error",
                MBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        
        if(fileObj.Path.DataSpace.Access == DataSpace.AccessType.ReadWrite)
            HandleModChange(args, fileObj);
        else
            HandleVanillaChange(args, fileObj);
        
    }
    
    public static void HandleUnknownFileChange(FileChangedEventArgs args)
    {
        // Get Dataspace of changed file
        var dataspace = FileManager.GetDataSpaceFromFullPath(args.FullPath);
        var isReadOnly = dataspace.Access == DataSpace.AccessType.ReadOnly;
        //TODO: @Melco In the future, we might want to get the corresponding descriptor here as well.
        switch (args.ChangeType)
        {
            case WatcherChangeTypes.Renamed:
                ArcLog.WriteLine(FileStateManager.LOG_SOURCE, LogLevel.INF,
                    $"Not tracked file renamed from {args.OldFullPath} to {args.FullPath}");
                break;
            case WatcherChangeTypes.Changed:
                ArcLog.WriteLine(FileStateManager.LOG_SOURCE, LogLevel.INF,
                    $"Not tracked file changed: {args.FullPath}");
                break;
            case WatcherChangeTypes.Created:
                if (isReadOnly)
                {
                    UIHandle.Instance.PopUpHandle.ShowMBox(
                        $"A new file '{args.FullPath}' has been created in a read-only data space ('{dataspace.Name}'). Arcanum does not support new files at the moment. Please ensure that this change was intentional and reload the mod. Do you want to reload now?",
                        "Read-Only Data Space File Created",
                        MBoxButton.OKCancel, MessageBoxImage.Warning);
                }
                else
                {
                    //TODO: @Melco custom UI for this. We want to have a reload and save and reload and discard option here.
                    UIHandle.Instance.PopUpHandle.ShowMBox(
                        $"A new file '{args.FullPath}' has been created. To allow it to be tracked by Arcanum, please reload the mod. Do you want to reload now?",
                        "Untracked File Created",
                        MBoxButton.OKCancel, MessageBoxImage.Warning);
                }
                
                ArcLog.WriteLine(FileStateManager.LOG_SOURCE, LogLevel.INF,
                    $"Not tracked file created: {args.FullPath}");
                break;
            case WatcherChangeTypes.Deleted:
                ArcLog.WriteLine(FileStateManager.LOG_SOURCE, LogLevel.INF,
                    $"Not tracked file deleted: {args.FullPath}");
                break;
            case WatcherChangeTypes.All:
                break;
            default:
                ArcLog.WriteLine(FileStateManager.LOG_SOURCE, LogLevel.WRN,
                    $"Unhandled file change type: {args.ChangeType} for file {args.FullPath}");
                break;
        }
    }
}