using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Arcanum.Core.Utils.Parsing;

public static partial class StringOps
{
   public static List<(string, string)> SplitLocationColors(string path, int estimate = -1)
   {
      List<(string, string)> results;
      if (estimate > 0)
         results = new(estimate);
      else
         results = [];

      var regex = KvpRegex();
      using var reader = new StreamReader(path);

      while (reader.ReadLine() is { } line)
      {
         var match = regex.Match(line);
         if (match.Success)
         {
            var key = match.Groups[1].Value;
            var value = match.Groups[2].Value;
            results.Add((key, value));
         }
      }
      
      return results;
   }

   [GeneratedRegex(@"^(?:[^#\r\n]*)\b(\w+)\s*=\s*([\da-f]+)", RegexOptions.Compiled)]
   private static partial Regex KvpRegex();
}