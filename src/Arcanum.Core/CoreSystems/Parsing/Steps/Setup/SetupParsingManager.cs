using System.Collections.Frozen;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.Setup;
/*
 * To interpret *any* file in the /main_menu/setup/start directory we have to parse them into blocks
 * Then depending on the block key or the content key we categorize them to invoke their specific interpreters
 */

/// <summary>
/// Always called last during loading as this has a shit ton of dependencies but nothing depends on this.
/// </summary>
public static class SetupParsingManager
{
   private const string SETUP_START_NODE_PARSING = "Setup.Start.NodeParsing";
   private static readonly FrozenDictionary<string, SetupFileLoadingService> SetupFileLoaders;

   // Each file which edits the given object will be listed here if it originates from the setup folder.
   private static readonly Dictionary<Type, List<Eu5FileObj>> PartDefinitions = [];

   public static Eu5FileObj[] LoadedFiles
   {
      get
      {
         HashSet<Eu5FileObj> objs = [];
         foreach (var subList in PartDefinitions.Values)
            objs.UnionWith(subList);
         return objs.ToArray();
      }
   }

   public static List<SetupFileLoadingService> RegisteredServices => SetupFileLoaders.Values.ToList();

   static SetupParsingManager()
   {
      // All dependencies can be empty here as this is enforced to be last.
      SetupFileLoaders = new Dictionary<string, SetupFileLoadingService>
      {
         { "locations", new LocationSetupParsing([]) },
      }.ToFrozenDictionary();
   }

   public static bool LoadFile(Eu5FileObj fo, object? lockObject)
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

            service.LoadSetupFile(sn, ctx, fo, SETUP_START_NODE_PARSING, source, ref validation, lockObject);
            foreach (var tt in service.ParsedObjects)
               if (!PartDefinitions.TryAdd(tt, [fo]))
                  PartDefinitions[tt].Add(fo);
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

   public static void ReloadFileByService<T>(Eu5FileObj fileObj,
                                             object? lockObject,
                                             string actionStack,
                                             ref bool validation)
   {
      SetupFileLoadingService service = null!;

      foreach (var s in SetupFileLoaders.Values)
         if (s.ParsedObjects.Contains(typeof(T)))
         {
            service = s;
            break;
         }

      if (service == null)
         throw new InvalidOperationException($"No setup file loader found for type {typeof(T).FullName}.");

      service.UnloadSingleFileContent(fileObj, null);
      LoadFile(fileObj, lockObject);
   }
}