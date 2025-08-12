using Arcanum.Core.CoreSystems.Parsing.ParsingStep;
using Arcanum.Core.CoreSystems.SavingSystem.Services;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.SavingSystem.Util;

public class FileDescriptor : IDependencyNode<string>
{
   public readonly string[] LocalPath;
   public readonly FileDescriptor[] DescriptorDependencies;
   public readonly ISavingService SavingService;
   public readonly FileTypeInformation FileType;
   public FileLoadingService LoadingService { get; }
   public readonly bool AllowMultipleInstances;

   public List<FileObj> Files;

   public FileDescriptor(FileDescriptor[] dependencies,
                         string[] localPath,
                         ISavingService savingService,
                         FileTypeInformation fileType,
                         FileLoadingService loadingServiceService,
                         bool isMultithreadable,
                         bool allowMultipleInstances = true)
   {
      LocalPath = localPath;
      DescriptorDependencies = dependencies;
      SavingService = savingService;
      FileType = fileType;
      LoadingService = loadingServiceService;
      AllowMultipleInstances = allowMultipleInstances;
      IsMultithreadable = isMultithreadable;

      Files = FileManager.GetAllFileInfosForDirectory(this);
   }

   public string GetFilePath()
   {
      return $"{string.Join("/", LocalPath)}";
   }

   public string Id => GetFilePath();
   public TimeSpan LastTotalLoadingDuration { get; set; } = TimeSpan.Zero;
   public bool SuccessfullyLoaded { get; set; } = false;
   public bool IsMultithreadable { get; }
   public IEnumerable<IDependencyNode<string>> Dependencies => DescriptorDependencies;
   public static FileDescriptor Dummy { get; } = new([],
                                                     [],
                                                     ISavingService.Dummy,
                                                     FileTypeInformation.Default,
                                                     null!,
                                                     false);

   public override string ToString() => $"FileDescriptor: {GetFilePath()}";

   public override bool Equals(object? obj)
   {
      if (obj is not FileDescriptor other)
         return false;

      return GetFilePath() == other.GetFilePath();
   }

   public override int GetHashCode()
   {
      return GetFilePath().GetHashCode();
   }
}