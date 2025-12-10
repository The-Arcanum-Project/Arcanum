using System.Collections.Frozen;
using System.Diagnostics;
using System.Text;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.History;
using Arcanum.Core.CoreSystems.History.Commands;
using Arcanum.Core.CoreSystems.Parsing.Steps.Setup;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.InjectReplaceLogic;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Setup;
using Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.Registry;

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
   private static void RemoveObjectFromChanges(IEu5Object target)
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

   public static void SaveAll()
   {
      Save(NeedsToBeSaved.Keys.Select(o => o.GetType()).Distinct().ToList());
   }

   public static void Save(List<Type> typesToSave)
   {
      if (AppData.HistoryManager.Current == LastSavedHistoryNode)
         return;

      List<IEu5Object> objsToSave = [];
      foreach (var obj in NeedsToBeSaved.Keys)
         if (typesToSave.Contains(obj.GetType()))
            objsToSave.Add(obj);

      SaveSetupFolder(objsToSave);

      if (objsToSave.Count == 0)
         return;

      // We save the objects without any replace and inject logic by simply inserting them into the file they originate from
      if (!Config.Settings.SavingConfig.UseInjectReplaceCalls)
      {
         SaveObjects(objsToSave);
         return;
      }

      InjectionHelper.HandleObjectsWithOptionalInjectLogic(objsToSave);
      LastSavedHistoryNode = AppData.HistoryManager.Current;
   }

   /// <summary>
   /// Sorts the given objects by their source file and saves each file with the modified objects. <br/>
   /// DOES NOT USE INJECT / REPLACE LOGIC!
   /// </summary>
   public static void SaveObjects(List<IEu5Object> objectsToSave)
   {
      var fileGroups = objectsToSave.GroupBy(o => o.Source);

      foreach (var group in fileGroups)
         SaveFile(group.ToList());
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

      var topLevelModObjs = FilterOutNestedObjects(modifiedObjects);
      var sb = UpdateEu5ObjectsInFile(topLevelModObjs);

      if (sb == null)
         return;

      WriteFile(sb.InnerBuilder, fileObj, true);
   }

   /// <summary>
   /// Extracts all setup objects from the list and removes them as they are already handled.
   /// </summary>
   private static void SaveSetupFolder(List<IEu5Object> modifiedObjects)
   {
      var types = SetupParsingManager.GetSetupTypesToProcess(modifiedObjects);
      // we have 2 modes to save:
      // - Vanilla Split: pops / ranks / institutions are in separate files
      // - Combined: all pops / ranks / institutions are in a single file (I prefer this one)

      if (Config.Settings.SavingConfig.CompactSetupFolder)
         SaveSetupCompacted(types);
      else
         SaveSetupSplit(types);
   }

   private static void SaveSetupSplit(Type[] types)
   {
      Debug.Assert(types.All(t => t.IsAssignableTo(typeof(IEu5Object))), "All types must be IEu5Object types.");
      Debug.Assert(types.All(t => SetupFileWritersByType.ContainsKey(t)),
                   "All types must have a corresponding SetupFileWriter.");
      Debug.Assert(types.Length == types.Distinct().Count(), "Types list must not contain duplicates.");

      foreach (var type in types)
      {
         var writers = SetupFileWritersByType[type];
         foreach (var writer in writers)
            WriteFile(writer.WriteFile().InnerBuilder, writer.FullPath);
      }
   }

   private static void SaveSetupCompacted(Type[] types)
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
         var isb = new IndentedStringBuilder();
         foreach (var obj in empty.GetGlobalItemsNonGeneric().Values)
            ((IEu5Object)obj).ToAgsContext().BuildContext(isb);
         var newfo = FileStateManager.CreateEu5FileObject(empty);
         WriteFile(isb.InnerBuilder, newfo, true);
      }
   }

   public static bool AppendOrCreateFiles(List<CategorizedSaveable> cssos, List<InjectObj> removeFromFiles)
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

            cssoInj.FileLocation = new (0,
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
            csso.Target.Source = fo;
            fo.ObjectsInFile.Add(csso.Target);
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

      IndentedStringBuilder? sb;

      if (onlyModifiedObjects)
         sb = UpdateEu5ObjectsInFile(fileObj.ObjectsInFile
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

   /// <summary>
   /// Takes a file and a list of modifiedObjects
   /// and returns a formatted version of the file with the modified objects replaced. <br/>
   /// Also updates the FileLocation of the modified objects to reflect their new position in the file.
   /// </summary>
   private static IndentedStringBuilder? UpdateEu5ObjectsInFile(List<IEu5Object> objs)
   {
      if (objs.Count == 0)
         return null;

      Debug.Assert(objs.All(o => o.Source == objs[0].Source), "All objects must belong to the same source file.");

      List<IEu5Object> toUpdateObjs = [];
      List<IEu5Object> newObjs = [];

      foreach (var obj in objs)
         if (obj.FileLocation == Eu5ObjectLocation.Empty)
            newObjs.Add(obj);
         else
            toUpdateObjs.Add(obj);

      // Initialize StringBuilder with estimated size to avoid multiple resizes and a bit more,
      // in case we have to adjust indentation

      var original = toUpdateObjs.Count > 0
                        ? IO.IO.ReadAllTextUtf8(toUpdateObjs[0].Source.Path.FullPath)
                        : newObjs.Count > 0
                           ? IO.IO.ReadAllTextUtf8(newObjs[0].Source.Path.FullPath)
                           : string.Empty;

      if (string.IsNullOrEmpty(original))
         if (newObjs.Count > 0 && toUpdateObjs.Count > 0)
            throw
               new InvalidOperationException("File is empty but contains existing objects with valid FileLocations.");
         else
            return WriteObjectsToNewFile(newObjs, false);

      // Make sure we start with a fresh cache in case settings have changed.
      PropertyOrderCache.Clear();

      var sb = new IndentedStringBuilder(original.Length + toUpdateObjs.Count * 20);
      var currentPos = 0;
      var spaces = Config.Settings.SavingConfig.SpacesPerIndent;
      var isb = new IndentedStringBuilder();
      var sortedToUpdate = toUpdateObjs.OrderBy(o => o.FileLocation.CharPos).ToList();
      Debug.Assert(sortedToUpdate.All(o => o.FileLocation.CharPos + o.FileLocation.Length <= original.Length),
                   "All modified objects must have a valid FileLocation within the bounds of the file.");

      var sortedOldObjects = objs[0]
                            .Source.ObjectsInFile.Except(sortedToUpdate)
                            .Where(o => o.FileLocation != Eu5ObjectLocation.Empty)
                            .OrderBy(o => o.FileLocation.CharPos)
                            .ToArray();

      var oldObjIndex = 0;
      var deltaLines = 0;
      var deltaCharPos = 0;
      for (var i = 0; i < sortedToUpdate.Count; i++)
      {
         var obj = sortedToUpdate[i];
         isb.Clear();
         sb.InnerBuilder.Append(original, currentPos, obj.FileLocation.CharPos - currentPos);
         currentPos = obj.FileLocation.CharPos + obj.FileLocation.Length;

         var indentLevel = Math.DivRem(obj.FileLocation.Column, spaces, out var remainder);

         // We have an object defined in one line, disgusting we can not really deal with it,
         // so we just append a new line and dump our formatted obj there :P
         if (remainder != 0)
            isb.InnerBuilder.AppendLine();

         isb.SetIndentLevel(indentLevel);
         if (obj.InjRepType != InjRepType.None)
            obj.ToAgsContext().BuildContext(isb, [.. obj.SaveableProps], obj.InjRepType, true);
         else
            obj.ToAgsContext().BuildContext(isb);
         var lineOffset = CountNewLinesInStringBuilder(isb.InnerBuilder);
         var charPosOffset = isb.InnerBuilder.Length;
         var oldLineCount = CountLinesInOriginal(original, obj.FileLocation);
         var oldCharCount = Math.Max(0, obj.FileLocation.Length);

         obj.FileLocation.CharPos = sb.InnerBuilder.Length;
         obj.FileLocation.Length = charPosOffset;
         obj.FileLocation.Line = indentLevel == 0
                                    ? CountNewLinesInStringBuilder(sb.InnerBuilder) + 1
                                    : CountNewLinesInStringBuilder(sb.InnerBuilder);
         obj.FileLocation.Column = indentLevel * spaces;

         // Any object before this one has to get it's FileLocation updated as well
         if (sortedOldObjects.Length > 0)
            while (oldObjIndex < sortedOldObjects.Length &&
                   sortedOldObjects[oldObjIndex].FileLocation.CharPos < obj.FileLocation.CharPos)
            {
               // We can skip the first one as everything before it is just copied over from before.
               if (i == 0)
               {
                  while (oldObjIndex < sortedOldObjects.Length &&
                         sortedOldObjects[oldObjIndex].FileLocation.CharPos < obj.FileLocation.CharPos)
                     oldObjIndex++;
                  break;
               }

               // We only need to update objects that are not being modified right now
               // We update the line and char pos offsets based on the changes made so far
               var oldObj = sortedOldObjects[oldObjIndex];
               oldObj.FileLocation.CharPos += deltaCharPos;
               oldObj.FileLocation.Line += deltaLines;
               oldObjIndex++;
            }

         deltaLines += lineOffset - oldLineCount;
         deltaCharPos += charPosOffset - oldCharCount;
         isb.Merge(sb);
      }

      sb.InnerBuilder.Append(original, currentPos, original.Length - currentPos);

      while (oldObjIndex < sortedOldObjects.Length)
      {
         var newCp = sortedOldObjects[oldObjIndex].FileLocation.CharPos + deltaCharPos;
         var oldObj = sortedOldObjects[oldObjIndex];
         oldObj.FileLocation.CharPos = newCp;
         oldObj.FileLocation.Line += deltaLines;
         oldObjIndex++;
      }

      sb.AppendLine();
      foreach (var obj in newObjs)
      {
         isb.Clear();
         obj.ToAgsContext().BuildContext(isb);
         obj.FileLocation = new (0,
                                 CountNewLinesInStringBuilder(isb.InnerBuilder),
                                 isb.InnerBuilder.Length,
                                 sb.InnerBuilder.Length);
         obj.Source.ObjectsInFile.Add(obj);
         isb.Merge(sb);
      }

#if DEBUG
      // Verify that all modified objects have valid FileLocations within the bounds of the new file and do not overlap
      var newFileLength = sb.InnerBuilder.Length;
      var lastEndPos = -1;
      foreach (var obj in objs[0].Source.ObjectsInFile)
      {
         Debug.Assert(obj.FileLocation is { CharPos: >= 0, Length: >= 0 } &&
                      obj.FileLocation.CharPos + obj.FileLocation.Length <= newFileLength,
                      "All modified objects must have a valid FileLocation within the bounds of the new file.");

         if (lastEndPos != -1)
            if (obj.FileLocation.CharPos <= lastEndPos)
               Debug.Fail("Modified objects must not overlap in the new file.");

         lastEndPos = obj.FileLocation.CharPos + obj.FileLocation.Length - 1;
      }
#endif

      foreach (var obj in objs)
         RemoveObjectFromChanges(obj);

      return sb;
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
               obj.FileLocation = new (0,
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
         o.FileLocation = new (0, fl.Line + deltaLines, fl.Length, fl.CharPos + deltaCharPos);
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
}