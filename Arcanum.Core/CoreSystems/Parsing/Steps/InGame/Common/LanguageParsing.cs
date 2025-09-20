using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Culture;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(Language))]
public partial class LanguageParsing : ParserValidationLoadingService<Language>
{
   public override List<Type> ParsedObjects => [typeof(Language)];

   public override string GetFileDataDebugInfo() => $"Parsed Languages: {Globals.Languages.Count}";

   protected override bool UnloadSingleFileContent(Eu5FileObj<Language> fileObj, object? lockObject)
   {
      foreach (var obj in fileObj.GetEu5Objects())
         Globals.Languages.Remove(obj.UniqueId);
      return true;
   }

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj<Language> fileObj,
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
                               Globals.Languages,
                               lockObject);
   }
}