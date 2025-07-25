using System.Diagnostics;

namespace Arcanum.Core.CoreSystems.Parsing.DocsParsing;

public static class DocsParsing
{
   public static List<DocsObj> ParseDocs(string path)
   {
      var lines = IO.IO.ReadAllLinesUtf8(path);
      if (lines == null || lines.Length == 0)
         return [];

      var triggerList = new List<DocsObj>();
      DocsObj? currentTrigger = null;

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
            currentTrigger.Scopes = line[22..].Trim().Split(',');
            continue;
         }

         if (line.StartsWith("**Supported Targets**:"))
         {
            currentTrigger.Targets = line[22..].Trim().Split(',');
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

      Debug.WriteLine($"[TriggerParsing] Parsed {triggerList.Count} triggers from {path}.");
      return triggerList;
   }
}