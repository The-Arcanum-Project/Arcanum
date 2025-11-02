namespace BuildTimeCodeGeneration;

public class ScopeDefinition(string key)
{
   public string Key { get; } = key;
   public string Description { get; set; } = "No description provided.";
   public bool RequiresData { get; set; }
   public IReadOnlySet<ScopeType> InputType { get; set; } = new HashSet<ScopeType>();
   public IReadOnlySet<ScopeType> OutputType { get; set; } = new HashSet<ScopeType>();

   public override int GetHashCode() => Key.GetHashCode();
   public override bool Equals(object? obj) => obj is ScopeDefinition other && other.Key == Key;
}