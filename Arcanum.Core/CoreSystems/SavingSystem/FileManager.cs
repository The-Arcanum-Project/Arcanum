using System.Diagnostics;
using System.IO;
using Arcanum.API.UI;
using Arcanum.Core.CoreSystems.Parsing.DocumentsLoading;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Mod;
using Arcanum.Core.CoreSystems.SavingSystem.Services;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Utils.vdfParser;

namespace Arcanum.Core.CoreSystems.SavingSystem;

/// <summary>
/// The FileManager class is responsible for managing file paths for both vanilla and modded content.
/// </summary>
public static class FileManager
{
   public static DataSpace ModDataSpace = DataSpace.Empty;
   public static DataSpace[] DependentDataSpaces;
   public static DataSpace VanillaDataSpace => DependentDataSpaces[0];

   public static readonly List<ISaveable> NewSaveables = [];

   public static readonly DataSpace DocumentsEUV;

   private static readonly char DefaultSeparationChar = Path.DirectorySeparatorChar;
   private static readonly char AlternativeSeparationChar = Path.AltDirectorySeparatorChar;

   static FileManager()
   {
      var userDocumentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                           "Paradox Interactive",
                                           "Europa Universalis V");
      DocumentsEUV = new("DocumentsEUV",
                         userDocumentsPath.Split(Path.DirectorySeparatorChar),
                         DataSpace.AccessType.ReadOnly);

