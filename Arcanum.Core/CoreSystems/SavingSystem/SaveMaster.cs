using System.Text;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.SavingSystem;

public static class SaveMaster
{
   private static List<Eu5FileObj> NeedsToBeSaved { get; } = [];
   private static Dictionary<SaveableType, List<ISaveable>> NewSaveables { get; } = [];
   private static readonly Dictionary<SaveableType, int> ModificationCache;

   static SaveMaster()
   {
      var saveableTypes = Enum.GetValues<SaveableType>();
      ModificationCache = new(saveableTypes.Length);
      foreach (var t in saveableTypes)
         ModificationCache.Add(t, 0);
   }

   public static int GetModifiedCount => ModificationCache.Values.Sum();

   public static List<(SaveableType type, int amount)> GetModifiedCounts()
   {
      return ModificationCache
            .Where(kvp => kvp.Value > 0)
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToList();
   }

   public static void HandleNewSaveables()
   {
   }

   public static void SaveAll(bool onlyModified = true)
   {
      Save([..Enum.GetValues<SaveableType>()], onlyModified);
   }

   public static ObjState GetState(IEu5Object obj)
   {
      return ObjState.Unchanged;
   }

   public static void Save(List<SaveableType> saveableTypes, bool onlyModified = true)
   {
      /*
       How to handle Dependencies

      FileObj is the instance of a single File
      FileInformationProvider more or less defines the behavior of it

         Where to put the Information of the dependency?
         We have to load a folder of files anyway -> But the files should share a FileInformationProvider

      So the FileInformationProvider will have to have the dependency information

      Why not directly make a class for all the saveabletypes
         -> then the dependency can be made as static?
         -> Then we need some sort of static method, which can be inherited and generate a FileObj?
         -> Does that make sense if we have like 50 different file types, or does it not really change since we would need 50 different IFileInformationProvider implementations anyway?
      */
   }

   public static void RegisterFile(Eu5FileObj fileObj)
   {
   }

   /// <summary>
   /// A list of any modified objects and the file they belong to.
   /// This list can contain nested objects.
   /// </summary>
   /// <param name="modifiedObjects"></param>
   /// <param name="fileObj"></param>
   public static bool SaveFile(List<IEu5Object> modifiedObjects, Eu5FileObj fileObj)
   {
      var topLevelModObjs = FilterOutNestedObjects(modifiedObjects);
      var sb = FormatFileForGivenObjects(fileObj, topLevelModObjs);

      if (sb == null)
         return false;

      IO.IO.WriteAllText(fileObj.Path.FullPath, sb.ToString(), Encoding.UTF8);
      fileObj.GenerateChecksum();
      return true;
   }

   /// <summary>
   /// Saves the given file.
   /// If onlyModifiedObjects is true, only the objects that have been modified will be saved.
   /// Otherwise the entire file will be reformatted and saved.
   /// </summary>
   /// <param name="fileObj"></param>
   /// <param name="onlyModifiedObjects"></param>
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
         sb = FormatFileForGivenObjects(fileObj,
                                        fileObj.ObjectsInFile.Where(obj => GetState(obj) != ObjState.Unchanged)
                                               .OrderBy(obj => obj.FileLocation.CharPos)
                                               .ToList());
      else
         sb = SavingUtil.FormatFilesMultithreadedIf(fileObj.ObjectsInFile.ToList());

      if (sb == null)
         return false;

      IO.IO.WriteAllText(fileObj.Path.FullPath, sb.ToString(), Encoding.UTF8);
      fileObj.GenerateChecksum();

      return true;
   }

   /// <summary>
   /// Takes a file and a list of modifiedObjects
   /// and returns a formatted version of the file with the modified objects replaced.
   /// </summary>
   /// <param name="fileObj"></param>
   /// <param name="modifiedObjects"></param>
   /// <returns></returns>
   private static IndentedStringBuilder? FormatFileForGivenObjects(Eu5FileObj fileObj, List<IEu5Object> modifiedObjects)
   {
      if (modifiedObjects.Count == 0)
         return null;

      // Initialize StringBuilder with estimated size to avoid multiple resizes and a bit more,
      // in case we have to adjust indentation
      var original = IO.IO.ReadAllText(fileObj.Path.FullPath, Encoding.UTF8);

      if (string.IsNullOrEmpty(original))
         return null;

      // Make sure we start with a fresh cache in case settings have changed.
      PropertyOrderCache.Clear();

      var sb = new IndentedStringBuilder(original.Length + modifiedObjects.Count * 20);
      var currentPos = 0;
      var spaces = Config.Settings.AgsConfig.SpacesPerIndent;
      var isb = new IndentedStringBuilder();

      foreach (var obj in modifiedObjects)
      {
         sb.InnerBuilder.Append(original, currentPos, obj.FileLocation.CharPos - currentPos);

         var indentLevel = Math.DivRem(obj.FileLocation.Column, spaces, out var remainder);

         // We have an object defined in one line, disgusting we can not really deal with it,
         // so we just append a new line and dump our formatted obj there :P
         if (remainder != 0)
            sb.InnerBuilder.AppendLine();

         isb.Clear();
         isb.SetIndentLevel(indentLevel);
         obj.ToAgsContext().BuildContext(isb);
         sb.Merge(isb.InnerBuilder);

         currentPos = obj.FileLocation.CharPos + obj.FileLocation.Length;
      }

      sb.InnerBuilder.Append(original, currentPos, original.Length - currentPos);
      return sb;
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