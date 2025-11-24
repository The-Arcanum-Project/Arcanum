using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.Setup;
/*
 * To interpret *any* file in the /main_menu/setup/start directory we have to parse them into blocks
 * Then depending on the block key or the content key we categorize them to invoke their specific interpreters
 *
 *
 *
 */

/// <summary>
/// Always called last during loading as this has a shit ton of dependencies but nothing depends on this.
/// </summary>
public static class SetupParsingManager
{
   private const string SETUP_START_NODE_PARSING = "Setup.Start.NodeParsing";
   private static readonly Dictionary<string, SetupFileLoadingService> SetupFileLoaders = new();

   // Each file which edits the given object will be listed here if it originates from the setup folder.
   private static readonly Dictionary<IEu5Object, List<Eu5FileObj>> PartDefinitions = [];

   static SetupParsingManager()
   {
   }

   public static bool LoadSetupFolder(FileDescriptor descriptor)
   {
      var files = descriptor.Files;
      var success = false;
      foreach (var fo in files)
         if (!LoadFile(fo))
            success = false;

      return success;
   }

   public static bool LoadFile(Eu5FileObj fo)
   {
      var rn = Parser.Parse(fo, out var source, out var ctx);
      var validation = true;

      foreach (var sn in rn.Statements)
         if (sn is ContentNode or BlockNode)
         {
            var key = sn.KeyNode.GetLexeme(source);
            if (!SetupFileLoaders.TryGetValue(key, out var service))
            {
               ctx.SetPosition(sn.KeyNode);
               De.Warning(ctx,
                          ParsingError.Instance.InvalidContentKeyOrType,
                          SETUP_START_NODE_PARSING,
                          key,
                          SetupFileLoaders.Keys.ToArray());
               validation = false;
               continue;
            }

            validation &= service.LoadSetupFile(sn, ctx, fo, SETUP_START_NODE_PARSING, source, ref validation, null);
         }
         else
         {
            ctx.SetPosition(sn.KeyNode);
            De.Warning(ctx,
                       ParsingError.Instance.InvalidNodeType,
                       SETUP_START_NODE_PARSING,
                       sn.GetType(),
                       new[] { typeof(ContentNode), typeof(BlockNode) },
                       sn.KeyNode.GetLexeme(source));
            validation = false;
         }

      return validation;
   }
}