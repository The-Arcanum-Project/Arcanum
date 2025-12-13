using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.Steps.Setup;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.Court;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

[ParserFor(typeof(Character))]
public partial class CharacterParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : SetupFileLoadingService(dependencies)
{
   public override List<Type> ParsedObjects => SetupParsingManager.NestedSubTypes(Character.Empty).ToList();

   public override void ReloadSingleFile(Eu5FileObj fileObj,
                                         object? lockObject)
   {
      // We reach this up to the manager to invoke us again with the correct context.
      SetupParsingManager.ReloadFileByService<CharacterParsing>(fileObj,
                                                                lockObject);
   }

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, object? lockObject)
   {
      foreach (var character in Globals.Characters.Values)
         ((IEu5Object)character).ResetToDefault();
      return true;
   }

   public override void LoadSetupFile(StatementNode sn,
                                      ref ParsingContext pc,
                                      Eu5FileObj fileObj,
                                      object? lockObject)
   {
      if (!sn.IsBlockNode(ref pc, out var bn))
         return;

      foreach (var bnsn in bn.Children)
      {
         if (!bnsn.IsBlockNode(ref pc, out var characterBlock))
            continue;

         var key = pc.SliceString(characterBlock);
         var eu5Obj = Eu5Activator.CreateInstance<Character>(key, fileObj, characterBlock);
         LUtil.TryAddToGlobals(((SimpleKeyNode)characterBlock.KeyNode).KeyToken,
                               ref pc,
                               eu5Obj);
      }

      foreach (var bnsn in bn.Children)
      {
         if (!bnsn.IsBlockNode(ref pc, out var characterBlock))
            continue;

         var key = pc.SliceString(characterBlock);

         ParseProperties(characterBlock, Globals.Characters[key], ref pc, false);
      }
   }
}