using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.UI.Saving.Backend;

public class Eu5FileComparer : IComparer<Eu5FileObj>
{
    public int Compare(Eu5FileObj? x, Eu5FileObj? y)
    {
        return x switch
        {
            null when y is null => 0,
            null => -1,
            _ => y is null
                ? 1
                : string.Compare(x.Path.RelativePath, y.Path.RelativePath, StringComparison.OrdinalIgnoreCase)
        };
    }
}

public class FileDescriptorSavingWrapper : ISearchable
{
    public readonly FileDescriptor Descriptor;

    private readonly List<FileSavingWrapper> _newFiles = [];

    public readonly List<Eu5FileObj> AllFiles;

    public FileDescriptorSavingWrapper(FileDescriptor descriptor, SavingWrapperManager manager)
    {
        AllFiles = descriptor.Files;
        AllFiles.Sort(new Eu5FileComparer());
        
        Descriptor = descriptor;
    }

    public void AddNewFile(FileSavingWrapper file)
    {
        _newFiles.Add(file);
        var binarySearch = AllFiles.BinarySearch(file.FileObj, new Eu5FileComparer());
        if (binarySearch < 0)
            binarySearch = ~binarySearch;
        AllFiles.Insert(binarySearch, file.FileObj);
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