using System.Diagnostics;
using System.IO;
using System.Text;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.GameObjects.BaseTypes;

/// <summary>
/// Holds the char pos, line, length, and overall pos of this object in the file it was loaded from.
/// </summary>
public class Eu5ObjectLocation(int colum, int line, int length, int charPos)
{
   /// <summary>
   /// The character position in the line where this object starts.
   /// </summary>
   public int Column { get; set; } = colum;

   /// <summary>
   /// The line number in the file where this object starts.
   /// </summary>
   public int Line { get; set; } = line;

   /// <summary>
   /// The length in characters of this object in the file.
   /// </summary>
   public int Length { get; set; } = length;

   /// <summary>
   /// The overall character position in the file where this object starts.
   /// </summary>
   public int CharPos { get; set; } = charPos;

   public override string ToString()
   {
      return $"Line {Line}, Char {Column}, Length {Length}, Overall Pos {CharPos}";
   }

   public static Eu5ObjectLocation Empty { get; } = new(-1, -1, -1, -1);

   public void Update(int length, int line, int column, int charPos)
   {
      // The delta we get contains additional whitespaces from formatting so we need to subtract the column to get the actual length of the object.
      // And we need to add the column to the char pos to get the correct overall position where the object starts.
      Length = length - column;
      Line = line;
      Column = column;
      CharPos += charPos + column;
   }

   public LocationContext ToLocationContext(Eu5FileObj source)
   {
      return new(Line, Column, source.Path.FullPath);
   }

   // Writes a file where each object in said file is a "claiming" is space in the file marking each line and character position of the object.
   public static void Visualize(Eu5FileObj fo)
   {
      if (string.IsNullOrEmpty(fo.Path.FullPath))
      {
         Debug.WriteLine("Visualize: Input Eu5FileObj or its path is null.");
         return;
      }

      try
      {
         var originalText = File.ReadAllText(fo.Path.FullPath);
         var sb = new StringBuilder();

         var canvas = originalText.ToCharArray();
         for (var i = 0; i < canvas.Length; i++)
            if (!char.IsWhiteSpace(canvas[i]))
               canvas[i] = '.';

         var sortedObjects = fo.ObjectsInFile
                               .Where(o => o.FileLocation != Empty && o.FileLocation.CharPos != -1)
                               .OrderBy(o => o.FileLocation.CharPos)
                               .ToList();

         if (sortedObjects.Count == 0)
         {
            sb.AppendLine("No objects with valid locations found in this file.");
            File.WriteAllText("object_locations.txt", sb.ToString());
            return;
         }

         const string identifiers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
         var warnings = new List<string>();

         for (var i = 0; i < sortedObjects.Count; i++)
         {
            var obj = sortedObjects[i];
            var loc = obj.FileLocation;
            var id = identifiers[i % identifiers.Length];

            // Check for overlaps with the previous object
            if (i > 0)
            {
               var prevObj = sortedObjects[i - 1];
               var prevLoc = prevObj.FileLocation;
               if (loc.CharPos < prevLoc.CharPos + prevLoc.Length)
                  warnings.Add($"[WARNING] Object '{id}' ({obj.UniqueId}) overlaps with previous object.");
            }

            // Boundary check to prevent errors from invalid location data
            var endPos = loc.CharPos + loc.Length;
            if (endPos > canvas.Length)
            {
               warnings.Add($"[ERROR] Object '{id}' ({obj.UniqueId}) location is OUT OF BOUNDS. At: {loc.CharPos} Len: {loc.Length}, FileLen: {canvas.Length}");
               endPos = canvas.Length; // Clamp to the end of the file to avoid crashing
            }

            // Paint the identifier over the canvas
            for (var charIndex = loc.CharPos; charIndex < endPos; charIndex++)
               if (canvas[charIndex] != '\n' && canvas[charIndex] != '\r')
                  canvas[charIndex] = id;
               else
                  canvas[charIndex] = '\n'; // Preserve newlines
         }

         sb.AppendLine($"Visualization for: {fo.Path.FullPath}");
         sb.AppendLine("Each character represents an object. '.' means unclaimed space (excluding whitespace).");
         sb.AppendLine();

         if (warnings.Any())
         {
            sb.AppendLine("--- ISSUES DETECTED ---");
            foreach (var warning in warnings)
               sb.AppendLine(warning);

            sb.AppendLine("-----------------------");
         }

         sb.AppendLine("\n--- Visualization Map ---");
         sb.Append(new string(canvas));

         File.WriteAllText("object_locations.txt", sb.ToString());

         Process.Start(new ProcessStartInfo("object_locations.txt") { UseShellExecute = true });
      }
      catch (FileNotFoundException)
      {
         Debug.WriteLine($"Visualize: Could not find the file at {fo.Path.FullPath}");
         File.WriteAllText("object_locations.txt", $"ERROR: Could not find the file at {fo.Path.FullPath}");
      }
      catch (Exception ex)
      {
         Debug.WriteLine($"Visualize: An unexpected error occurred: {ex.Message}");
         File.WriteAllText("object_locations.txt", $"An unexpected error occurred: {ex.Message}\n\n{ex.StackTrace}");
      }
   }
}