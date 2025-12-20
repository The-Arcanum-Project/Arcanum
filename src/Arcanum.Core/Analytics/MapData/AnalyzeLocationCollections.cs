using System.Text;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections.BaseClasses;

namespace Arcanum.Core.Analytics.MapData;

public static class AnalyzeLocationCollections
{
   public static bool VerifyUniquenessOfChildren<T>(ILocationCollection<T>[] collection,
                                                    out List<string> errorsMsgs) where T : class, ILocation
   {
      List<KeyValuePair<T, List<ILocationCollection<T>>>> duplicates = [];
      HashSet<T> visitedLocations = new(collection.Length);
      foreach (var col in collection)
      {
         foreach (var child in col.LocationChildren)
            if (!visitedLocations.Add(child))
            {
               var existingDuplicate = duplicates.FirstOrDefault(kv => kv.Key.Equals(child));
               if (existingDuplicate.Key != null)
                  existingDuplicate.Value.Add(col);
               else
                  duplicates.Add(new(child, [col]!));
            }
      }

      errorsMsgs = [];
      var sb = new StringBuilder();
      foreach (var duplicate in duplicates)
      {
         sb.Clear();
         sb.AppendLine($"Location '{duplicate.Key}' is present in multiple collections:");
         foreach (var col in duplicate.Value)
            sb.AppendLine($" - Collection: '{col}'");
         errorsMsgs.Add(sb.ToString());
      }

      return duplicates.Count == 0;
   }
}