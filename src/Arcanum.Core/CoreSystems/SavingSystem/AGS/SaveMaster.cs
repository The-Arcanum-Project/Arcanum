using System.Diagnostics;
using System.IO;
using System.Text;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.History;
using Arcanum.Core.CoreSystems.History.Commands;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.InjectReplaceLogic;
using Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public static class SaveMaster
{
   private static Dictionary<IEu5Object, List<Eu5ObjectCommand>> NeedsToBeSaved { get; } = [];
   private static Dictionary<Type, List<IEu5Object>> NewObjects { get; } = [];
   private static readonly Dictionary<Type, int> ModificationCache = [];
   private static readonly List<Eu5ObjectCommand> ChangesSinceLastSave = [];

   public static HistoryNode? LastSavedHistoryNode = null;

   public static Enum[] GetChangesForObject(IEu5Object obj)
   {
      return !NeedsToBeSaved.TryGetValue(obj, out var list) ? [] : list.Select(c => c.Attribute).Distinct().ToArray();
   }

   static SaveMaster()
   {
   }

   public static int GetModifiedCount => ModificationCache.Values.Sum();

   public static ICollection<IEu5Object> GetAllModifiedObjects() => NeedsToBeSaved.Keys;
   public static Dictionary<Type, List<IEu5Object>> GetNewSaveables() => NewObjects;

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
      {
         ChangesSinceLastSave.Remove(command);
      }

      NeedsToBeSaved.Remove(target);
   }

   private static void AddSingleCommand(Eu5ObjectCommand command, IEu5Object target)
   {
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
      foreach (var target in command.GetTargets())
      {
         AddSingleCommand(command, target);
      }

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
      {
         AddSingleCommand(command, target);
      }

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
   private static bool SaveFile(List<IEu5Object> modifiedObjects)
   {
      if (modifiedObjects.Count == 0)
         return false;

      var fileObj = modifiedObjects[0].Source;

      if (modifiedObjects.Any(o => o.Source != fileObj))
         throw new ArgumentException("All modified objects must belong to the same file.");

      var topLevelModObjs = FilterOutNestedObjects(modifiedObjects);
      var sb = UpdateEu5ObjectsInFile(topLevelModObjs);

      if (sb == null)
         return false;

      WriteFile(sb.InnerBuilder, fileObj);
      return true;
   }

   public static bool AppendOrCreateFileWithInjects(List<CategorizedSaveable> cssos, List<InjectObj> removeFromFiles)
   {
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
         var sb = new IndentedStringBuilder();
         var fileExisted = IO.IO.FileExists(fo.Path.FullPath);
         if (!fileExisted)
            File.Create(fo.Path.FullPath).Close();
         else
            sb.InnerBuilder.Append(IO.IO.ReadAllTextUtf8WithBom(fo.Path.FullPath));

         // We have non -inject/replace objects that just override the file
         if (value[0].SavingCategory == SavingCategory.FileOverride)
         {
            SaveFile(fo, true);
         }
         else
         {
            // TODO: Always creates new file, we want to make it append to existing files if they exist

            foreach (var csso in value)
            {
               var objSb = new IndentedStringBuilder();
               csso.Target.ToAgsContext()
                   .BuildContext(objSb, csso.GetPropertiesToSave(), csso.SavingCategory.ToInjRepStrategy(), true);
               Debug.Assert(csso.InjectedObj != InjectObj.Empty,
                            "InjectedObj should not be empty when saving with inject/replace logic.");
               if (csso.InjectedObj.FileLocation == Eu5ObjectLocation.Empty)
                  csso.InjectedObj.FileLocation = new(0,
                                                      CountNewLinesInStringBuilder(sb.InnerBuilder),
                                                      objSb.InnerBuilder.Length,
                                                      sb.InnerBuilder.Length);
               else
                  csso.InjectedObj.FileLocation.Update(objSb.InnerBuilder.Length,
                                                       CountNewLinesInStringBuilder(objSb.InnerBuilder),
                                                       0,
                                                       sb.InnerBuilder.Length);
               objSb.Merge(sb);
               RemoveObjectFromChanges(csso.Target);
            }

            WriteFile(sb.InnerBuilder, fo);
         }
      }

      return true;
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

      // If the file is not exactly how we left it of we cannot save it,
      // as the positions we have saved from our lexer/parser are not valid anymore
      if (FileStateManager.CalculateSha256(fileObj) != fileObj.Checksum || fileObj.Checksum == Array.Empty<byte>())
      {
         IllegalFileState(fileObj);
         return false;
      }

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

      WriteFile(sb.InnerBuilder, fileObj);

      // TODO: @Melco remove this from the modified list and all caches.
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
      Debug.Assert(objs.All(o => o.FileLocation != Eu5ObjectLocation.Empty),
                   "All objects must have a valid FileLocation.");
      // Initialize StringBuilder with estimated size to avoid multiple resizes and a bit more,
      // in case we have to adjust indentation
      var original = IO.IO.ReadAllTextUtf8(objs[0].Source.Path.FullPath);

      if (string.IsNullOrEmpty(original))
         return WriteObjectsToNewFile(objs);

      // Make sure we start with a fresh cache in case settings have changed.
      PropertyOrderCache.Clear();

      var sb = new IndentedStringBuilder(original.Length + objs.Count * 20);
      var currentPos = 0;
      var spaces = Config.Settings.SavingConfig.SpacesPerIndent;
      var isb = new IndentedStringBuilder();
      var sortedMods = objs.OrderBy(o => o.FileLocation.CharPos).ToList();

      foreach (var obj in sortedMods)
      {
         isb.Clear();
         sb.InnerBuilder.Append(original, currentPos, obj.FileLocation.CharPos - currentPos);
         currentPos = obj.FileLocation.CharPos + obj.FileLocation.Length;

         var indentLevel = Math.DivRem(obj.FileLocation.Column, spaces, out var remainder);

         // We have an object defined in one line, disgusting we can not really deal with it,
         // so we just append a new line and dump our formatted obj there :P
         if (remainder != 0)
            isb.InnerBuilder.AppendLine();

         isb.SetIndentLevel(indentLevel);
         obj.ToAgsContext().BuildContext(isb);
         obj.FileLocation.Update(isb.InnerBuilder.Length,
                                 CountNewLinesInStringBuilder(isb.InnerBuilder),
                                 indentLevel * spaces,
                                 sb.InnerBuilder.Length);

         sb.Merge(isb.InnerBuilder);
      }

      sb.InnerBuilder.Append(original, currentPos, original.Length - currentPos);
      return sb;
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
            if (obj.Source != Eu5FileObj.Empty)
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
      Debug.Assert(objs.All(o => o.FileLocation != Eu5ObjectLocation.Empty),
                   "All objects must have a valid FileLocation.");

      var sample = objs[0];
      if (sample.Source == Eu5FileObj.Empty || sample.FileLocation == Eu5ObjectLocation.Empty)
      {
         ArcLog.WriteLine(CommonLogSource.FSM, LogLevel.ERR, "InjectObj has no valid source file object.");
         Debug.Fail("InjectObj has no valid source file object.");
         return null;
      }

      var sb = new IndentedStringBuilder();
      foreach (var obj in objs)
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
            obj.ToAgsContext().BuildContext(sb);
            obj.FileLocation.Update(sb.InnerBuilder.Length,
                                    CountNewLinesInStringBuilder(sb.InnerBuilder),
                                    0,
                                    sb.InnerBuilder.Length - sb.InnerBuilder.Length);
         }

      return sb;
   }

   /// <summary>
   /// Handles the writing of the given StringBuilder to the file represented by the given Eu5FileObj. <br/>
   /// Moves the file to the modded data space if it is not there yet. <br/>
   /// Updates the checksum of the file after writing.
   /// </summary>
   public static void WriteFile(StringBuilder sb, Eu5FileObj fileObj)
   {
      // We need to move the file to the modded data space if it is not there yet
      if (!fileObj.IsModded)
         fileObj.Path.MoveToMod();

      IO.IO.WriteAllTextUtf8WithBom(fileObj.Path.FullPath, sb.ToString());
      fileObj.GenerateChecksum();
   }

   public static void RemoveObjectsFromFiles(List<IEu5Object> objs)
   {
      var fileGroups = objs.GroupBy(o => o.Source);

      foreach (var group in fileGroups)
      {
         var sb = RemoveEu5ObjectFromFile(group.ToList());
         if (sb == null)
            continue;

         WriteFile(sb, group.Key);
      }
   }

   public static void RemoveObjectsFromFile(List<IEu5Object> objs)
   {
      var sb = RemoveEu5ObjectFromFile(objs);
      if (sb == null)
         return;

      WriteFile(sb, objs[0].Source);
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
      if (sample.Source == Eu5FileObj.Empty || sample.FileLocation == Eu5ObjectLocation.Empty)
      {
         ArcLog.WriteLine(CommonLogSource.FSM, LogLevel.ERR, "InjectObj has no valid source file object.");
         Debug.Fail("InjectObj has no valid source file object.");
         return null;
      }

      var original = IO.IO.ReadAllTextUtf8(sample.Source.Path.FullPath);

      if (string.IsNullOrEmpty(original))
         return null;

      Debug.Assert(obj.All(o => o.FileLocation.CharPos + o.FileLocation.Length <= original.Length),
                   "All objects must have a valid FileLocation within the bounds of the file.");

      PropertyOrderCache.Clear();
      var sb = new StringBuilder(original.Length);
      var currentPos = 0;
      var sortedObjs = obj.OrderBy(o => o.FileLocation.CharPos).ToHashSet();
      var objInFile = sample.Source.ObjectsInFile;

      foreach (var o in objInFile)
      {
         // If we find an object that is yet to be written we skip it
         if (o.FileLocation == Eu5ObjectLocation.Empty)
            continue;

         var objectLength = o.FileLocation.Length;
         if (sortedObjs.Contains(o))
         {
            // Skip over the object to be removed
            currentPos = o.FileLocation.CharPos + objectLength;
            continue;
         }

         sb.Append(original, currentPos, objectLength);
         currentPos = o.FileLocation.CharPos + objectLength;

         o.FileLocation.Update(objectLength,
                               CountNewLinesInStringBuilder(new(original,
                                                                currentPos - objectLength,
                                                                objectLength,
                                                                objectLength)),
                               o.FileLocation.Column,
                               sb.Length - objectLength);
      }

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

   private static void UpdateFileLocations(IEu5Object obj, int delta, int deltaLines)
   {
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

   private static void IllegalFileState(Eu5FileObj fileObj)
   {
      // TODO: Reload? Shutdown? Ignore?
   }
}