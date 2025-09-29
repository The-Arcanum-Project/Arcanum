using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.UI.Saving.Backend;

public class FileSavingWrapper : ISearchable
{
    public readonly Eu5FileObj FileObj;

    private List<IEu5Object> AddedObjects { get; set; } = [];
    private List<IEu5Object> TransferredObjects { get; set; } = [];
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

    public void AddObject(IEu5Object obj)
    {
        if (!TransferredObjects.Remove(obj))
            AddedObjects.Add(obj);
    }

    private void RemoveObject(IEu5Object obj)
    {
        if(!AddedObjects.Remove(obj))
            TransferredObjects.Add(obj);
    }

    public void TransferObjectTo(IEu5Object obj, FileSavingWrapper targetFile)
    {
        TransferObject(obj, this, targetFile);
    }

    private static void TransferObject(IEu5Object obj, FileSavingWrapper sourceFile, FileSavingWrapper targetFile)
    {
        sourceFile.RemoveObject(obj);
        targetFile.AddObject(obj);
    }
}