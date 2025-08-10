using Arcanum.Core.CoreSystems.SavingSystem.Services;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.Utils.Parsing.ParsingStep;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.SavingSystem.Util;

public class FileDescriptor(
   FileDescriptor[] dependencies,
   string[] localPath,
   ISavingService savingService,
   FileTypeInformation fileType,
   SingleFileLoadingBase loadingService) : IDependencyNode<string>
{
   public readonly string[] LocalPath = localPath;
   public readonly FileDescriptor[] DescriptorDependencies = dependencies;
   public readonly ISavingService SavingService = savingService;
   public readonly FileTypeInformation FileType = fileType;
   public readonly SingleFileLoadingBase SingleFileLoading = loadingService;

   public readonly List<FileObj> Files = [];

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
}