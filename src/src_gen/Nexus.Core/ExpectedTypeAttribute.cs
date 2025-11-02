namespace Nexus.Core;

/// <summary>
/// Marks the corresponding type to the enum value
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class ExpectedTypeAttribute(Type type) : Attribute
{
   public Type Type { get; } = type;
}