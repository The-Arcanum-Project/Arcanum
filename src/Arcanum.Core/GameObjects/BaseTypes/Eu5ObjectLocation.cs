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

   public static Eu5ObjectLocation Empty => new(-1, -1, -1, -1);

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
}