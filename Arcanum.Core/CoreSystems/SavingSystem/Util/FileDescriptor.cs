using Arcanum.Core.CoreSystems.SavingSystem.Services;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.Utils.Parsing.ParsingStep;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.SavingSystem.Util;

public class FileDescriptor : IDependencyNode<string>
{
   public readonly string[] LocalPath;
   public readonly FileDescriptor[] DescriptorDependencies;
   public readonly ISavingService SavingService;
   public readonly FileTypeInformation FileType;
   public readonly SingleFileLoadingBase SingleFileLoading;
   public readonly bool AllowMultipleInstances;

   public List<FileObj> Files;

   public FileDescriptor(FileDescriptor[] dependencies,
                         string[] localPath,
                         ISavingService savingService,
                         FileTypeInformation fileType,
                         SingleFileLoadingBase loadingService,
                         bool allowMultipleInstances = true)
   {
      LocalPath = localPath;
      DescriptorDependencies = dependencies;
      SavingService = savingService;
      FileType = fileType;
      SingleFileLoading = loadingService;
      AllowMultipleInstances = allowMultipleInstances;
      
      Files = FileManager.GetAllFileInfosForDirectory(this);
   }

   public string GetFilePath()
   {
      return $"{string.Join("/", LocalPath)}";
   }

   public string Id => GetFilePath();
   public IEnumerable<IDependencyNode<string>> Dependencies => DescriptorDependencies;
   public static FileDescriptor Dummy { get; } = new([],
                                                     [],
                                                     ISavingService.Dummy,
                                                     FileTypeInformation.Default,
                                                     SingleFileLoadingBase.Dummy);
   
   public override string ToString() => $"FileDescriptor: {GetFilePath()}";
   
   public override bool Equals(object? obj)
   {
      if (obj is not FileDescriptor other) return false;
      return GetFilePath() == other.GetFilePath();
   }
   public override int GetHashCode()
   {
      return GetFilePath().GetHashCode();
   }
}