using System.Collections.Frozen;
using System.Diagnostics;
using System.Text;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.History;
using Arcanum.Core.CoreSystems.History.Commands;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.Steps.Setup;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.InjectReplaceLogic;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Setup;
using Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.Registry;
using Region = Arcanum.Core.GameObjects.LocationCollections.Region;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public static class SaveMaster
{
   private static Dictionary<IEu5Object, List<Eu5ObjectCommand>> NeedsToBeSaved { get; } = [];
   private static Dictionary<Type, List<IEu5Object>> NewObjects { get; } = [];
   private static readonly Dictionary<Type, int> ModificationCache = [];
   private static readonly List<Eu5ObjectCommand> ChangesSinceLastSave = [];

   public static HistoryNode? LastSavedHistoryNode;

   public static readonly FrozenDictionary<Type, List<SetupFileWriter>> SetupFileWritersByType;

   public static Enum[] GetChangesForObject(IEu5Object obj)
   {
      return !NeedsToBeSaved.TryGetValue(obj, out var list)
                ? []
                : list.Select(c => c.Attribute).OfType<Enum>().Distinct().ToArray();
   }

   static SaveMaster()
   {
      var dict = new Dictionary<Type, List<SetupFileWriter>>();
      AddIntoDict(dict, new ArtWriter());
      AddIntoDict(dict, new CharacterWriter());
      AddIntoDict(dict, new CitiesAndBuildingsWriter());
      AddIntoDict(dict, new ColoniesWriter());
      AddIntoDict(dict, new CoreWriter());
      AddIntoDict(dict, new CountriesWriter());
      AddIntoDict(dict, new DevelopmentWriter());
      AddIntoDict(dict, new DiplomacyWriter());
      AddIntoDict(dict, new DiseasesWriter());
      AddIntoDict(dict, new DynastyWriter());
      AddIntoDict(dict, new ExplorationPreferenceWriter());
      AddIntoDict(dict, new InstitutionWriter());
      AddIntoDict(dict, new InternationalOrganizationsWriter());
      AddIntoDict(dict, new LocationsWriter());
      AddIntoDict(dict, new MarketWriter());
      AddIntoDict(dict, new OpinionsWriter());
      AddIntoDict(dict, new PopsWriter());
      AddIntoDict(dict, new ReligionWriter());
      AddIntoDict(dict, new RivalsWriter());
      AddIntoDict(dict, new RoadsWriter());
      AddIntoDict(dict, new SituationsWriter());
      AddIntoDict(dict, new WarsWriter());

      SetupFileWritersByType = dict.ToFrozenDictionary();
   }

   public static int GetModifiedCount => ModificationCache.Values.Sum();
   public static int GetNeedsToBeSaveCount => NeedsToBeSaved.Count;

   public static ICollection<IEu5Object> GetAllModifiedObjects() => NeedsToBeSaved.Keys;
   public static Dictionary<Type, List<IEu5Object>> GetNewSaveables() => NewObjects;

   private static void AddIntoDict(Dictionary<Type, List<SetupFileWriter>> dict, SetupFileWriter writer)
   {
      foreach (var type in writer.ContainedTypes)
      {
         if (!dict.TryGetValue(type, out var list))
            dict[type] = list = [];
         list.Add(writer);
      }
   }

   public static void AddNewObject(IEu5Object obj)
   {
      var type = obj.GetType();
      if (!NewObjects.TryGetValue(type, out var list))
         NewObjects[type] = list = [];

      list.Add(obj);
   }

   // TODO: @Melco Handle removal of new objects on undo of creation
   public static void RemoveNewObject(IEu5Object obj)
   {
      var type = obj.GetType();
      if (NewObjects.TryGetValue(type, out var list))
      {
         list.Remove(obj);
         if (list.Count == 0)
            NewObjects.Remove(type);
      }
   }

   private static void RemoveChange(Eu5ObjectCommand command)
   {
      //TODO REMOVE THIS
      if (CommandManager.IgnoreCommands)
         return;

      var targets = command.GetTargets();
      foreach (var target in targets)
      {
         if (!NeedsToBeSaved.TryGetValue(target, out var list))
         {
            NeedsToBeSaved[target] = list = [];
            var type = target.GetType();
            if (ModificationCache.TryGetValue(type, out var value))
               ModificationCache[type] = --value;
            else
               throw new InvalidOperationException("ModificationCache does not contain type " + type);
         }

         list.Remove(command);
      }

      ChangesSinceLastSave.Remove(command);
   }

   // TODO: @Melco Handle removal of all changes for an objects saved with injection as it is not saved on a on command basis.
   public static void RemoveObjectFromChanges(IEu5Object target)
   {
      if (!NeedsToBeSaved.TryGetValue(target, out var list))
         return;

      foreach (var command in list)
         ChangesSinceLastSave.Remove(command);

      NeedsToBeSaved.Remove(target);
      if (NewObjects.TryGetValue(target.GetType(), out var newList))
      {
         newList.Remove(target);
         if (newList.Count == 0)
            NewObjects.Remove(target.GetType());
      }
   }

   private static void AddSingleCommand(Eu5ObjectCommand command, IEu5Object target)
   {
      // TODO REMOVE THIS
      if (CommandManager.IgnoreCommands)
         return;

      if (!NeedsToBeSaved.TryGetValue(target, out var list))
      {
         NeedsToBeSaved[target] = list = [];
         var type = target.GetType();
         //TODO: @Melco move to a different place for multitarget
         if (ModificationCache.TryGetValue(type, out var value))
            ModificationCache[type] = ++value;
         else
            ModificationCache[type] = 1;
      }
      else
      {
         // check if command is already in the list
         if (list.Contains(command))
            return;
      }

      list.Add(command);
   }

   private static void AddChange(Eu5ObjectCommand command)
   {
      //TODO REMOVE THIS
      if (CommandManager.IgnoreCommands)
         return;

      foreach (var target in command.GetTargets())
         AddSingleCommand(command, target);

      ChangesSinceLastSave.Add(command);
   }

   public static void CommandExecuted(Eu5ObjectCommand command)
   {
      if (ChangesSinceLastSave.HasItems() && ChangesSinceLastSave.Last() == command)
         RemoveChange(command);
      AddChange(command);
   }

   public static void InitCommand(Eu5ObjectCommand command, IEu5Object target)
   {
      AddSingleCommand(command, target);
      ChangesSinceLastSave.Add(command);
   }

   public static void InitCommand(Eu5ObjectCommand command, IEu5Object[] targets)
   {
      foreach (var target in targets)
         AddSingleCommand(command, target);

      ChangesSinceLastSave.Add(command);
   }

   public static void AddToCommand(Eu5ObjectCommand command, IEu5Object target)
   {
      Debug.Assert(ChangesSinceLastSave.HasItems() && ChangesSinceLastSave.Last() == command,
                   "The command to add to is not the last executed command.");
      AddSingleCommand(command, target);
   }

   public static void CommandUndone(Eu5ObjectCommand command)
   {
      if (ChangesSinceLastSave.HasItems() && ChangesSinceLastSave.Last() == command)
         AddChange(command);
      RemoveChange(command);
   }

   public static ObjState GetState(IEu5Object obj)
   {
      if (NeedsToBeSaved.ContainsKey(obj))
         return ObjState.Modified;

      return NewObjects.Values.Any(list => list.Contains(obj)) ? ObjState.New : ObjState.Unchanged;
   }

   public static void Save(List<Type> typesToSave, Action<string> updateHandle)
   {
      if (AppData.HistoryManager.Current == LastSavedHistoryNode)
         return;

      List<IEu5Object> objsToSave = [];
      foreach (var obj in NeedsToBeSaved.Keys)
         if (typesToSave.Contains(obj.GetType()))
            objsToSave.Add(obj);

      // We saved setup stuff and have nothing else to save
      if (SaveSetupFolder(objsToSave, updateHandle) && objsToSave.Count == 0)
         return;

      // We save the objects without any replace and inject logic by simply inserting them into the file they originate from
      if (!Config.Settings.SavingConfig.UseInjectReplaceCalls)
      {
         SaveObjects(objsToSave, updateHandle);
         return;
      }

      InjectionHelper.HandleObjectsWithOptionalInjectLogic(objsToSave, updateHandle);
      LastSavedHistoryNode = AppData.HistoryManager.Current;
   }

   /// <summary>
   /// Sorts the given objects by their source file and saves each file with the modified objects. <br/>
   /// DOES NOT USE INJECT / REPLACE LOGIC!
   /// </summary>
   public static void SaveObjects(List<IEu5Object> objectsToSave, Action<string> updateProgress)
   {
      if (SaveDefnitionsFile(objectsToSave))
         return;

      var fileGroups = objectsToSave.GroupBy(o => o.Source);

      foreach (var group in fileGroups)
      {
         updateProgress.Invoke($"Saving file: {group.Key.Path.Filename}");
         SaveFile(group.ToList());
      }

      LastSavedHistoryNode = AppData.HistoryManager.Current;
   }

   /// <summary>
   /// A list of any modified objects and the file they belong to.
   /// This list can contain nested objects.
   /// </summary>
   private static void SaveFile(List<IEu5Object> modifiedObjects)
   {
      if (modifiedObjects.Count == 0)
         return;

      var fileObj = modifiedObjects[0].Source;

      if (modifiedObjects.Any(o => o.Source != fileObj))
         throw new ArgumentException("All modified objects must belong to the same file.");

      if (SaveDefnitionsFile(modifiedObjects))
         return;

      var topLevelModObjs = FilterOutNestedObjects(modifiedObjects);
      var sb = FileUpdateManager.UpdateEu5ObjectsInFile(topLevelModObjs);

      if (sb == null)
         return;

      WriteFile(sb.InnerBuilder, fileObj, true);
   }

   private static bool SaveDefnitionsFile(List<IEu5Object> objectsToSave)
   {
      var definitionObjects = new List<IEu5Object>();
      for (var i = objectsToSave.Count - 1; i >= 0; i--)
      {
         var obj = objectsToSave[i];
         if (obj is Continent or SuperRegion or Region or Area or Province)
         {
            definitionObjects.Add(obj);
            objectsToSave.RemoveAt(i);
         }
      }

      if (definitionObjects.Count == 0)
         return false;

      var sb = new IndentedStringBuilder();
      foreach (IEu5Object obj in Globals.Continents.Values)
         obj.ToAgsContext().BuildContext(sb);

      WriteFile(sb.InnerBuilder, DescriptorDefinitions.DefinitionsDescriptor.Files[0], true);

      return objectsToSave.Count > 0;
   }

   /// <summary>
   /// Extracts all setup objects from the list and removes them as they are already handled.
   /// </summary>
   private static bool SaveSetupFolder(List<IEu5Object> modifiedObjects, Action<string> updateHandle)
   {
      var types = SetupParsingManager.GetSetupTypesToProcess(modifiedObjects);

      // We do have nothing to handle here.
      if (types.Length == 0)
         return false;

      // we have 2 modes to save:
      // - Vanilla Split: pops / ranks / institutions are in separate files
      // - Combined: all pops / ranks / institutions are in a single file (I prefer this one)
      if (Config.Settings.SavingConfig.CompactSetupFolder)
         SaveSetupCompacted(types, updateHandle);
      else
         SaveSetupSplit(types, updateHandle);

      return true;
   }

   private static void SaveSetupSplit(Type[] types, Action<string> updateHandle)
   {
      Debug.Assert(types.All(t => t.IsAssignableTo(typeof(IEu5Object))), "All types must be IEu5Object types.");
      Debug.Assert(types.All(t => SetupFileWritersByType.ContainsKey(t)),
                   "All types must have a corresponding SetupFileWriter.");
      Debug.Assert(types.Length == types.Distinct().Count(), "Types list must not contain duplicates.");

      foreach (var type in types)
      {
         var writers = SetupFileWritersByType[type];
         foreach (var writer in writers)
         {
            updateHandle.Invoke($"Saving setup file: {writer.FileName}");
            IO.IO.WriteAllText(writer.FullPath, writer.WriteFile().InnerBuilder.ToString(), writer.FileEncoding);
         }
      }
   }

   private static void SaveSetupCompacted(Type[] types, Action<string> updateHandle)
   {
      Debug.Assert(types.All(t => t.IsAssignableTo(typeof(IEu5Object))), "All types must be IEu5Object types.");

      // Create the dummy files:
      var emptySb = new StringBuilder(0);
      foreach (var t in types)
      {
         var writers = SetupFileWritersByType[t];
         foreach (var writer in writers)
            WriteFile(emptySb, writer.FullPath);
      }

      // Now save all objects into a single new file.
      foreach (var type in types)
      {
         var empty = (IEu5Object)EmptyRegistry.Empties[type];
         updateHandle.Invoke($"Saving compacted setup file for type: {empty.Source.Path.RelativePath}");
         var isb = new IndentedStringBuilder();
         foreach (var obj in empty.GetGlobalItemsNonGeneric().Values)
            ((IEu5Object)obj).ToAgsContext().BuildContext(isb);
         var newfo = FileStateManager.CreateEu5FileObject(empty);
         WriteFile(isb.InnerBuilder, newfo, true);
      }
   }

   public static bool AppendOrCreateFiles(List<CategorizedSaveable> cssos, List<InjectObj> removeFromFiles, Action<string> updateHandle)
   {
      if (cssos.Count == 0 && removeFromFiles.Count == 0)
         return false;

      // TODO: This can be done way better but we have a unique challenge for game/in_game/map_data/definitions.txt
      // As we have nested objects in there we can not just append or update into the file
      if (cssos.Count > 0 && cssos[0].Target.Source.Descriptor.FilePath.EndsWith("definitions.txt"))
      {
         // We just format all continents fully into the file
         var sortedContinents =
            Globals.Continents.Values.OrderBy(c => c.FileLocation.CharPos).Cast<IEu5Object>().ToList();
         var sb = SavingUtil.FormatFilesMultithreadedIf(sortedContinents);
         WriteFile(sb.InnerBuilder, cssos[0].Target.Source, true);
         return true;
      }

      RemoveObjectsFromFile(removeFromFiles.Cast<IEu5Object>().ToList());

      foreach (var injectObj in removeFromFiles)
      {
         injectObj.FileLocation = Eu5ObjectLocation.Empty;
         injectObj.Source.ObjectsInFile.Remove(injectObj);
      }

      Dictionary<Eu5FileObj, List<CategorizedSaveable>> fileGroups = [];
      foreach (var csso in cssos)
      {
         if (!fileGroups.TryGetValue(csso.SaveLocation, out var list))
            fileGroups[csso.SaveLocation] = list = [];

         list.Add(csso);
      }

      PropertyOrderCache.Clear();

      foreach (var (fo, value) in fileGroups)
      {
         updateHandle.Invoke($"Saving file: {fo.Path.RelativePath}");
         // We have non -inject/replace objects that just override the file
         if (value[0].SavingCategory == SavingCategory.FileOverride)
         {
            HandleFileOverrides(value, fo);
            continue;
         }

         if (value[0].SavingCategory == SavingCategory.Modify)
         {
            foreach (var csso in value)
            {
               fo.ObjectsInFile.Add(csso.Target);
               csso.Target.Source = fo;
            }

            SaveFile(fo, true);
            foreach (var csso in value)
               RemoveObjectFromChanges(csso.Target);
            continue;
         }

         var sb = new IndentedStringBuilder();

         // The file already exists so we just append it's content first
         if (IO.IO.FileExists(fo.Path.FullPath))
            sb.InnerBuilder.Append(IO.IO.ReadAllTextUtf8WithBom(fo.Path.FullPath));

         var objSb = new IndentedStringBuilder();
         foreach (var csso in value)
         {
            objSb.Clear();

            csso.Target.ToAgsContext()
                .BuildContext(objSb, csso.GetPropertiesToSave(), csso.SavingCategory.ToInjRepStrategy(), true);
            var cssoInj = csso.InjectedObj;

            Debug.Assert(cssoInj != InjectObj.Empty,
                         "InjectedObj should not be empty when saving with inject/replace logic.");

            cssoInj.FileLocation = new(0,
                                       CountNewLinesInStringBuilder(sb.InnerBuilder),
                                       objSb.InnerBuilder.Length,
                                       sb.InnerBuilder.Length);

            objSb.Merge(sb);
            RemoveObjectFromChanges(csso.Target);
         }

         WriteFile(sb.InnerBuilder, fo, true);
      }

      return true;
   }

   private static void HandleFileOverrides(List<CategorizedSaveable> value, Eu5FileObj fo)
   {
      Debug.Assert(value.All(csso => csso.SavingCategory == SavingCategory.FileOverride),
                   "All objects must be of FileOverride category.");

      // If we have a newly created object it's Source property is still Empty so we need to set it here
      foreach (var csso in value)
      {
         if (csso.Target.Source == Eu5FileObj.Empty)
         {
            // Set the source to the file we are saving to
            // We do NOT add it to the file's object list as this is done in the saving itself to not have it interfere with the rewrite logic
            csso.Target.Source = fo;
         }
      }

      Debug.Assert(value.All(csso => csso.Target.Source != Eu5FileObj.Empty && csso.Target.Source == fo),
                   "All objects must have a valid file location matching the file they are being saved to.");

      SaveFile(fo, true);

      foreach (var csso in value)
         RemoveObjectFromChanges(csso.Target);
   }

   /// <summary>
   /// Saves the given file.
   /// If onlyModifiedObjects is true, only the objects that have been modified will be saved.
   /// Otherwise the entire file will be reformatted and saved.
   /// </summary>
   /// <returns></returns>
   public static bool SaveFile(Eu5FileObj fileObj, bool onlyModifiedObjects = false)
   {
      if (fileObj == Eu5FileObj.Empty)
         return false;

      if (fileObj == DescriptorDefinitions.DefinitionsDescriptor.Files[0])
      {
         SaveDefnitionsFile(fileObj.ObjectsInFile.ToList());
         return true;
      }

      IndentedStringBuilder? sb;

      if (onlyModifiedObjects)
         sb = FileUpdateManager.UpdateEu5ObjectsInFile(fileObj.ObjectsInFile
                                                              .Where(obj => GetState(obj) != ObjState.Unchanged)
                                                              .OrderBy(obj => obj.FileLocation.CharPos)
                                                              .ToList());
      else
         sb = SavingUtil.FormatFilesMultithreadedIf(fileObj.ObjectsInFile.ToList());

      if (sb == null)
         return false;

      WriteFile(sb.InnerBuilder, fileObj, true);
      return true;
   }

   private static int CountLinesInOriginal(string originalText, Eu5ObjectLocation location)
   {
      var lineCount = 0;
      var endPos = Math.Min(location.CharPos + location.Length, originalText.Length);
      for (var i = location.CharPos; i < endPos; i++)
         if (originalText[i] == '\n')
            lineCount++;

      return location.Length > 0 ? lineCount + 1 : 0;
   }

   public static IndentedStringBuilder? WriteObjectsToNewFile(List<IEu5Object> objs, bool generateFileName = true)
   {
      if (objs.Count == 0)
         return null;

      if (generateFileName)
      {
         var fo = FileStateManager.CreateEu5FileObject(objs[0]);
         foreach (var obj in objs)
         {
            if (obj.Source == Eu5FileObj.Empty)
            {
               ArcLog.WriteLine(CommonLogSource.FSM,
                                LogLevel.ERR,
                                "Object already has a source file. Overriding to new file.");
               Debug.Fail("Object already has a source file. Overriding to new file.");
            }

            obj.Source = fo;
         }
      }

      Debug.Assert(objs.All(o => o.Source == objs[0].Source), "All objects must belong to the same source file.");

      var sb = new IndentedStringBuilder();
      var objBuilder = new IndentedStringBuilder();
      foreach (var obj in objs)
      {
         objBuilder.Clear();
         if (obj is InjectObj injectObj)
         {
            ArcLog.WriteLine(CommonLogSource.FSM,
                             LogLevel.WRN,
                             $"Writing new file with InjectObj {injectObj.UniqueId}. " +
                             "InjectObjs are not supported in new files and will be skipped.");
            Debug.Fail("Writing new file with InjectObj. InjectObjs are not supported in new files and will be skipped.");
         }
         else
         {
            obj.ToAgsContext().BuildContext(objBuilder);
            if (obj.FileLocation == Eu5ObjectLocation.Empty)
               obj.FileLocation = new(0,
                                      CountNewLinesInStringBuilder(sb.InnerBuilder),
                                      objBuilder.InnerBuilder.Length,
                                      sb.InnerBuilder.Length);
            else
               obj.FileLocation.Update(objBuilder.InnerBuilder.Length,
                                       CountNewLinesInStringBuilder(sb.InnerBuilder),
                                       0,
                                       sb.InnerBuilder.Length);
            objBuilder.Merge(sb);
         }
      }

      return sb;
   }

   /// <summary>
   /// Handles the writing of the given StringBuilder to the file represented by the given Eu5FileObj. <br/>
   /// Moves the file to the modded data space if it is not there yet. <br/>
   /// Updates the checksum of the file after writing. <br/>
   /// Registers the file path with the FileStateManager if register is true and the file is not yet registered.
   /// </summary>
   public static void WriteFile(StringBuilder sb, Eu5FileObj fileObj, bool register)
   {
      // We need to move the file to the modded data space if it is not there yet
      if (!fileObj.IsModded && Config.Settings.SavingConfig.MoveFilesToModdedDataSpaceOnSaving)
         fileObj.Path.MoveToMod();

      WriteFile(sb, fileObj.Path.FullPath);
      if (register)
         FileStateManager.RegisterPath(fileObj.Path);
   }

   private static void WriteFile(StringBuilder sb, string path)
   {
      IO.IO.WriteAllTextUtf8WithBom(path, sb.ToString());
   }

   public static void RemoveObjectsFromFiles(List<IEu5Object> objs)
   {
      var fileGroups = objs.GroupBy(o => o.Source);

      foreach (var group in fileGroups)
      {
         var sb = RemoveEu5ObjectFromFile(group.ToList());
         if (sb == null)
            continue;

         WriteFile(sb, group.Key, false);
      }
   }

   public static void RemoveObjectsFromFile(List<IEu5Object> objs)
   {
      var sb = RemoveEu5ObjectFromFile(objs);
      if (sb == null)
         return;

      WriteFile(sb, objs[0].Source, false);
   }

   /// <summary>
   /// Removes the given objects from their source file and updates the FileLocation of the remaining objects. <br/>
   /// Returns the modified file as a StringBuilder. <br/>
   /// Fails if the objects do not belong to the same source file or if any of the objects do not have a valid FileLocation. <br/>
   /// Returns null if no objects were provided or if the file could not be read. 
   /// </summary>
   private static StringBuilder? RemoveEu5ObjectFromFile(List<IEu5Object> obj)
   {
      if (obj.Count == 0)
         return null;

      Debug.Assert(obj.All(o => o.Source == obj[0].Source), "All objects must belong to the same source file.");
      Debug.Assert(obj.All(o => o.FileLocation != Eu5ObjectLocation.Empty),
                   "All objects must have a valid FileLocation.");

      var sample = obj[0];
      var fo = sample.Source;
      if (fo == Eu5FileObj.Empty || sample.FileLocation == Eu5ObjectLocation.Empty)
      {
         ArcLog.WriteLine(CommonLogSource.FSM, LogLevel.ERR, "InjectObj has no valid source file object.");
         Debug.Fail("InjectObj has no valid source file object.");
         return null;
      }

      var original = IO.IO.ReadAllTextUtf8(fo.Path.FullPath);

      if (string.IsNullOrEmpty(original))
         return null;

      Debug.Assert(obj.All(o => o.FileLocation.CharPos + o.FileLocation.Length <= original.Length),
                   "All objects must have a valid FileLocation within the bounds of the file.");

      PropertyOrderCache.Clear();
      var sb = new StringBuilder(original.Length);
      // The position we are in the source string we are removing content from

      var srcPointer = 0;
      var toRemove = obj.OrderBy(o => o.FileLocation.CharPos).ToArray();
      var objInFile = fo.ObjectsInFile.OrderBy(o => o.FileLocation.CharPos).ToArray();

      var lastWrittenObject = 0;
      var deltaLines = 0;
      var deltaCharPos = 0;
      for (var i = 0; i < objInFile.Length; i++)
      {
         var o = objInFile[i];
         // If we find an object that is yet to be written we skip it
         if (o.FileLocation == Eu5ObjectLocation.Empty)
         {
            lastWrittenObject = i + 1;
            continue;
         }

         // We found the next object we want to remove.
         if (srcPointer < toRemove.Length && toRemove[srcPointer].FileLocation.CharPos == o.FileLocation.CharPos)
         {
            // Write everything up to the object to be removed
            sb.Append(original,
                      objInFile[lastWrittenObject].FileLocation.CharPos,
                      o.FileLocation.CharPos - objInFile[lastWrittenObject].FileLocation.CharPos);

            // Skip over the object to be removed
            deltaLines -= CountLinesInOriginal(original, o.FileLocation);
            deltaCharPos -= o.FileLocation.Length;
            fo.ObjectsInFile.Remove(o);
            srcPointer++;
            lastWrittenObject = i + 1;
            continue;
         }

         // We are after the object to be removed, so we need to update the FileLocation
         var fl = o.FileLocation;
         o.FileLocation = new(0, fl.Line + deltaLines, fl.Length, fl.CharPos + deltaCharPos);
      }

      // Write any remaining content after the last removed object
      if (lastWrittenObject < objInFile.Length)
         sb.Append(original,
                   objInFile[lastWrittenObject].FileLocation.CharPos,
                   original.Length - objInFile[lastWrittenObject].FileLocation.CharPos);

#if DEBUG
      var newFileLength = sb.Length;
      var lastEndPos = -1;
      foreach (var ob in fo.ObjectsInFile)
      {
         // An unsaved new object will not have a valid FileLocation
         if (ob.FileLocation == Eu5ObjectLocation.Empty)
            continue;

         Debug.Assert(ob.FileLocation is { CharPos: >= 0, Length: >= 0 } &&
                      ob.FileLocation.CharPos + ob.FileLocation.Length <= newFileLength,
                      "All modified objects must have a valid FileLocation within the bounds of the new file.");

         if (lastEndPos != -1)
            if (ob.FileLocation.CharPos <= lastEndPos)
               Debug.Fail("Original objects must not overlap in the new file after removal of objects.");

         lastEndPos = ob.FileLocation.CharPos + ob.FileLocation.Length - 1;
      }
#endif
      return sb;
   }

   public static int CountNewLinesInStringBuilder(StringBuilder sb)
   {
      ArgumentNullException.ThrowIfNull(sb);

      var newlineCount = 0;

      for (var i = 0; i < sb.Length; i++)
         if (sb[i] == '\n')
            newlineCount++;

      return newlineCount;
   }

   /// <summary>
   /// Counts the number of newline characters ('\n') in a specific region of a StringBuilder.
   /// </summary>
   public static int CountNewlinesInRegion(StringBuilder sb, int startIndex, int count)
   {
      ArgumentNullException.ThrowIfNull(sb);

      if (startIndex < 0 || count < 0 || startIndex > sb.Length - count)
         throw new ArgumentOutOfRangeException(nameof(startIndex),
                                               "The specified region is out of the bounds of the StringBuilder.");

      var newlineCount = 0;
      var endIndex = startIndex + count;

      for (var i = startIndex; i < endIndex; i++)
         if (sb[i] == '\n')
            newlineCount++;

      return newlineCount;
   }

   private static List<IEu5Object> FilterOutNestedObjects(List<IEu5Object> allMods)
   {
      if (allMods.Count < 2)
         return allMods;

      var topLevel = new List<IEu5Object>();
      var modSpans = allMods.Select(m => (Start: m.FileLocation.CharPos,
                                          End: m.FileLocation.CharPos + m.FileLocation.Length, Obj: m))
                            .ToList();

      for (var i = 0; i < modSpans.Count; i++)
      {
         var current = modSpans[i];
         var isNested = false;

         for (var j = 0; j < modSpans.Count; j++)
         {
            if (i == j)
               continue;

            var other = modSpans[j];
            // Is 'current' completely inside 'other'?
            if (current.Start < other.Start || current.End > other.End)
               continue;

            isNested = true;
            break;
         }

         if (!isNested)
            topLevel.Add(current.Obj);
      }

      return topLevel;
   }

   public static void SaveAll(Action<string> updateHook)
   {
      Save(NeedsToBeSaved.Keys.Select(o => o.GetType()).Distinct().ToList(), updateHook);
   }
}