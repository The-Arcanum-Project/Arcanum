using System.Globalization;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.PathAccessorEngine;

public class FileSorter
{
   public Type TargetType { get; init; }
   public PathAccessor PathAccessor { get; init; }

   public FileSorter(Type targetType, PathAccessor pathAccessor)
   {
      TargetType = targetType;
      PathAccessor = pathAccessor;
   }

   public List<IEu5Object> SortFile(List<IEu5Object> objects)
   {
      SortedDictionary<string, IEu5Object> sortedList = new(new AlphanumComparer());

      foreach (var obj in objects)
      {
         var value = PathAccessor.GetValueForSorting(obj);
         var key = value.ToString() ?? string.Empty;

         if (key == "null")
            continue;

         sortedList[key] = obj;
      }

      return sortedList.Values.ToList();
   }

   /// <summary>
   /// Groups files by the value obtained from the PathAccessor.
   /// </summary>
   /// <param name="objects"></param>
   /// <returns></returns>
   public SortedDictionary<string, IEu5Object> GroupFiles(List<IEu5Object> objects)
   {
      SortedDictionary<string, IEu5Object> groupedFiles = new(new AlphanumComparer());

      foreach (var obj in objects)
      {
         var value = PathAccessor.GetValueForSorting(obj);
         var key = value.ToString() ?? string.Empty;

         if (key == "null")
            continue;

         groupedFiles.TryAdd(key, obj);
      }

      return groupedFiles;
   }
}

internal class AlphanumComparer : IComparer<string>
{
   public int Compare(string? x, string? y)
   {
      return CultureInfo.InvariantCulture.CompareInfo
                        .Compare(x,
                                 y,
                                 CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols | CompareOptions.StringSort);
   }
}