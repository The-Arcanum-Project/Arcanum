namespace Arcanum.API.UtilServices.Search;

public interface IQueastor
{
   public IQueastorSearchSettings Settings { get; set; }
   public void AddToIndex(ISearchable item);
   public void RemoveFromIndex(ISearchable item);
   public void ModifyInIndex(ISearchable item, IReadOnlyList<string> oldTerms);

   public List<ISearchable> Search(string query);
   public List<ISearchable> SearchExact(string query);
}