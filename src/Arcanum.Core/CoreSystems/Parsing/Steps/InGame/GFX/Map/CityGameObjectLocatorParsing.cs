using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.InGame.gfx.map;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.GFX.Map;

[ParserFor(typeof(GameObjectLocator))]
public partial class CityGameObjectLocatorParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<GameObjectLocator>(dependencies)
{
   public override void LoadSingleFile(RootNode rn, ref ParsingContext pc, Eu5FileObj fileObj, object? lockObject)
   {
      SimpleObjectParser.Parse(fileObj, rn, ref pc, ParseProperties, Globals.GameObjectLocators, null);
   }

   protected override void ParsePropertiesToObject(BlockNode block, GameObjectLocator target, ref ParsingContext pc, bool allowUnknownNodes)
   {
      ParseProperties(block, target, ref pc, false);
   }
}