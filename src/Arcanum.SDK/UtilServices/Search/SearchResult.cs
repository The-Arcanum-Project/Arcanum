namespace Arcanum.API.UtilServices.Search;

public struct SearchResult(string key, ISearchable value) : IComparable<SearchResult>
{
   public readonly string Key = key;
   public readonly ISearchable Value = value;

   public override string ToString() => $"{Key} ({Value.GetType().Name})";

   public override int GetHashCode() => Key.GetHashCode() ^ Value.GetHashCode();

   public int CompareTo(SearchResult other) => string.Compare(Key, other.Key, StringComparison.Ordinal);
}