using System.Collections.ObjectModel;
using System.IO;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Queastor;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;

namespace Arcanum.UI.Saving.Backend;

public class FileRepresentation : ISearchable
{
    private readonly Queastor _queastor;
    public readonly Eu5FileObj FileObj;

    public Eu5ObjectsRegistry.Eu5ObjectsEnum[] AllowedObjects;
    
    public string Path => FileObj.Path.RelativePath;
    
    public ObservableCollection<IEu5Object> ChangedObjects { get; set; }

    public FileRepresentation(Queastor queastor, Eu5FileObj fileObj, List<IEu5Object> changedObjects)
    {
        ChangedObjects = new(changedObjects);
        _queastor = queastor;
        FileObj = fileObj;
        AllowedObjects = fileObj.Descriptor.LoadingService[0].ParsedObjects.Select(c =>
        {
            if(Eu5ObjectsRegistry.TryGetEnumRepresentation(c, out var objEnum))
                return objEnum;
            throw new InvalidOperationException("The object type was not registered in the Eu5ObjectsRegistry");
        }
            ).Distinct().ToArray();
        fileObj.Path.AddSearchTerms(SearchTerms);
        _queastor.AddToIndex(this);
        SearchTerms = [];
    }
    
    public string GetNamespace { get; } = "";
    public string ResultName => FileObj.Path.Filename;
    public List<string> SearchTerms { get; } = [];
    public void OnSearchSelected()
    {
    }

    public void AddSaveable(IAgs item)
    {
        if (item is IEu5Object eu5Object)
            _queastor.AddToIndex(this,eu5Object.UniqueId);
        else
            _queastor.AddToIndex(this, item.SavingKey);
    }
    
    public void RemoveSaveable(IAgs item)
    {
        if (item is IEu5Object eu5Object)
            _queastor.RemoveFromIndex(this,eu5Object.UniqueId);
        else
            _queastor.RemoveFromIndex(this, item.SavingKey);
    }
    
    public ISearchResult VisualRepresentation => throw new NotImplementedException();
    public IQueastorSearchSettings.Category SearchCategory => IQueastorSearchSettings.Category.None;
}