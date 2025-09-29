using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.UI.Saving.Backend;

public class FileDescriptorSavingWrapper : ISearchable
{
    public readonly FileDescriptor Descriptor;

    private readonly List<FileSavingWrapper> _newFiles = [];

    public readonly List<FileSavingWrapper> AllFiles;

    public FileDescriptorSavingWrapper(FileDescriptor descriptor, SavingWrapperManager manager)
    {
        AllFiles = manager.GetFiles(descriptor.Files);
        AllFiles.Sort();
        
        Descriptor = descriptor;
    }

    public void AddNewFile(FileSavingWrapper file)
    {
        _newFiles.Add(file);
        var binarySearch = AllFiles.BinarySearch(file);
        if (binarySearch < 0)
            binarySearch = ~binarySearch;
        AllFiles.Insert(binarySearch, file);
    }

    public string GetNamespace => "";

    //TODO: @MelCo: Better name for descriptors
    public string ResultName  => Descriptor.FileType.TypeName;
    public List<string> SearchTerms => [ResultName];
    public void OnSearchSelected()
    {
        throw new NotImplementedException();
    }

    public ISearchResult VisualRepresentation => throw new NotImplementedException();
    public IQueastorSearchSettings.Category SearchCategory => IQueastorSearchSettings.Category.None;
}