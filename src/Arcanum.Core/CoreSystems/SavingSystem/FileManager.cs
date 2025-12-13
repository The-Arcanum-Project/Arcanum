using System.Diagnostics;
using System.IO;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.DocumentsLoading;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Mod;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Utils.vdfParser;
#if !DEBUG
using Common.UI;
#endif

namespace Arcanum.Core.CoreSystems.SavingSystem;

/// <summary>
/// The FileManager class is responsible for managing file paths for both vanilla and modded content.
/// </summary>
public static class FileManager
{
   public static DataSpace ModDataSpace = DataSpace.Empty;
   public static DataSpace[] DependentDataSpaces;
   public static DataSpace VanillaDataSpace => DependentDataSpaces[0];

   public static readonly DataSpace DocumentsEUV;

   private static readonly char DefaultSeparationChar = Path.DirectorySeparatorChar;
   private static readonly char AlternativeSeparationChar = Path.AltDirectorySeparatorChar;

   private static readonly Dictionary<Type, FileDescriptor> FileDescriptorCache = new();

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

      foreach (var descriptor in DescriptorDefinitions.FileDescriptors)
      {
         foreach (var type in descriptor.LoadingService[0]
                                        .ParsedObjects.Where(type => !FileDescriptorCache.TryAdd(type, descriptor)))
            Debug.Fail($"FileDescriptorCache already contains a descriptor for type {type.FullName}");
      }
   }

   public static void InitHeadlessMode(string modPath, List<string> dependencies)
   {
      ModDataSpace = new("ModEUV",
                         modPath.Split(Path.DirectorySeparatorChar),
                         DataSpace.AccessType.ReadWrite);

      DependentDataSpaces =
      [
         .. dependencies.Select(dep => new DataSpace("DependencyMod",
                                                     dep.Split(Path.DirectorySeparatorChar),
                                                     DataSpace.AccessType.ReadOnly)),
      ];

      CoreData.ModMetadata = ExistingModsLoader.ParseModMetadata(modPath)!;

      FileDescriptorCache.Clear();
      foreach (var descriptor in DescriptorDefinitions.FileDescriptors)
      {
         foreach (var type in descriptor.LoadingService[0]
                                        .ParsedObjects.Where(type => !FileDescriptorCache.TryAdd(type, descriptor)))
            Debug.Fail($"FileDescriptorCache already contains a descriptor for type {type.FullName}");
      }
   }

   public static string SanitizePath(string path, char separationChar = '.')
   {
      if (string.IsNullOrEmpty(path))
         return string.Empty;

      // Remove the mod or vanilla path if it is the beginning of the path
      if (path.StartsWith(ModDataSpace.FullPath, StringComparison.OrdinalIgnoreCase))
         return ArrayToPointingPath(path[ModDataSpace.FullPath.Length..]
                                      .TrimStart(DefaultSeparationChar, AlternativeSeparationChar),
                                    separationChar);
      if (path.StartsWith(VanillaDataSpace.FullPath, StringComparison.OrdinalIgnoreCase))
         return ArrayToPointingPath(path[VanillaDataSpace.FullPath.Length..]
                                      .TrimStart(DefaultSeparationChar, AlternativeSeparationChar),
                                    separationChar);
      if (path.StartsWith(DocumentsEUV.FullPath, StringComparison.OrdinalIgnoreCase))
         return ArrayToPointingPath(path[(DocumentsEUV.FullPath.Length - 1)..]
                                      .TrimStart(DefaultSeparationChar, AlternativeSeparationChar),
                                    separationChar);

      // If the path does not start with any of the known paths, we return the path as is
      return path;
   }

   public static string ArrayToPointingPath(string[] pathParts, char separationChar)
   {
      if (pathParts.Length == 0)
         return string.Empty;

      return string.Join(separationChar, pathParts);
   }

   public static string ArrayToPointingPath(string path, char separationChar)
   {
      if (string.IsNullOrEmpty(path))
         return string.Empty;

      var pathParts = path.Split(DefaultSeparationChar, AlternativeSeparationChar);
      return ArrayToPointingPath(pathParts, separationChar);
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
#if !DEBUG
         UIHandle.Instance.PopUpHandle
                 .ShowMBox($"Failed to load mod metadata for {modMetadata?.Name} (ID: {modMetadata?.Id}).\nSome functionality may depend on this metadata and thus be broken or not available",
                           "Mod Metadata Loaded");
#endif
         return;
      }

      CoreData.ModMetadata = modMetadata;
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

   public static bool ExistsInMod(string[] internalPath, bool isDirectory = false) => ExistsInMod(Path.Combine(internalPath), isDirectory);

   public static bool ExistsInVanilla(string[] internalPath, bool isDirectory = false) => ExistsInVanilla(Path.Combine(internalPath), isDirectory);

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
   public static ICollection<PathObj> GetAllFilesForDirectory(
      string[] subPath,
      string searchPattern)
   {
      // Loading Order:
      // --> Vanilla Files
      // --> Mod Files
      //
      // Files are alpha numerically sorted for Vanilla and mod each.
      //    objects in the mod folder can replace objects in the vanilla folder.
      //    Vanilla is only not loaded if the path is replaced (does not affect sub folders)

      var internalPath = RemoveFileNameEntryFromPath(subPath, out var fileName);
      if (fileName != null)
         return [new(internalPath, fileName, GetDataSpace(Path.Combine(subPath)))];

      List<string> modFiles = [];
      List<PathObj> modPathObjs = [];
      if (Directory.Exists(GetModPath(internalPath)))
      {
         modFiles = Directory.GetFiles(GetModPath(internalPath), searchPattern).Select(Path.GetFileName).ToList()!;
         modPathObjs = modFiles.Select(file => new PathObj(internalPath, file, ModDataSpace)).ToList();
         if (IsPathReplaced(internalPath))
         {
            modPathObjs.Sort(new PathObjComparer());
            return modPathObjs;
         }
      }

      var defined = new HashSet<string>(modFiles);
      List<PathObj> fileList = [];

      var vanillaFiles = Directory.GetFiles(GetVanillaPath(internalPath), searchPattern);
      foreach (var file in vanillaFiles)
      {
         var fileNameOnly = Path.GetFileName(file);
         if (defined.Contains(fileNameOnly))
            continue; // We already have this file in the mod data space

         fileList.Add(new(internalPath, fileNameOnly, VanillaDataSpace));
      }

      fileList.Sort(new PathObjComparer());

      fileList.AddRange(modPathObjs);
      return fileList;
   }

   public static DataSpace GetDataSpaceFromFullPath(string fullPath)
   {
      if (fullPath.StartsWith(ModDataSpace.FullPath, StringComparison.OrdinalIgnoreCase))
         return ModDataSpace;

      foreach (var dependentDataSpace in DependentDataSpaces)
         if (fullPath.StartsWith(dependentDataSpace.FullPath, StringComparison.OrdinalIgnoreCase))
            return dependentDataSpace;

      return DataSpace.Empty;
   }

   private static DataSpace GetDataSpace(string path)
   {
      //TODO: @Minnator make this also take base mods into account
      return ExistsInMod(path) ? ModDataSpace : VanillaDataSpace;
   }

   public static Eu5FileObj GetGameOrModFileObj(string? fileName, FileDescriptor descriptor)
   {
      // TODO: @Melco @Minnator
      // How do we handle the Dependencies?
      // How do we verify that they are already loaded and valid?

      if (ExistsInMod(descriptor.LocalPath, fileName != null))
      {
         // We have a mod file, so we return the mod file information
         Debug.Assert(fileName != null, nameof(fileName) + " != null");
         return new(new(descriptor.LocalPath, fileName, ModDataSpace), descriptor);
      }

      if (ExistsInVanilla(descriptor.LocalPath, fileName != null))
      {
         // We have a vanilla file, so we return the vanilla file information
         Debug.Assert(fileName != null, nameof(fileName) + " != null");
         return new(new(descriptor.LocalPath, fileName, VanillaDataSpace),
                    descriptor);
      }

      throw new
         OhShitHereWeGoAgainException("No file found in mod or vanilla data space for the given path. Is the file missing or the path incorrect?");
   }

   /// <summary>
   /// Returns a <see cref="FileInformation"/> for each file in the given directory boxed in a <see cref="List{FileInformation}"/>.
   /// </summary>
   /// <param name="descriptor"></param>
   /// <returns></returns>
   /// <exception cref="ArgumentException"></exception>
   public static List<Eu5FileObj> GetAllFileInfosForDirectory(FileDescriptor descriptor)
   {
      List<Eu5FileObj> fileInfos = [];
      foreach (var po in GetAllFilesForDirectory(descriptor.LocalPath, $"*.{descriptor.FileType.FileEnding}"))
         fileInfos.Add(new(po, descriptor));

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

   private static string[] RemoveFileNameEntryFromPath(string[] subPaths, out string? fileName)
   {
      if (subPaths.Length == 0)
      {
         fileName = null;
         return [];
      }

      // We take it as granted that folders do not contain a dot in their name
      // so we can safely assume that the last entry is the file name if it contains a dot
      // If it does not contain a dot, we assume it is a folder name
      if (subPaths[^1].Contains('.'))
      {
         fileName = subPaths[^1];
         return subPaths[..^1];
      }

      fileName = null;
      return subPaths;
   }

   public static (string[] path, FileDescriptor descriptor) GeneratePathForNewObject(
      IEu5Object no,
      bool allowReuseOfExistingArcFile)
   {
      if (!FileDescriptorCache.TryGetValue(no.GetType(), out var descriptor))
         throw new
            OhShitHereWeGoAgainException($"No FileDescriptor found for type {no.GetType().FullName}. Cannot generate path for new object.");

      var folderPath = descriptor.LocalPath.Aggregate(ModDataSpace.FullPath, Path.Combine);
      return ([
                 ..descriptor.LocalPath, GetDefaultFileNameForFolder(folderPath,
                                                                     no.GetType(),
                                                                     descriptor.FileType.FileEnding,
                                                                     allowReuseOfExistingArcFile),
              ],
              descriptor);
   }

   public const string ARCANUM_FILE_NAME_WATERMARK = "_ARC";
   // TODO: Create a hidden settings option to disable this watermark in generated file names for contributors and Patreon Members

   public static string GetDefaultFileNameForFolder(string folder,
                                                    Type objectType,
                                                    string fileEnding,
                                                    bool allowReuseOfExistingArcFile)
   {
      IO.IO.EnsureDirectoryExists(folder);

      var num = 0;
      string name;
      do
         name = $"{num++:D2}{ARCANUM_FILE_NAME_WATERMARK}_{objectType.Name}.{fileEnding}";
      while (File.Exists(Path.Combine(folder, name)) && !allowReuseOfExistingArcFile);
      return name;
   }
}