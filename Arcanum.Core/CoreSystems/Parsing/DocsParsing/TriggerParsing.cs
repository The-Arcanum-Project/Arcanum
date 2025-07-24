using System.Diagnostics;

namespace Arcanum.Core.CoreSystems.Parsing.DocsParsing;

public class DocsTrigger
{
   public string Name { get; }
   public string Description { get; set; } = string.Empty;
   public string Usage { get; set; } = string.Empty;
   public bool ReadsGameStateForAllScopes { get; set; } = false;
   public string[] Traits { get; set; } = [];
   public string[] Scopes { get; set; } = [];
   public string[] Targets { get; set; } = [];
   
   public DocsTrigger(string name)
   {
      Name = name;
   }
   
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

public static class TriggerParsing
{
   public static List<DocsTrigger> ParseTriggers(string path)
   {
      var lines = IO.IO.ReadAllLinesUtf8(path);
      if (lines == null || lines.Length == 0)
         return [];
      
      var triggerList = new List<DocsTrigger>();
      DocsTrigger? currentTrigger = null;
      
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