namespace Arcanum.Core.CoreSystems.Parsing.DocsParsing;

public class DocsObj(string name)
{
   public string Name { get; } = name;
   public string Description { get; set; } = string.Empty;
   public string Usage { get; set; } = string.Empty;
   public bool ReadsGameStateForAllScopes { get; set; } = false;
   public string[] Traits { get; set; } = [];
   public string[] Scopes { get; set; } = [];
   public string[] Targets { get; set; } = [];
   
   public string TraitsString => string.Join(", ", Traits);
   public string ScopesString => string.Join(", ", Scopes);
   public string TargetsString => string.Join(", ", Targets);

   public string ToCsv()
   {
      return $"{Name},{Description},{Usage},{ReadsGameStateForAllScopes.ToString().ToLower()}" +
             $",{string.Join(";", Traits)},{string.Join(";", Scopes)},{string.Join(";", Targets)}";
   }

   public static string GetCsvHeader()
   {
      return "Name,Description,Usage,ReadsGameStateForAllScopes,Traits,Scopes,Targets";
   }
}