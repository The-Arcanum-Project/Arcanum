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
   public int OverallPos { get; set; } = charPos;

   public override string ToString()
   {
      return $"Line {Line}, Char {Column}, Length {Length}, Overall Pos {OverallPos}";
   }

   public static Eu5ObjectLocation Empty => new(-1, -1, -1, -1);
}