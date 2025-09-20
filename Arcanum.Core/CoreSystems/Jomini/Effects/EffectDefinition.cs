using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.Jomini.Scopes;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Jomini.Effects;

public partial class EffectDefinition(string name) : INUI, ICollectionProvider<EffectDefinition>
{
   public ModifierType ModifierType { get; set; } = ModifierType.ScriptedValue;
   public string Name { get; set; } = name;
   public string Description { get; set; } = string.Empty;
   public string Usage { get; set; } = string.Empty;
   public bool ReadsGameStateForAllScopes { get; set; }
   public ObservableRangeCollection<string> Traits { get; set; } = [];
   public ObservableRangeCollection<ScopeType> Scopes { get; set; } = [];
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
   public static IEnumerable<EffectDefinition> GetGlobalItems() => EffectRegistry.Effects.Values;

   public override string ToString() => Name;
}