using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.NUI.Attributes;

/// <summary>
/// If a property is marked with this attribute, it will be displayed as read-only in the NUI.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class NuiConfigAttribute(bool isReadonly = false,
                                bool isInlined = false,
                                bool allowEmpty = true,
                                bool disableMapInferButtons = false) : Attribute
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
}