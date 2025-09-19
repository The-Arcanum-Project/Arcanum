using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(Climate))]
public partial class ClimateParsing : ParserValidationLoadingService<Climate>
{
   public override List<Type> ParsedObjects { get; } = [typeof(Climate)];
   public override string GetFileDataDebugInfo() => $"Parsed Climates: {Globals.Climates.Count}";
   public override bool IsFullyParsed => false;

   protected override bool UnloadSingleFileContent(Eu5FileObj<Climate> fileObj)
   {
      foreach (var obj in fileObj.GetEu5Objects())
         Globals.Climates.Remove(obj.UniqueId);

      return true;
   }

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj<Climate> fileObj,
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
                               Globals.Climates);
   }
}