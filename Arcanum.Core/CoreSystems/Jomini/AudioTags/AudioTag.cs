using System.ComponentModel;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.Jomini.AudioTags;

[ObjectSaveAs]
public partial class AudioTag : INUI, IEmpty<AudioTag>, IModifierPattern
{
   [ReadonlyNexus]
   [DefaultValue("null")]
   [Description("Unique key of this object. Must be unique among all objects of this type.")]
   public string UniqueId { get; set; } = null!;

   [Description("The factor by which this audio tag influences sound effects. Default is 1.0.")]
   [DefaultValue(1f)]
   public object Value { get; set; } = 1f;
   [Description("The type of modifier this audio tag represents.")]
   [DefaultValue(ModifierType.Float)]
   public ModifierType Type { get; set; } = ModifierType.Float;

   public bool IsReadonly => true;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.AudioTagSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static AudioTag Empty { get; } = new() { UniqueId = "Arcanum_empty_audio_tag", Value = 1f };

   public override string ToString() => UniqueId;

   protected bool Equals(AudioTag other) => UniqueId == other.UniqueId && Value.Equals(other.Value);

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;
      if (obj.GetType() != GetType())
         return false;

      return Equals((AudioTag)obj);
   }

   // ReSharper disable twice NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => HashCode.Combine(UniqueId, Value);
}