using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(LocationRank), ignoredBlockKeys: ["allow"])]
public partial class LocationRankParsing : ParserValidationLoadingService<LocationRank>
{
   public override List<Type> ParsedObjects => [typeof(LocationRank)];

   public override string GetFileDataDebugInfo() => $"Parsed LocationRanks: {Globals.Regencies.Count}";

   protected override bool UnloadSingleFileContent(Eu5FileObj<LocationRank> fileObj, object? lockObject)
   {
      foreach (var obj in fileObj.GetEu5Objects())
         Globals.Regencies.Remove(obj.UniqueId);
      return true;
   }

   public override bool IsFullyParsed => false;

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj<LocationRank> fileObj,
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
                               Globals.LocationRanks,
                               lockObject);
   }
}