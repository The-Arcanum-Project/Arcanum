using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;
using ModifierDefinition = Arcanum.Core.GameObjects.InGame.Common.ModifierDefinition;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Common;

[ParserFor(typeof(ModifierDefinition))]
public partial class ModifierParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<ModifierDefinition>(dependencies)
{
   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, object? lockObject)
   {
      foreach (var obj in fileObj.ObjectsInFile)
         Globals.ModifierDefinitions.Remove(obj.UniqueId);

      return true;
   }

   public override void LoadSingleFile(RootNode rn,
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       object? lockObject)
   {
      SimpleObjectParser.Parse(fileObj,
                               rn,
                               ref pc,
                               ParseProperties,
                               Globals.ModifierDefinitions,
                               lockObject);
   }

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   ModifierDefinition target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes) => ParseProperties(block, target, ref pc, allowUnknownNodes);
}