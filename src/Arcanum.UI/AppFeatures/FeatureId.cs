using System.Runtime.CompilerServices;
using Arcanum.Core.Utils;

namespace Arcanum.UI.AppFeatures;

public record FeatureId(string Value)
{
   // IDs self-construct based on its location in the code
   public static FeatureId Create(string path, [CallerMemberName] string name = "") => new($"{path}.{name.ToSnakeCase().ToLower()}");

   public override string ToString() => Value;
   public static implicit operator string(FeatureId id) => id.Value;
}