using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;

namespace Arcanum.Core.CoreSystems.SavingSystem;

/// <summary>
/// The FileManager class is responsible for managing file paths for both vanilla and modded content.
/// </summary>
public static class FileManager
{
   public static DataSpace ModDataSpace = DataSpace.Empty;
   public static DataSpace[] DependendDataSpaces = [DataSpace.Empty];
   public static DataSpace VanillaDataSpace => DependendDataSpaces[0];

   public static List<ISaveable> NewSaveables = [];

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