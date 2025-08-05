using System.IO;
using System.Text;
using System.Windows;
using Arcanum.API.UI;
using Arcanum.Core.CoreSystems.Parsing.DocsParsing;
using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.EffectsAndTriggers;

public static class EffectsAndTriggersDocsParser
{
   public static void LoadDocs()
   {
      var triggerPath = FileManager.GetDocumentsPath("docs", "triggers.log");
      var effectPath = FileManager.GetDocumentsPath("docs", "effects.log");

      if (!File.Exists(triggerPath))
      {
         AppData.WindowLinker.ShowMBox("`triggers.log` file not found. Please run `script_docs` in the game console!",
                         "File Not Found",
                         MBoxButton.OK,
                         MessageBoxImage.Error);
      }
      else
      {
         var docsTriggers = DocsParsingClass.ParseDocs(triggerPath);
         var sb = new StringBuilder();
         sb.AppendLine(DocsObj.GetCsvHeader());
         foreach (var trigger in docsTriggers)
            sb.AppendLine(trigger.ToCsv());
#if DEBUG
         File.WriteAllText("EU5_Triggers_Data.csv", sb.ToString());
#endif
         StaticData.DocsTriggers = docsTriggers;
      }

      if (!File.Exists(effectPath))
      {
         AppData.WindowLinker.ShowMBox("`effects.log` file not found. Please run `script_docs` in the game console!",
                                       "File Not Found",
                                       MBoxButton.OK,
                                       MessageBoxImage.Error);
         return;
      }

      var sbb = new StringBuilder();
      var docsEffects = DocsParsingClass.ParseDocs(effectPath);
      sbb.Clear();
      sbb.AppendLine(DocsObj.GetCsvHeader());
      foreach (var effect in docsEffects)
         sbb.AppendLine(effect.ToCsv());
#if DEBUG
      File.WriteAllText("EU5_Effects_Data.csv", sbb.ToString());
#endif
      
      StaticData.DocsEffects = docsEffects;
   }
}