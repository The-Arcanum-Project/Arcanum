using System.ComponentModel;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.Jomini.AiTags;

[ObjectSaveAs]
public partial class AiTag : INUI, IEmpty<AiTag>, IModifierPattern
{
   [ReadonlyNexus]
   [DefaultValue("null")]
   [Description("Unique key of this object. Must be unique among all objects of this type.")]
   public string UniqueId { get; set; } = null!;

   [Description("The factor by which the AI will modify its behavior.")]
   public object Value { get; set; } = 1f;
   public ModifierType Type { get; set; } = ModifierType.Float;

   public bool IsReadonly => true;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.AiTagSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static AiTag Empty { get; } = new() { UniqueId = "Arcanum_empty_ai_tag", Value = 1f };

   public override string ToString() => UniqueId;

   protected bool Equals(AiTag other) => UniqueId == other.UniqueId && Value.Equals(other.Value);

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;
      if (obj.GetType() != GetType())
         return false;

      return Equals((AiTag)obj);
   }

   // ReSharper disable twice NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => HashCode.Combine(UniqueId, Value);
}