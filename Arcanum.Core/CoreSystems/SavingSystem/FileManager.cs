using System.IO;
using Arcanum.Core.CoreSystems.Parsing.DocumentsLoading;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Mod;
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

   public static List<ISaveable> NewSaveables = [];

   public static readonly DataSpace DocumentsEUV;

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
         MessageBox.Show($"Failed to load mod metadata for {modMetadata?.Name} (ID: {modMetadata?.Id}).\nSome functionality may depend on this metadata and thus be broken or not available",
                         "Mod Metadata Loaded",
                         MessageBoxButtons.CancelTryContinue);
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

      return false;
      //TODO: @Minnator parse the .metadata file from the mods and implement proper replace checks
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