      var eu5Path = VdfParser.GetEu5Path();
      DependentDataSpaces =
      [
         new("VanillaEUV",
             eu5Path.Split(Path.DirectorySeparatorChar),
             DataSpace.AccessType.ReadOnly),
      ];
   }

   public static string Normalize(string path) => path.Replace(AlternativeSeparationChar, DefaultSeparationChar);

   /// <summary>
   /// Loads the project file descriptor into the application.
   /// This will set the ModDataSpace and DependentDataSpaces to the values from the descriptor.
   /// </summary>
   /// <param name="descriptor"></param>
   public static void LoadToApplication(this ProjectFileDescriptor descriptor)
   {
      DependentDataSpaces = [descriptor.VanillaPath, .. descriptor.RequiredMods];
      ModDataSpace = descriptor.ModPath;

      var modMetadata = ExistingModsLoader.ParseModMetadata(descriptor.ModPath.FullPath);
      if (modMetadata == null)
      {
         AppData.WindowLinker
                .ShowMBox($"Failed to load mod metadata for {modMetadata?.Name} (ID: {modMetadata?.Id}).\nSome functionality may depend on this metadata and thus be broken or not available",
                          "Mod Metadata Loaded");
         return;
      }

      CoreData.ModMetadata = modMetadata!;
   }

   public static string GetDocumentsPath(params string[]? subPaths)
   {
      if (subPaths == null || subPaths.Length == 0)
         return DocumentsEUV.FullPath;

      return Path.Combine(DocumentsEUV.FullPath, Path.Combine(subPaths));
   }

   /// <summary>
   /// Combines the subPaths with the vanilla path but does NOT check if the path should be replaced.
   /// </summary>
   /// <param name="subPaths"></param>
   /// <returns></returns>
   public static string GetVanillaPath(params string[]? subPaths)
   {
      if (subPaths == null || subPaths.Length == 0)
         return VanillaDataSpace.FullPath;

      return Path.Combine(VanillaDataSpace.FullPath, Path.Combine(subPaths));
   }

   public static string GetModPath(params string[]? subPaths)
   {
      if (subPaths == null || subPaths.Length == 0)
         return ModDataSpace.FullPath;

      return Path.Combine(ModDataSpace.FullPath, Path.Combine(subPaths));
   }

   public static bool ExistsInMod(string[] internalPath, bool isDirectory = false)
      => ExistsInMod(Path.Combine(internalPath), isDirectory);

   public static bool ExistsInVanilla(string[] internalPath, bool isDirectory = false)
      => ExistsInVanilla(Path.Combine(internalPath), isDirectory);

   /// <summary>
   /// Returns true if the given path exists in the mod data space.
   /// This will check the mod data space only, not the vanilla data space.
   /// </summary>
   /// <param name="path"></param>
   /// <param name="isDirectory"></param>
   /// <returns></returns>
   /// <exception cref="ArgumentException"></exception>
   public static bool ExistsInMod(string path, bool isDirectory = false)
   {
      if (string.IsNullOrEmpty(path))
         throw new ArgumentException("Path cannot be null or empty.", nameof(path));

      var fullPath = Path.Combine(ModDataSpace.FullPath, path);
      return isDirectory ? Directory.Exists(fullPath) : File.Exists(fullPath);
   }

   /// <summary>
   /// Returns true if the given path exists in the vanilla data space.
   /// This will check the vanilla data space only, not the mod data space.
   /// </summary>
   /// <param name="path"></param>
   /// <param name="isDirectory"></param>
   /// <returns></returns>
   /// <exception cref="ArgumentException"></exception>
   public static bool ExistsInVanilla(string path, bool isDirectory = false)
   {
      if (string.IsNullOrEmpty(path))
         throw new ArgumentException("Path cannot be null or empty.", nameof(path));

      var fullPath = Path.Combine(VanillaDataSpace.FullPath, path);
      return isDirectory ? Directory.Exists(fullPath) : File.Exists(fullPath);
      //TODO: @Minnator make this also take base mods into account
   }

   /// <summary>
   /// Returns all files in the given subdirectory path from either the mod or vanilla data space.
   /// Handles <c>replaced paths</c> and also invalid paths.
   /// </summary>
   /// <param name="subPath"></param>
   /// <param name="searchPattern"></param>
   /// <returns><see cref="ICollection{T}"/> of (<c>string</c>, <c>bool</c>) representing the path and if it is a mod file</returns>
   /// <exception cref="ArgumentException"></exception>
   public static ICollection<(string path, bool isMod)> GetAllFilesInDirectory(string[] subPath, string searchPattern)
   {
      var vSubPath = RemoveFileNameEntryFromPath(subPath, out var fileName);
      if (vSubPath.Length == 0)
         throw new ArgumentException("SubPaths cannot be empty after removing the file name entry.", nameof(subPath));
      if (fileName != null)
         throw new ArgumentException("The SubPath should NOT point to a file but a directory", nameof(subPath));

      var modFiles = Directory.GetFiles(GetModPath(vSubPath), searchPattern);
      if (IsPathReplaced(vSubPath))
         return modFiles.Select(file => (file, true)).ToArray();

      var defined = new HashSet<string>(modFiles);
      List<(string path, bool isMod)> fileList = [];
      fileList.AddRange(defined.Select(file => (file, true)));

      var vanillaFiles = Directory.GetFiles(GetVanillaPath(vSubPath), searchPattern);
      foreach (var file in vanillaFiles)
      {
         if (defined.Contains(file))
            continue; // We already have this file in the mod data space

         fileList.Add((file, false));
      }

      return fileList;
   }

   /// <summary>
   /// Should only be used for paths pointing to a file not a directory.
   /// Creates a FileInformation object for the given file type information and dependencies.
   /// This will check if the file exists in the mod data space or the vanilla data space and return the corresponding FileInformation.
   /// </summary>
   /// <param name="fileTypeInfo"></param>
   /// <param name="savingService"></param>
   /// <param name="dependencies"></param>
   /// <param name="subPath"></param>
   /// <returns></returns>
   /// <exception cref="ArgumentException"></exception>
   /// <exception cref="OhShitHereWeGoAgainException"></exception>
   public static FileInformation GetGameFileForMod(FileTypeInformation fileTypeInfo,
                                                   ISavingService savingService,
                                                   FileDescriptor[] dependencies,
                                                   params string[] subPath)
   {
      // TODO: @Melco @Minnator
      // How do we handle the Dependencies?
      // How do we verify that they are already loaded and valid?

      if (subPath == null || subPath.Length == 0)
         throw new ArgumentException("SubPaths cannot be null or empty.", nameof(subPath));

      var vSubPath = RemoveFileNameEntryFromPath(subPath, out var fileName);
      if (vSubPath.Length == 0)
         throw new ArgumentException("SubPaths cannot be empty after removing the file name entry.", nameof(subPath));

      if (ExistsInMod(vSubPath, fileName != null))
      {
         // We have a mod file, so we return the mod file information
         Debug.Assert(fileName != null, nameof(fileName) + " != null");
         return new(fileName, true, new(dependencies, subPath, savingService, fileTypeInfo));
      }

      if (ExistsInVanilla(vSubPath, fileName != null))
      {
         // We have a vanilla file, so we return the vanilla file information
         Debug.Assert(fileName != null, nameof(fileName) + " != null");
         return new(fileName, true, new(dependencies, subPath, savingService, fileTypeInfo));
      }

      throw new
         OhShitHereWeGoAgainException("No file found in mod or vanilla data space for the given path. Is the file missing or the path incorrect?");
   }

   /// <summary>
   /// Returns a <see cref="FileInformation"/> for each file in the given directory boxed in a <see cref="List{FileInformation}"/>.
   /// </summary>
   /// <param name="fileTypeInfo"></param>
   /// <param name="savingService"></param>
   /// <param name="dependencies"></param>
   /// <param name="isMod"></param>
   /// <param name="subPath"></param>
   /// <returns></returns>
   public static List<FileInformation> GetAllFileInfosForDirectory(FileTypeInformation fileTypeInfo,
                                                                   ISavingService savingService,
                                                                   FileDescriptor[] dependencies,
                                                                   params string[] subPath)
   {
      return GetAllFileInfosForDirectory(new(dependencies, subPath, savingService, fileTypeInfo));
   }

   /// <summary>
   /// Returns a <see cref="FileInformation"/> for each file in the given directory boxed in a <see cref="List{FileInformation}"/>.
   /// </summary>
   /// <param name="descriptor"></param>
   /// <param name="isMod"></param>
   /// <returns></returns>
   /// <exception cref="ArgumentException"></exception>
   public static List<FileInformation> GetAllFileInfosForDirectory(FileDescriptor descriptor)
   {
      var vSubPath = RemoveFileNameEntryFromPath(descriptor.LocalPath, out var fileName);
      if (vSubPath.Length == 0)
         throw new ArgumentException("`descriptor.LocalPath` cannot be empty after removing the file name entry.",
                                     nameof(descriptor.LocalPath));
      if (fileName != null)
         throw new ArgumentException("The `descriptor.LocalPath` should NOT point to a file but a directory",
                                     nameof(descriptor.LocalPath));

      List<FileInformation> fileInfos = [];
      foreach (var (file, isMod) in GetAllFilesInDirectory(descriptor.LocalPath, $"*.{descriptor.FileType.FileEnding}"))
         fileInfos.Add(new(Path.GetFileName(file),
                           isMod,
                           descriptor));

      return fileInfos;
   }

   public static string GetDependentPath(params string[]? subPaths)
   {
      if (subPaths == null || subPaths.Length == 0)
         return string.Empty;

      // We check from mod in editing down through the base mods to vanilla
      for (var i = DependentDataSpaces.Length - 1; i >= 0; i--)
      {
         // we hit vanilla, so we check in the .metadata file if the path is replaced
         // if it is replaced, we return string.Empty
         if (i == 0 && IsPathReplaced(subPaths))
            return string.Empty;

         var verifyPath = Path.Combine(DependentDataSpaces[i].FullPath, Path.Combine(subPaths));
         if (File.Exists(verifyPath) || Directory.Exists(verifyPath))
            return verifyPath;
      }

      return string.Empty;
   }

   public static bool IsPathReplaced(params string[]? subPaths)
   {
      if (subPaths == null || subPaths.Length == 0)
         return false;

      // if we have a file ending we need to remove it and only check the path
      return CoreData.ModMetadata.ReplacePaths.Any(replacePath
                                                      => replacePath.Equals(RemoveFileNameEntryFromPath(subPaths,
                                                                             out _)));
   }

   private static string RemoveFileNameEntryFromPath(string[] subPaths, out string? fileName)
   {
      if (subPaths.Length == 0)
      {
         fileName = null;
         return string.Empty;
      }

      // We take it as granted that folders do not contain a dot in their name
      // so we can safely assume that the last entry is the file name if it contains a dot
      // If it does not contain a dot, we assume it is a folder name
      if (subPaths[^1].Contains('.'))
      {
         fileName = subPaths[^1];
         return Path.Combine(subPaths[..^1]);
      }

      fileName = null;
      return Path.Combine(subPaths);
   }

   public static void GenerateCustomSavingCatalog()
   {
      // TODO: Implement for existing saveables

      // For new saveable, we need to set the file dropdown to a new default value
      // List of a tuple of a string and corresponding FileObj
      // The default option is marked with the FileObj.Empty

      Dictionary<FileInformation, List<(string, FileObj)>> groupedSaveables = [];

      foreach (var fileInformation in NewSaveables.Select(saveable => saveable.GetFileInformation()))
      {
         if (groupedSaveables.TryGetValue(fileInformation, out var fileList))
            continue;

         fileList = GenerateFileSelection(fileInformation);
         groupedSaveables[fileInformation] = fileList;
      }
   }

   public static List<(string, FileObj)> GenerateFileSelection(FileInformation fileInformation)
   {
      var descriptor = fileInformation.Descriptor;
      var allowsOverwrite = fileInformation.AllowsOverwrite;
      if (!allowsOverwrite)
      {
         // We need to Test if a file with the given name already exists for the descriptor
         var files = descriptor.Files;
         if (files.Count > 0)
         {
            // There exist at least one file, so we need to check if the file name already exists
            if (files.Any(file => !file.AllowMultipleInstances && file.Path.Filename == fileInformation.FileName))
            {
               throw new
                  InvalidOperationException($"A file with the name '{fileInformation.FileName}' already exists for the Path '{descriptor.GetFilePath()}' and does not allow multiple instances.");
            }
         }
      }

      return [];
   }
}