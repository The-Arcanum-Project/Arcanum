using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.UI.Saving.Backend;

public class Eu5FileComparer : IComparer<Eu5FileObj>
{
    public int Compare(Eu5FileObj? x, Eu5FileObj? y)
    {
        return NaturalStringComparer.Compare(x?.Path.RelativePath, y?.Path.RelativePath);
    }
}

public class FileDescriptorSavingWrapper : ISearchable
{
    private readonly FileDescriptor _descriptor;

    private readonly List<FileSavingWrapper> _newFiles = [];

    public readonly List<Eu5FileObj> AllFiles;

    public FileDescriptorSavingWrapper(FileDescriptor descriptor)
    {
        AllFiles = descriptor.Files;
        AllFiles.Sort(new Eu5FileComparer());
        
        _descriptor = descriptor;
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

    public string ResultName  => _descriptor.Name;
    public List<string> SearchTerms => [ResultName];
    public void OnSearchSelected()
    {
        throw new NotImplementedException();
    }

    public ISearchResult VisualRepresentation => throw new NotImplementedException();
    public IQueastorSearchSettings.Category SearchCategory => IQueastorSearchSettings.Category.None;
}