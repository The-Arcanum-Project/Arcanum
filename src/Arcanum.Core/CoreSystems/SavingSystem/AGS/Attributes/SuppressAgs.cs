namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

/// <summary>
/// Attribute to suppress saving a property using AGS (Automatic Generative Saving).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SuppressAgs : Attribute;