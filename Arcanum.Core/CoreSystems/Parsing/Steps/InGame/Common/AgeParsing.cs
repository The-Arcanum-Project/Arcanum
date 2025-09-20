using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.AbstractMechanics;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(Age))]
public partial class AgeParsing : ParserValidationLoadingService<Age>
{
   public override List<Type> ParsedObjects { get; } = [typeof(Age)];
   public override string GetFileDataDebugInfo() => $"Parsed Ages: {Globals.Ages.Count}";

   protected override bool UnloadSingleFileContent(Eu5FileObj<Age> fileObj, object? lockObject)
   {
      foreach (var obj in fileObj.GetEu5Objects())
         Globals.Ages.Remove(obj);

      return true;
   }

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj<Age> fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
      SimpleObjectParser.Parse(fileObj,
                               rn,
                               ctx,
                               actionStack,
                               source,
                               ref validation,
                               ParseProperties,
                               Globals.Ages,
                               lockObject);
   }
}