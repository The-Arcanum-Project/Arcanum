using System.IO;

namespace BuildTimeCodeGeneration;

public static class DocumentationParser
{
   public static List<EffectTriggerObj> ParseDocs(string path)
   {
      var lines = File.ReadAllLines(path);
      if (lines == null! || lines.Length == 0)
         return [];

      var triggerList = new List<EffectTriggerObj>();
      EffectTriggerObj? currentTrigger = null;

      foreach (var line in lines)
      {
         if (line.StartsWith("##"))
         {
            if (currentTrigger != null)
               triggerList.Add(currentTrigger);
            currentTrigger = new(line[2..].Trim());
            continue;
         }

         if (currentTrigger == null)
            continue;

         if (line.StartsWith("Traits:"))
         {
            currentTrigger.Traits = line[7..].Trim().Split(',');
            continue;
         }

         if (line.StartsWith("**Supported Scopes**:"))
         {
            currentTrigger.Scopes = ParseScopeTypes(line[22..].Trim()).ToArray();
            continue;
         }

         if (line.StartsWith("**Supported Targets**:"))
         {
            currentTrigger.Targets = ParseScopeTypes(line[22..].Trim()).ToArray();
            continue;
         }

         if (line.StartsWith(currentTrigger.Name))
         {
            currentTrigger.Usage = line.Replace(',', ';').Trim();
            continue;
         }

         if (line.StartsWith("Reads gamestate"))
         {
            currentTrigger.ReadsGameStateForAllScopes = true;
            continue;
         }

         if (currentTrigger.Description == string.Empty)
            currentTrigger.Description = line.Replace(',', ';').Trim();
         else
            currentTrigger.Description += ' ' + line.Replace(',', ';').Trim();
      }

      if (currentTrigger != null)
         triggerList.Add(currentTrigger);

      return triggerList;
   }

   public static HashSet<ScopeDefinition> ParseScopesDocumentationFile(string filePath)
   {
      var content = File.ReadAllText(filePath);
      var definitions = new HashSet<ScopeDefinition>();

      // Split the file into blocks based on the "###" delimiter
      var blocks = content.Split(["### "], StringSplitOptions.RemoveEmptyEntries);

      foreach (var block in blocks)
      {
         var trimmedBlock = block.Trim();
         if (string.IsNullOrWhiteSpace(trimmedBlock) || trimmedBlock.StartsWith('#'))
            continue;

         var lines = trimmedBlock.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
         var definition = new ScopeDefinition(lines[0].Trim())
         {
            Description = lines.Length > 1 ? lines[1].Trim() : "No description provided.",
         };

         // Parse remaining lines for specific properties
         foreach (var line in lines.Skip(2))
            if (line.StartsWith("Requires Data:"))
            {
               definition.RequiresData = line.Contains("yes", StringComparison.OrdinalIgnoreCase);
            }
            else if (line.StartsWith("Input Scopes:"))
            {
               var scopeStr = line["Input Scopes:".Length..].Trim();
               definition.InputType = ParseScopeTypes(scopeStr);
            }
            else if (line.StartsWith("Output Scopes:"))
            {
               var scopeStr = line["Output Scopes:".Length..].Trim();
               definition.OutputType = ParseScopeTypes(scopeStr);
            }

         definitions.Add(definition);
      }

      return definitions;
   }

   public static readonly HashSet<string> UnknowScopeTypes = [];

   private static HashSet<ScopeType> ParseScopeTypes(string scopeString)
   {
      // It now returns a collection
      var resultSet = new HashSet<ScopeType>();
      var parts = scopeString.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

      foreach (var part in parts)
      {
         var pascalCasePart = PascalCase(part);

         if (Enum.TryParse<ScopeType>(pascalCasePart, out var parsedScope))
            resultSet.Add(parsedScope);
         else
            UnknowScopeTypes.Add(part);
      }

      return resultSet;
   }

   public static ScopeType ParseSingleScopeType(string scopeString)
   {
      var trimmed = scopeString.Trim();
      var pascalCase = PascalCase(trimmed);

      if (Enum.TryParse<ScopeType>(pascalCase, out var parsedScope))
         return parsedScope;

      throw new ArgumentException($"Unknown scope type: '{scopeString}'");
   }

   private static string PascalCase(string trimmed)
   {
      return string.Concat(trimmed.Split('_').Select(s => char.ToUpper(s[0]) + s[1..]));
   }
}