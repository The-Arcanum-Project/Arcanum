namespace Nexus.Core;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class AddModifiableAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class IgnoreModifiableAttribute : Attribute;