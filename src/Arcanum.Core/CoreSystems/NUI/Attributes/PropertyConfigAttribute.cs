using Arcanum.Core.GameObjects.BaseTypes;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.NUI.Attributes;

/// <summary>
/// If a property is marked with this attribute, it will be displayed as read-only in the NUI.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class PropertyConfigAttribute(bool isReadonly = false,
                                     bool isInlined = false,
                                     bool allowEmpty = true,
                                     bool disableMapInferButtons = false,
                                     bool isRequired = false,
                                     double minValue = double.MinValue,
                                     double maxValue = double.MaxValue,
                                     string defaultValueMethod = "",
                                     bool ignoreCommand = false,
                                     AggregateLinkType aggreateLinkType = AggregateLinkType.None,
                                     string? aggregateLinktParent = null) : Attribute
{
   /// <summary>
   /// Whether the property is read-only in the NUI.
   /// </summary>
   public bool IsReadonly { get; set; } = isReadonly;

   /// <summary>
   /// Whether the property should be inlined in the NUI. <br/>
   /// Only works on <see cref="IEu5Object"/>s.
   /// </summary>
   public bool IsInlined { get; set; } = isInlined;

   /// <summary>
   /// Whether empty values are allowed for this property in the NUI.
   /// </summary>
   public bool AllowEmpty { get; set; } = allowEmpty;

   /// <summary>
   /// Whether to disable map infer buttons for this property in the NUI.
   /// </summary>
   public bool DisableMapInferButtons { get; set; } = disableMapInferButtons;

   /// <summary>
   /// Whether this property is required to have a value in the NUI.
   /// </summary>
   public bool IsRequired { get; set; } = isRequired;

   /// <summary>
   /// The minimum value allowed for this property in the NUI.
   /// </summary>
   public double MinValue { get; set; } = minValue;

   /// <summary>
   /// The maximum value allowed for this property in the NUI.
   /// </summary>
   public double MaxValue { get; set; } = maxValue;

   /// <summary>
   /// The name of a static method that provides the default value for this property.
   /// </summary>
   public string DefaultValueMethod { get; set; } = defaultValueMethod;

   public bool IgnoreCommand { get; set; } = ignoreCommand;

   public AggregateLinkType AggregateLinkType { get; set; } = aggreateLinkType;

   public string? AggregateLinkParent { get; set; } = aggregateLinktParent;
}