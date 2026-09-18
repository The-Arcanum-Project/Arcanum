#region

using System.Runtime.CompilerServices;
using Arcanum.Core.Utils;

#endregion

namespace Arcanum.UI.Commands;

public record CommandId

{
   public CommandId(string value) => Value = value.ToLower();

   // IDs self-construct based on its location in the code
   public static CommandId Create(string path, [CallerMemberName] string name = "") => new($"{path}.{name.ToSnakeCase()}".ToLower());

   public override string ToString() => Value;
   public static implicit operator string(CommandId id) => id.Value;
   public string Value { get; }
}