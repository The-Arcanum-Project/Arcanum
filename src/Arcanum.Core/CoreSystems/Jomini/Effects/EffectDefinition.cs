using System.ComponentModel;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.Jomini.Scopes;
using Arcanum.Core.CoreSystems.NUI;

namespace Arcanum.Core.CoreSystems.Jomini.Effects;

public partial class EffectDefinition(string name) : INUI, ICollectionProvider<EffectDefinition>
{
   [Description("The type of modifier this effect represents.")]
   [DefaultValue(ModifierType.ScriptedValue)]
   public ModifierType ModifierType { get; set; } = ModifierType.ScriptedValue;

   [Description("Unique name of this effect definition. Must be unique among all objects of this type.")]
   [DefaultValue("")]
   public string Name { get; set; } = name;

   [Description("A description of what this effect does.")]
   [DefaultValue("")]
   public string Description { get; set; } = string.Empty;

   [Description("Instructions on how to use this effect.")]
   [DefaultValue("")]
   public string Usage { get; set; } = string.Empty;

   [Description("Whether this effect reads the game state for all scopes when applied.")]
   [DefaultValue(false)]
   public bool ReadsGameStateForAllScopes { get; set; }

   [Description("Traits associated with this effect.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<string> Traits { get; set; } = [];

   [Description("Scopes where this effect can be applied.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<ScopeType> Scopes { get; set; } = [];

   [Description("Targets that this effect can affect.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<ScopeType> Targets { get; set; } = [];

   public EffectDefinition(string name,
                           string description,
                           string usage,
                           bool readsGameStateForAllScopes,
                           List<string> traits,
                           List<ScopeType> scopes,
                           List<ScopeType> targets)
      : this(name)
   {
      Description = description;
      Usage = usage;
      ReadsGameStateForAllScopes = readsGameStateForAllScopes;
      Traits.AddRange(traits);
      Scopes.AddRange(scopes);
      Targets.AddRange(targets);
   }

   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.EffectDefinitionSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Dictionary<string, EffectDefinition> GetGlobalItems() => EffectRegistry.Effects;

   public override string ToString() => Name;
}