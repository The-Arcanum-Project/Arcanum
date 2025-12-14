using System.IO;
using Arcanum.Core.CoreSystems.Jomini.Scopes;
using Common.UI;

namespace Arcanum.Core.CoreSystems.Jomini.Effects;

public static class EffectParser
{
   public static string DocumentsFolder => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

   public static bool ParseEffectDefinitions()
   {
      if (EffectRegistry.Effects.Count > 0)
         return true;

      var path = Path.Combine(DocumentsFolder, @"Paradox Interactive\Europa Universalis V\docs\effects.log");
      if (!File.Exists(path))
      {
         UIHandle.Instance.PopUpHandle.ShowMBox(
            "\"effects.log\" not found. Please run the 'script_docs' command in the EUV console!", "Error");
         return false;
      }

      var lines = File.ReadAllLines(path);
      if (lines == null! || lines.Length == 0)
         return false;

      EffectDefinition? effectDef = null;

      foreach (var line in lines)
      {
         if (line.StartsWith("##"))
         {
            if (effectDef != null)
               EffectRegistry.Effects.Add(effectDef.Name, effectDef);
            effectDef = new(line[2..].Trim());
            continue;
         }

         if (effectDef == null)
            continue;

         if (line.StartsWith("Traits:"))
         {
            effectDef.Traits.AddRange(line[7..].Trim().Split(','));
            continue;
         }

         if (line.StartsWith("**Supported Scopes**:"))
         {
            effectDef.Scopes.AddRange(ParseScopeTypes(line[22..].Trim()));
            continue;
         }

         if (line.StartsWith("**Supported Targets**:"))
         {
            effectDef.Targets.AddRange(ParseScopeTypes(line[22..].Trim()));
            continue;
         }

         if (line.StartsWith(effectDef.Name))
         {
            effectDef.Usage = line.Replace(',', ';').Trim();
            continue;
         }

         if (line.StartsWith("Reads gamestate"))
         {
            effectDef.ReadsGameStateForAllScopes = true;
            continue;
         }

         if (effectDef.Description == string.Empty)
            effectDef.Description = line.Replace(',', ';').Trim();
         else
            effectDef.Description += ' ' + line.Replace(',', ';').Trim();
      }

      if (effectDef != null)
         EffectRegistry.Effects.Add(effectDef.Name, effectDef);
      
      return true;
   }

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
      }

      return resultSet;
   }

   private static string PascalCase(string trimmed)
   {
      return string.Concat(trimmed.Split('_').Select(s => char.ToUpper(s[0]) + s[1..]));
   }
}