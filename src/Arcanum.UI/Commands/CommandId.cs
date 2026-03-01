using System.Runtime.CompilerServices;
using Arcanum.Core.Utils;

namespace Arcanum.UI.Commands;

public record CommandId(string Value)
{
   // IDs self-construct based on its location in the code
   public static CommandId Create(string path, [CallerMemberName] string name = "") => new($"{path}.{name.ToSnakeCase()}");

   public override string ToString() => Value;
   public static implicit operator string(CommandId id) => id.Value;
}