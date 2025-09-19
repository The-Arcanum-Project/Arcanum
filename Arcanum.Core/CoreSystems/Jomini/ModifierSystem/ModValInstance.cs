using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.Common;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Jomini.ModifierSystem;

/// <summary>
/// An instance of a modifier definition with an associated value.
/// </summary>
public partial class ModValInstance : INUI, IEmpty<ModValInstance>, IModifierPattern
{
   /// <summary>
   /// An instance of a modifier definition with an associated value.
   /// </summary>
   /// <param name="definition"></param>
   /// <param name="value"></param>
   /// <param name="type"></param>
   [Obsolete("Use the ModifierManager to create instances.")]
   public ModValInstance(ModifierDefinition definition, object value, ModifierType type)
   {
      Definition = definition;
      Value = value;
      Type = type;
   }

   /// <summary>
   /// The definition of the modifier.
   /// </summary>
   public ModifierDefinition Definition { get; set; }

   public string UniqueId
   {
      get => Definition.UniqueKey;
      set => Definition.UniqueKey = value;
   }
   /// <summary>
   /// The value of the modifier.
   /// </summary>
   public object Value { get; set; }
   /// <summary>
   /// The type of the modifier, inferred from the definition.
   /// </summary>
   public ModifierType Type { get; set; }

   public override string ToString()
   {
      return $"{Definition.UniqueKey} : {Value} ({Type})";
   }

   public bool IsReadonly => false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.ModValInstanceSettings;
   public INUINavigation[] Navigations { get; } = [];
#pragma warning disable CS0618 // Type or member is obsolete
   public static ModValInstance Empty { get; } = new(ModifierDefinition.Empty, 0, ModifierType.Integer);
#pragma warning restore CS0618 // Type or member is obsolete
}