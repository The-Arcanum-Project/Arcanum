using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.Steps.Setup;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;
using Dynasty = Arcanum.Core.GameObjects.InGame.Court.Dynasty;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

[ParserFor(typeof(Dynasty))]
public partial class DynastyManagerParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : SetupFileLoadingService(dependencies)
{
   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, object? lockObject) => throw new NotImplementedException();

   public override void LoadSingleFile(RootNode rn,
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       object? lockObject)
   {
   }

   public override void LoadSetupFile(StatementNode sn, ref ParsingContext pc, Eu5FileObj fileObj, object? lockObject)
   {
      if (!sn.IsBlockNode(ref pc, out var bn))
         return;

      foreach (var bnsn in bn.Children)
      {
         if (!bnsn.IsBlockNode(ref pc, out var dynastyBlock))
            continue;

         var key = pc.SliceString(dynastyBlock);
         var eu5Obj = Eu5Activator.CreateInstance<Dynasty>(key, fileObj, dynastyBlock);
         LUtil.TryAddToGlobals(((SimpleKeyNode)dynastyBlock.KeyNode).KeyToken,
                               ref pc,
                               eu5Obj);
         ParseProperties(dynastyBlock, eu5Obj, ref pc, false);
      }
   }

   public override List<Type> ParsedObjects { get; } = SetupParsingManager.NestedSubTypes(Dynasty.Empty).ToList();

   public override void ReloadSingleFile(Eu5FileObj fileObj, object? lockObject)
   {
      throw new NotImplementedException();
   }
}