using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.UI.Saving.Backend;

public class SavingWrapperManager
{
    private Dictionary<FileDescriptor, FileDescriptorSavingWrapper> Descriptors { get; } = [];
    private Dictionary<Eu5FileObj, FileSavingWrapper> Files { get; } = [];

    public FileSavingWrapper GetFile(Eu5FileObj fileObj)
    {
        if (!Files.TryGetValue(fileObj, out var wrapper))
        {
            Files[fileObj] = wrapper = new(fileObj);
            Debug.WriteLine($"Added file {fileObj.Path.RelativePath}: Total files: {Files.Count}");
        }

        return wrapper;
    }

    public bool TryGetFile(Eu5FileObj fileObj, [NotNullWhen(true)] out FileSavingWrapper? wrapper)
    {
        return Files.TryGetValue(fileObj, out wrapper);
    }

    public List<FileSavingWrapper> GetFiles(List<Eu5FileObj> files)
    {
        return files.Select(GetFile).ToList();
    }

    public FileDescriptorSavingWrapper GetDescriptor(FileDescriptor descriptor)
    {
        if (!Descriptors.TryGetValue(descriptor, out var wrapper))
        {
            Descriptors[descriptor] = wrapper = new(descriptor);
            Debug.WriteLine($"Added Descriptor {descriptor.FilePath}: Total Descriptors: {Descriptors.Count}");
        }

        return wrapper;
    }

    private bool TryGetDescriptor(FileDescriptor descriptor,
        [NotNullWhen(true)] out FileDescriptorSavingWrapper? wrapper)
    {
        return Descriptors.TryGetValue(descriptor, out wrapper);
    }

    public List<FileDescriptorSavingWrapper> GetDescriptors(List<FileDescriptor> descriptors)
    {
        return descriptors.Select(GetDescriptor).ToList();
    }

    public List<Eu5FileObj> GetAllFiles(FileDescriptor descriptor)
    {
        if (TryGetDescriptor(descriptor, out var wrapper))
            return wrapper.AllFiles;
        var files = new List<Eu5FileObj>(descriptor.Files);
        files.Sort(new Eu5FileComparer());
        return files;
    }
    
    public List<Eu5FileObj> GetAllRelevantFiles(FileDescriptor descriptor, HashSet<Eu5FileObj> relevantFiles)
    {
        return GetAllFiles(descriptor).Where(f => relevantFiles.Contains(f) || Files.ContainsKey(f)).ToList();
    }

    
    public List<IEu5Object> GetAllObjects(Eu5FileObj fileObj)
    {
        if (TryGetFile(fileObj, out var wrapper))
            return wrapper.AllObjects;
        var files = new List<IEu5Object>(fileObj.ObjectsInFile);
        files.Sort(new Eu5ObjectComparer());
        return files;
    }
    
    public List<IEu5Object> GetAllRelevantObjects(Eu5FileObj fileObj,  HashSet<IEu5Object> relevantObjects)
    {
        return GetAllObjects(fileObj).Where(relevantObjects.Contains).ToList();
    }
}