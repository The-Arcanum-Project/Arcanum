using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Character;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(Regency), ignoredBlockKeys: ["start_effect", "allow"])]
public partial class RegencyParsing : ParserValidationLoadingService<Regency>
{
   public override List<Type> ParsedObjects => [typeof(Regency)];

   public override string GetFileDataDebugInfo() => $"Parsed Regencies: {Globals.Regencies.Count}";

   protected override bool UnloadSingleFileContent(Eu5FileObj<Regency> fileObj)
   {
      foreach (var obj in fileObj.GetEu5Objects())
         Globals.Regencies.Remove(obj.UniqueId);
      return true;
   }

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj<Regency> fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation)
   {
      SimpleObjectParser.Parse(fileObj,
                               rn,
                               ctx,
                               actionStack,
                               source,
                               ref validation,
                               ParseProperties,
                               Globals.Regencies);
   }
}