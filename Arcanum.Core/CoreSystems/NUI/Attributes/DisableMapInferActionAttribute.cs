namespace Arcanum.Core.CoreSystems.NUI.Attributes;

/// <summary>
/// When applied to an INUI property that is a collection, this attribute prevents
/// the NUIViewGenerator from creating the map inference action buttons for it.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DisableMapInferActionsAttribute : Attribute;