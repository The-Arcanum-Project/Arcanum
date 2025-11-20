using System.ComponentModel;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;
using Nexus.Core.Attributes;

namespace Arcanum.Core.CoreSystems.Jomini.AiTags;

[NexusConfig]
[ObjectSaveAs]
public partial class AiTag : INUI, IEmpty<AiTag>, IModifierPattern
{
   [DefaultValue("null")]
   [Description("Unique key of this object. Must be unique among all objects of this type.")]
   public string UniqueId { get; set; } = null!;

   [DefaultValue(1f)]
   [Description("The factor by which the AI will modify its behavior.")]
   public object Value { get; set; } = 1f;

   [DefaultValue(ModifierType.Float)]
   [Description("The type of modifier this AI tag represents.")]
   public ModifierType Type { get; set; } = ModifierType.Float;

   public bool IsReadonly => true;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.AiTagSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static AiTag Empty { get; } = new() { UniqueId = "Arcanum_empty_ai_tag", Value = 1f };
}