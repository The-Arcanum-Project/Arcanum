using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Common;

public static class NaturalStringComparer
{
   public static int Compare(string? x, string? y)
   {
      return x switch
      {
         null when y is null => 0,
         null => -1,
         _ => y is null ? 1 : StrCmpLogicalW(x, y),
      };
   }

   [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
   private static extern int StrCmpLogicalW(string psz1, string psz2);
}

public class PathObjComparer : IComparer<PathObj>
{
   public int Compare(PathObj? x, PathObj? y)
   {
      if (ReferenceEquals(x, y))
         return 0;
      if (y is null)
         return 1;
      if (x is null)
         return -1;

      return NaturalStringComparer.Compare(x.Filename, y.Filename);
   }
}

public partial class NaturalStringComparerManual : Comparer<string>, IDisposable
{
   private Dictionary<string, string[]> _table = new();

   public void Dispose()
   {
      _table.Clear();
      _table = null!;
   }

   public override int Compare(string? x, string? y)
   {
      if (x == y)
      {
         return 0;
      }

      if (x == null)
         return -1;
      if (y == null)
         return 1;

      if (!_table.TryGetValue(x, out var x1))
      {
         x1 = MyRegex().Split(x.Replace(" ", ""));
         _table.Add(x, x1);
      }

      if (!_table.TryGetValue(y, out var y1))
      {
         y1 = MyRegex1().Split(y.Replace(" ", ""));
         _table.Add(y, y1);
      }

      for (var i = 0; i < x1.Length && i < y1.Length; i++)
      {
         if (x1[i] != y1[i])
         {
            return PartCompare(x1[i], y1[i]);
         }
      }

      if (y1.Length > x1.Length)
      {
         return 1;
      }

      if (x1.Length > y1.Length)
      {
         return -1;
      }

      return 0;
   }

   private static int PartCompare(string left, string right)
   {
      if (!int.TryParse(left, out var x) || !int.TryParse(right, out var y))
      {
         return string.Compare(left, right, StringComparison.Ordinal);
      }

      return x.CompareTo(y);
   }

   [GeneratedRegex("([0-9]+)")]
   private static partial Regex MyRegex();

   [GeneratedRegex("([0-9]+)")]
   private static partial Regex MyRegex1();
}