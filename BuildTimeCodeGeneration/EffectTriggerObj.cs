namespace BuildTimeCodeGeneration;

public class EffectTriggerObj(string name)
{
   public string Name { get; } = name;
   public string Description { get; set; } = string.Empty;
   public string Usage { get; set; } = string.Empty;
   public bool ReadsGameStateForAllScopes { get; set; }
   public string[] Traits { get; set; } = [];
   public ScopeType[] Scopes { get; set; } = [];
   public ScopeType[] Targets { get; set; } = [];

   public EffectTriggerObj(string name,
                           string description,
                           string usage,
                           bool readsGameStateForAllScopes,
                           string[] traits,
                           ScopeType[] scopes,
                           ScopeType[] targets)
      : this(name)
   {
      Description = description;
      Usage = usage;
      ReadsGameStateForAllScopes = readsGameStateForAllScopes;
      Traits = traits;
      Scopes = scopes;
      Targets = targets;
   }
}