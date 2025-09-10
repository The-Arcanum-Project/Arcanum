namespace Arcanum.Core.CoreSystems.NUI.Attributes;

/// <summary>
/// If this attribute is applied to a property, the NUI will not show empty values setter
/// for this property if it is embedded.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class BlockEmptyAttribute : Attribute;