namespace Arcanum.Core.CoreSystems.NUI.Attributes;

/// <summary>
/// Specifies the format string to be used when calling <c>ToString()</c> on a property's value for UI display.
/// The property's type must implement <see cref="IFormattable"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ToStringArgumentsAttribute(string format) : Attribute
{
   /// <summary>
   /// The format string (e.g., "X", "F2", "yyyy-MM-dd").
   /// </summary>
   public string Format { get; } = format;
}