using System.IO;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.SavingSystem.Util;

public class FileDescriptor : IEmpty<FileDescriptor>
{
   public string[] LocalPath { get; }
   public readonly FileTypeInformation FileType;
   public FileLoadingService[] LoadingService { get; }
   public readonly bool AllowMultipleInstances;

   public List<Eu5FileObj> Files { get; }

   public FileDescriptor(string[] localPath,
                         FileTypeInformation fileType,
                         FileLoadingService[] loadingService,
                         bool isMultithreadable,
                         bool allowMultipleInstances = true) //TODO @MelCo remove uniqueId
   {
      foreach (var fileLoadingService in loadingService)
      {
         fileLoadingService.Descriptor = this;
      }
      LocalPath = localPath;
      FileType = fileType;
      LoadingService = loadingService;
      AllowMultipleInstances = allowMultipleInstances;
      IsMultithreadable = isMultithreadable;
      Files = FileManager.GetAllFileInfosForDirectory(this);
   }

   public void SetPathFileName(string newFileName)
   {
      if (string.IsNullOrWhiteSpace(newFileName))
         throw new ArgumentException("File name cannot be null or empty.", nameof(newFileName));

      LocalPath[^1] = newFileName;
   }

   public string FilePath => Path.Combine(LocalPath);
   public bool IsMultithreadable { get; }
   public static FileDescriptor Empty { get; } = new([],
                                                     FileTypeInformation.Default,
                                                     [],
                                                     false);

   public override string ToString() => $"FileDescriptor: {FilePath}";

   public override bool Equals(object? obj)
   {
      if (obj is not FileDescriptor other)
         return false;

      return FilePath == other.FilePath;
   }

   public override int GetHashCode()
   {
      return FilePath.GetHashCode();
   }
}