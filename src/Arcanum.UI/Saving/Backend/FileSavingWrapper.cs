using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.UI.Saving.Backend;

public class Eu5ObjectComparer : IComparer<IEu5Object>
{
   public int Compare(IEu5Object? x, IEu5Object? y)
   {
      return NaturalStringComparer.Compare(x?.ResultName, y?.ResultName);
   }
}

public class FileSavingWrapper : ISearchable
{
   public readonly Eu5FileObj FileObj;

   public readonly List<IEu5Object> AllObjects;

   public List<IEu5Object> AddedObjects { get; } = [];
   public List<IEu5Object> TransferredObjects { get; } = [];

   public FileSavingWrapper(Eu5FileObj fileObj)
   {
      AllObjects = new(fileObj.ObjectsInFile);
      AllObjects.Sort(new Eu5ObjectComparer());
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
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.None;

   public void AddObject(IEu5Object obj)
   {
      if (!TransferredObjects.Remove(obj))
         AddedObjects.Add(obj);

      var binarySearch = AllObjects.BinarySearch(obj, new Eu5ObjectComparer());
      if (binarySearch < 0)
         binarySearch = ~binarySearch;
      AllObjects.Insert(binarySearch, obj);
   }

   private void RemoveObject(IEu5Object obj)
   {
      if (!AddedObjects.Remove(obj))
         TransferredObjects.Add(obj);
      AllObjects.Remove(obj);
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