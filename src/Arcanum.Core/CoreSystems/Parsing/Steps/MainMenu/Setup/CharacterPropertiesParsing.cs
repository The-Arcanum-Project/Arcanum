using Arcanum.Core.CoreSystems.Common;
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
   public override List<Type> ParsedObjects
   {
      get
      {
         List<Type> types = [typeof(Character)];
         var empty = Character.Empty;
         foreach (var prop in empty.GetAllProperties())
            if (empty.GetNxPropType(prop) is { } t && typeof(IEu5Object).IsAssignableFrom(t))
               types.Add(t);
         return types;
      }
   }

   public override void ReloadSingleFile(Eu5FileObj fileObj,
                                         object? lockObject,
                                         string actionStack,
                                         ref bool validation)
   {
      // We reach this up to the manager to invoke us again with the correct context.
      SetupParsingManager.ReloadFileByService<CharacterParsing>(fileObj,
                                                                lockObject,
                                                                actionStack,
                                                                ref validation);
   }

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, object? lockObject)
   {
      foreach (var character in Globals.Characters.Values)
         ((IEu5Object)character).ResetToDefault();
      return true;
   }

   public override void LoadSetupFile(StatementNode sn,
                                      LocationContext ctx,
                                      Eu5FileObj fileObj,
                                      string actionStack,
                                      string source,
                                      ref bool validation,
                                      object? lockObject)
   {
      if (!sn.IsBlockNode(ctx, source, actionStack, ref validation, out var bn))
         return;

      foreach (var bnsn in bn.Children)
      {
         if (!bnsn.IsBlockNode(ctx, source, actionStack, ref validation, out var characterBlock))
            continue;

         var key = characterBlock.KeyNode.GetLexeme(source);
         var eu5Obj = Eu5Activator.CreateInstance<Character>(key, fileObj, characterBlock);
         LUtil.TryAddToGlobals(ctx,
                               ((SimpleKeyNode)characterBlock.KeyNode).KeyToken,
                               key,
                               actionStack,
                               ref validation,
                               eu5Obj);
         ParseProperties(characterBlock, eu5Obj, ctx, source, ref validation, false);
      }
   }
}