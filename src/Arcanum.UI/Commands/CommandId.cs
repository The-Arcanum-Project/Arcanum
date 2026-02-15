using System.Runtime.CompilerServices;

namespace Arcanum.UI.Commands;

public record CommandId
{
   public string Value { get; }

   private CommandId(string value) => Value = value;

   // IDs self-construct based on its location in the code
   public static CommandId Create(string path, [CallerMemberName] string name = "") => new($"{path}.{name}".ToLowerInvariant());

   public override string ToString() => Value;
   public static implicit operator string(CommandId id) => id.Value;
}