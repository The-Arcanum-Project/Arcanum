using System.IO;
using System.IO.Compression;
using Arcanum.Core.CoreSystems.IO;

namespace Arcanum.Core.CoreSystems.ProjectFileUtil.Mod;

public static class ProjectFileUtil
{
   private const string ARCANUM_PROJECT_FILE_EXTENSION = ".arcanum";
   private const string ARCANUM_PROJECT_FILES_DIRECTORY = "ArcanumProjects";

   public static ZipArchive CreateZipArchive(string zipFilePath)
   {
      return ZipFile.Open(zipFilePath, ZipArchiveMode.Create);
   }

   public static void AddFileToZip(ZipArchive zip, string filePath)
   {
      if (!File.Exists(filePath))
         throw new FileNotFoundException($"The file '{filePath}' does not exist. Can not add to zip archive.");

      zip.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
   }

   public static void AddFileFromStringToArchive(ZipArchive zip, string fileName, string content)
   {
      ArgumentException.ThrowIfNullOrEmpty(fileName, nameof(fileName));

      var entry = zip.CreateEntry(fileName);
      using var writer = new StreamWriter(entry.Open());
      writer.Write(content);
   }

   public static string CreateFromFiles(List<string> files, string outputFileName, string outputDirectory)
   {
      IO.IO.EnsureDirectoryExists(outputDirectory);

      var outputFile = Path.Combine(outputDirectory, outputFileName + ARCANUM_PROJECT_FILE_EXTENSION);
      using var zip = ZipFile.Open(outputFile, ZipArchiveMode.Create);
      foreach (var file in files)
         zip.CreateEntryFromFile(file, Path.GetFileName(file));

      return outputFile;
   }

   public static string CreateAndRemoveEntries(List<string> files, string outputFileName, string outputDirectory)
   {
      IO.IO.EnsureDirectoryExists(outputDirectory);

      var outputFile = Path.Combine(outputDirectory, outputFileName + ARCANUM_PROJECT_FILE_EXTENSION);
      using var zip = ZipFile.Open(outputFile, ZipArchiveMode.Create);
      foreach (var file in files)
      {
         zip.CreateEntryFromFile(file, Path.GetFileName(file));
         File.Delete(file);
      }

      return outputFile;
   }

   public static string? GetFileFromProject(string projectFile, string fileName)
   {
      if (!IsValidProjectFile(projectFile))
         throw new
            InvalidDataException($"Project file '{projectFile}' is not a valid Arcanum project file. Expected extension: {ARCANUM_PROJECT_FILE_EXTENSION}");

      using var zip = ZipFile.OpenRead(projectFile);
      var entry = zip.GetEntry(fileName);
      if (entry == null)
         return null;

      var tempFilePath = Path.Combine(Path.GetTempPath(), entry.Name);
      entry.ExtractToFile(tempFilePath, true);
      return tempFilePath;
   }

   public static List<string> ExtractProjectFile(string projectFile, string outputDirectory)
   {
      if (!IsValidProjectFile(projectFile))
         throw new
            InvalidDataException($"Project file '{projectFile}' is not a valid Arcanum project file. Expected extension: {ARCANUM_PROJECT_FILE_EXTENSION}");

      var extractedFiles = new List<string>();
      using var zip = ZipFile.OpenRead(projectFile);
      foreach (var entry in zip.Entries)
      {
         var filePath = Path.Combine(outputDirectory, entry.Name);
         entry.ExtractToFile(filePath, true);
         extractedFiles.Add(filePath);
      }

      return extractedFiles;
   }

   private static bool IsValidProjectFile(string projectFile)
   {
      return File.Exists(projectFile) && Path.GetExtension(projectFile) == ARCANUM_PROJECT_FILE_EXTENSION;
   }
}