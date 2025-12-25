namespace Arcanum.API.Console;

public interface ICommandDefinition
{
   string Name { get; }
   string Usage { get; }
   ClearanceLevel Clearance { get; }
   IReadOnlyList<string> Aliases { get; }
   Func<string[], string[]> Execute { get; }
   Func<string[], string[]>? GetSuggestions { get; }
}

public abstract class CommandBase : ICommandDefinition
{
   public string Name { get; protected init; }
   public string Usage { get; protected init; }
   public ClearanceLevel Clearance { get; protected init; }
   public IReadOnlyList<string> Aliases { get; protected init; }
   public Func<string[], string[]> Execute { get; protected init; }
   public Func<string[], string[]>? GetSuggestions { get; protected init; }

   protected CommandBase(string name,
                         string usage,
                         ClearanceLevel clearance,
                         IReadOnlyList<string> aliases,
                         Func<string[], string[]> execute,
                         Func<string[], string[]>? getSuggestions = null)
   {
      Name = name;
      Usage = usage;
      Clearance = clearance;
      Aliases = aliases;
      Execute = execute;
      GetSuggestions = getSuggestions;
   }

   public override bool Equals(object? obj)
   {
      if (obj is not ICommandDefinition other)
         return false;

      return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
   }

   public override int GetHashCode()
   {
      return Name.GetHashCode(StringComparison.OrdinalIgnoreCase);
   }
}