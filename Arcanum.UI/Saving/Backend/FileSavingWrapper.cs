using System.Collections.ObjectModel;
using System.IO;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Queastor;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;

namespace Arcanum.UI.Saving.Backend;

public class FileSavingWrapper : ISearchable, IComparable<FileSavingWrapper>
{
    public readonly Eu5FileObj FileObj;

    private string Path => FileObj.Path.RelativePath;

    public List<IEu5Object> AddedObjects { get; set; } = [];
    public List<IEu5Object> TransferredObjects { get; set; } = [];
    public FileSavingWrapper(Eu5FileObj fileObj)
    {
        FileObj = fileObj;
        fileObj.Path.AddSearchTerms(SearchTerms);
        SearchTerms = [];
    }
    
    public string GetNamespace => "";
    public string ResultName => FileObj.Path.Filename;
    public List<string> SearchTerms { get; } = [];
    public void OnSearchSelected()
    {
    }
    
    public ISearchResult VisualRepresentation => throw new NotImplementedException();
    public IQueastorSearchSettings.Category SearchCategory => IQueastorSearchSettings.Category.None;
    public int CompareTo(FileSavingWrapper? other)
    {
        return other == null ? 1 : string.Compare(Path, other.Path, StringComparison.OrdinalIgnoreCase);
    }

    public void AddObject(IEu5Object obj)
    {
        if (!TransferredObjects.Remove(obj))
            AddedObjects.Add(obj);
    }

    public void RemoveObject(IEu5Object obj)
    {
        if(!AddedObjects.Remove(obj))
            TransferredObjects.Add(obj);
    }

    public void TransferObjectTo(IEu5Object obj, FileSavingWrapper targetFile)
    {
        TransferObject(obj, this, targetFile);
    }

    public static void TransferObject(IEu5Object obj, FileSavingWrapper sourceFile, FileSavingWrapper targetFile)
    {
        sourceFile.RemoveObject(obj);
        targetFile.AddObject(obj);
    }
}