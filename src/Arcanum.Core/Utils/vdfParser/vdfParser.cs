using System.IO;
using System.Text.RegularExpressions;
using Arcanum.Core.CoreSystems.IO;

namespace Arcanum.Core.Utils.vdfParser;

public static partial class VdfParser
{
   private const string EU5_STEAM_ID = "3450310";
   private const string EU5_FOLDER_NAME = "Europa Universalis V";

   // private const string EU4_STEAM_ID = "236850";
   // private const string EU4_FOLDER_NAME = "Europa Universalis IV";

   // TODO Fix this to use the correct path for EU5
   public static string GetEu5Path()
   {
      var libraries = GetLibrariesAndGames();
      if (libraries.Count == 0)
         return string.Empty;

      var libPath = libraries.FirstOrDefault(x => x.Value.Contains(EU5_STEAM_ID)).Key;
      if (string.IsNullOrEmpty(libPath))
         return string.Empty;

      return Path.Combine(libPath, "steamapps", "common", EU5_FOLDER_NAME).Replace(@"\\", @"\");
   }

   public static Dictionary<string, List<string>> GetLibrariesAndGames()
   {
      var path = GetSteamLibrariesVdfPath;

      // Steam is not installed or at least not in the default location
      if (!File.Exists(path))
         return [];

      var lines = IO.ReadAllLinesUtf8(GetSteamLibrariesVdfPath);
      if (lines?.Length == 0)
         return [];

      return ParseVdfFile(lines!);
   }

   private static Dictionary<string, List<string>> ParseVdfFile(string[] lines)
   {
      var libraries = new Dictionary<string, List<string>>();

      var currentLibrary = string.Empty;
      var getPathRegex = GetPathRegex();
      var getAppsRegex = GetAppsRegex();

      var isInAppsSection = false;

      foreach (var raw in lines)
      {
         var line = raw.Trim();

         if (line.StartsWith("\"path\""))
         {
            var matches = getPathRegex.Matches(line);
            if (matches.Count != 2)
               throw new FormatException($"Can not parse 'path' from .vdf file: {line}");

            currentLibrary = matches[1].Groups[1].Value;
            if (!libraries.ContainsKey(currentLibrary))
               libraries[currentLibrary] = [];
         }
         else if (line.Equals("\"apps\""))
            isInAppsSection = true;
         else if (line.Equals("}") && isInAppsSection)
            isInAppsSection = false;
         else if (isInAppsSection)
         {
            var matches = getAppsRegex.Matches(line);
            if (matches.Count == 0)
               continue;

            libraries[currentLibrary].Add(matches[0].Groups[1].Value);
         }
      }

      return libraries;
   }

   private static string GetSteamLibrariesVdfPath
      => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                      "Steam",
                      "steamapps",
                      "libraryfolders.vdf");

   [GeneratedRegex(@"""([^""]*)""")]
   private static partial Regex GetPathRegex();

   [GeneratedRegex(@"""(\d*)""\s+""\d*""")]
   private static partial Regex GetAppsRegex();
}