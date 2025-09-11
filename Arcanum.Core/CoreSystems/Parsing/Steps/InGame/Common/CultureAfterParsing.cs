using System.Text;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Culture;
using Arcanum.Core.GlobalStates;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

public class CultureAfterParsing : AfterLoadingService
{
   public override string GetFileDataDebugInfo()
      => $"Initialized '{Globals.Cultures.Values.Sum(x => x.Opinions.Count)}' cultural relations.";

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
   {
      foreach (var culture in Globals.Cultures.Values)
         Nx.ClearCollection(culture, Culture.Field.Opinions);

      return true;
   }

   protected override bool AfterLoadingStep()
   {
      const string actionStack = nameof(CultureAfterParsing);

      foreach (var (key, op) in CultureParsing.Opinions2NdParse)
      {
         var ctx = LocationContext.GetNew(key);
         var source = IO.IO.ReadAllText(key.Path.FullPath, Encoding.UTF8)!;

         foreach (var opList in op)
         {
            foreach (var cn in opList.Nodes)
            {
               if (!cn.TryGetEnumValue(ctx, actionStack, source, typeof(SimpleOpinion), out var simpleOpinion))
                  continue;

               var lexeme = cn.KeyNode.GetLexeme(source);
               if (!Globals.Cultures.TryGetValue(lexeme, out var targetCulture))
               {
                  ctx.SetPosition(cn.KeyNode);
                  DiagnosticException.LogWarning(ctx.GetInstance(),
                                                 ParsingError.Instance.InvalidContentKeyOrType,
                                                 actionStack,
                                                 lexeme,
                                                 "Any culture name");
                  continue;
               }

               opList.Culture.Opinions.Add(new(targetCulture, (SimpleOpinion)simpleOpinion));
            }
         }
      }

      return true;
   }
}