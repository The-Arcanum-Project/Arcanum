namespace Nexus.Core;

/// <summary>
/// Marks the enum parameter and links it to the target object parameter
/// that defines the enum type.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class LinkedPropertyEnumAttribute(string targetParameterName) : Attribute
{
   /// <summary>
   /// The name of the parameter that is the target IEnumProperty object.
   /// </summary>
   public string TargetParameterName { get; } = targetParameterName;
}

/// <summary>
/// Marks the parameter that receives the value for the property.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class PropertyValueAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class PropertyGetterAttribute : Attribute
{
}