#region

using System.Runtime.CompilerServices;
using Arcanum.Core.Utils;

#endregion

namespace Arcanum.UI.AppFeatures;

public record FeatureId
{
   public FeatureId(string value) => Value = value.ToLower();

   // IDs self-construct based on its location in the code
   public static FeatureId Create(string path, [CallerMemberName] string name = "") => new($"{path}.{name.ToSnakeCase()}");
   public override string ToString() => Value;
   public static implicit operator string(FeatureId id) => id.Value;
   public override int GetHashCode() => Value.GetHashCode();
   public virtual bool Equals(FeatureId? other) => other is not null && GetHashCode() == other.GetHashCode();
   public string Value { get; init; }
}