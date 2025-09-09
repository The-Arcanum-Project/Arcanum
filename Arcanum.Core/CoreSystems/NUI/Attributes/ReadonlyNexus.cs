namespace Arcanum.Core.CoreSystems.NUI.Attributes;

/// <summary>
/// If a property is marked with this attribute, it will be displayed as read-only in the NUI.
/// </summary>

[AttributeUsage(AttributeTargets.Property)]
public class ReadonlyNexusAttribute : Attribute;