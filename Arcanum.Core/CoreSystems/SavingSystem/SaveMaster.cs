using Arcanum.Core.CoreSystems.SavingSystem.Util;

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
}