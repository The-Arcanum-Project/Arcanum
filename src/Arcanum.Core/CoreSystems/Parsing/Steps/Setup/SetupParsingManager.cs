using System.Collections.Frozen;
using System.Diagnostics;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;

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

   private static Type[]? _parsedTypes;
   public static Type[] ParsedTypes
   {
      get
      {
         if (_parsedTypes == null)
         {
            List<Type> types = [];
            foreach (var service in RegisteredServices)
               types.AddRange(service.ParsedObjects);
            _parsedTypes = [.. types];
         }

         return _parsedTypes;
      }
   }

   public static List<SetupFileLoadingService> RegisteredServices => SetupFileLoaders.Values.ToList();

   static SetupParsingManager()
   {
      // All dependencies can be empty here as this is enforced to be last.
      SetupFileLoaders = new Dictionary<string, SetupFileLoadingService>
      {
         { "locations", new LocationSetupParsing([]) },
         { "building_manager", new BuildingManagerParsing([]) },
         { "character_db", new CharacterParsing([]) },
      }.ToFrozenDictionary();
   }

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

   public static bool IsSetupObject(IEu5Object obj) => PartDefinitions.ContainsKey(obj.GetType());

   public static Type[] NestedSubTypes(IEu5Object eu5Obj)
   {
      List<Type> types = [eu5Obj.GetType()];
      foreach (var prop in eu5Obj.GetAllProperties())
         if (eu5Obj.GetNxPropType(prop) is { } t && typeof(IEu5Object).IsAssignableFrom(t))
         {
            var value = eu5Obj._getValue(prop);
            if (value is IEu5Object &&
                eu5Obj.SaveableProps.First(p => Equals(p.NxProp, prop)).ValueType != SavingValueType.Identifier)
               types.Add(t);
         }

      return types.ToArray();
   }

   public static Type[] GetSetupTypesToProcess(List<IEu5Object> objects)
   {
      HashSet<Type> typesToProcess = [];
      for (var i = objects.Count - 1; i >= 0; i--)
      {
         var obj = objects[i];
         if (SaveMaster.SetupFileWritersByType.ContainsKey(obj.GetType()))
         {
            typesToProcess.Add(obj.GetType());
            objects.RemoveAt(i);
         }
      }

      HashSet<Eu5FileObj> files = [];
      HashSet<Type> processedTypes = [];

      foreach (var et in typesToProcess)
      {
         var (etFiles, etTypes) = GetAllFilesToOverwrite(et);
         files.UnionWith(etFiles);
         processedTypes.UnionWith(etTypes);
      }

      if (files.Count == 0 || processedTypes.Count == 0)
         return [];

      Debug.Assert(files.Count > 0, "There should be at least one file to process.");
      Debug.Assert(processedTypes.Count > 0, "There should be at least one type processed.");

      return [.. processedTypes];
   }

   public static (List<Eu5FileObj> files, List<Type> types) GetAllFilesToOverwrite(Type editedType)
   {
      Debug.Assert(PartDefinitions.ContainsKey(editedType), "Type must be a setup edited type.");

      Dictionary<Eu5FileObj, List<Type>> invertedParts = [];

      foreach (var (type, file) in PartDefinitions)
         foreach (var fo in file)
            if (!invertedParts.TryAdd(fo, [type]))
               invertedParts[fo].Add(type);

      var filesToProcess = new Queue<Eu5FileObj>(PartDefinitions[editedType]);
      List<Eu5FileObj> files = [];
      List<Type> processedTypes = [editedType];

      while (filesToProcess.Count > 0)
      {
         var currentFile = filesToProcess.Dequeue();
         if (files.Contains(currentFile))
            continue;

         files.Add(currentFile);

         if (!invertedParts.TryGetValue(currentFile, out var typesInFile))
            continue;

         foreach (var type in typesInFile)
         {
            if (processedTypes.Contains(type))
               continue;

            processedTypes.Add(type);
            if (!PartDefinitions.TryGetValue(type, out var relatedFiles))
               continue;

            foreach (var relatedFile in relatedFiles)
               if (!filesToProcess.Contains(relatedFile))
                  filesToProcess.Enqueue(relatedFile);
         }
      }

      Debug.Assert(files.Count > 0, "There should be at least one file to process.");
      Debug.Assert(processedTypes.Count > 0, "There should be at least one type processed.");

      Debug.Assert(files.Count == files.Distinct().Count(), "Files to process should be distinct.");
      Debug.Assert(processedTypes.Count == processedTypes.Distinct().Count(), "Processed types should be distinct.");

      return (files, processedTypes);
   }

   public static bool LoadFile(Eu5FileObj fo, object? lockObject)
   {
      var rn = Parser.Parse(fo, out var source, out var ctx);
      var validation = true;
      var pc = new ParsingContext(ctx, source.AsSpan(), nameof(SetupParsingManager), ref validation);

      foreach (var sn in rn.Statements)
         if (sn is ContentNode or BlockNode)
         {
            var key = pc.SliceString(sn);
            if (!SetupFileLoaders.TryGetValue(key, out var service))
            {
               pc.SetContext(sn);
               De.Warning(ref pc,
                          ParsingError.Instance.InvalidContentKeyOrType,
                          key,
                          SetupFileLoaders.Keys.ToArray());
               pc.Fail();
               continue;
            }

            service.LoadSetupFile(sn, ref pc, fo, lockObject);
            foreach (var tt in service.ParsedObjects)
               if (!PartDefinitions.TryAdd(tt, [fo]))
                  PartDefinitions[tt].Add(fo);
         }
         else
         {
            pc.SetContext(sn);
            De.Warning(ref pc,
                       ParsingError.Instance.InvalidNodeType,
                       sn.GetType(),
                       new[] { typeof(ContentNode), typeof(BlockNode) },
                       pc.SliceString(sn));
            pc.Fail();
         }

      return validation;
   }

   public static void ReloadFileByService<T>(Eu5FileObj fileObj,
                                             object? lockObject)
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