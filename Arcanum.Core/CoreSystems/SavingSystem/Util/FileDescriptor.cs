using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.SavingSystem.Util;

public class FileDescriptor : IDependencyNode<string>, IEmpty<FileDescriptor>
{
   public string[] LocalPath { get; }
   public readonly FileDescriptor[] DescriptorDependencies;
   public readonly FileTypeInformation FileType;
   public FileLoadingService LoadingService { get; }
   public readonly bool AllowMultipleInstances;

   /// <summary>
   /// this is use for cases like the default.map file which are loaded in several steps.
   /// But each step needs it's own identifier and hasValue to be sorted for loading so we can configure this to make it happen.
   /// </summary>
   public readonly char UniqueId;

   public List<Eu5FileObj> Files { get; private set; }

   public FileDescriptor(FileDescriptor[] dependencies,
                         string[] localPath,
                         FileTypeInformation fileType,
                         FileLoadingService loadingServiceService,
                         bool isMultithreadable,
                         bool allowMultipleInstances = true,
                         char uniqueId = 'G')
   {
      LocalPath = localPath;
      DescriptorDependencies = dependencies;
      FileType = fileType;
      LoadingService = loadingServiceService;
      AllowMultipleInstances = allowMultipleInstances;
      IsMultithreadable = isMultithreadable;
      UniqueId = uniqueId;

      Files = FileManager.GetAllFileInfosForDirectory(this);
   }

   public void SetPathFileName(string newFileName)
   {
      if (string.IsNullOrWhiteSpace(newFileName))
         throw new ArgumentException("File name cannot be null or empty.", nameof(newFileName));

      LocalPath[^1] = newFileName;
      Files = FileManager.GetAllFileInfosForDirectory(this);
   }

   public string FilePath => $"{UniqueId}:{string.Join("/", LocalPath)}";

   public string Id => FilePath;
   public TimeSpan LastTotalLoadingDuration { get; set; } = TimeSpan.Zero;
   public bool SuccessfullyLoaded { get; set; } = false;
   public bool IsMultithreadable { get; }

   /// <summary>
   /// Points to other file descriptors that this file depends on.
   /// This is used to ensure that the files are loaded in the correct order.
   /// </summary>
   public IEnumerable<IDependencyNode<string>> Dependencies => DescriptorDependencies;
   public static FileDescriptor Empty { get; } = new([],
                                                     [],
                                                     FileTypeInformation.Default,
                                                     null!,
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