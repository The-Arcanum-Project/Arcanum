using System.IO;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Common;

/// <summary>
/// This context will be modified and passed around during the parsing process.
/// If an error occurs, it will be used to provide a DiagnosticDescriptor with information.
/// </summary>
public class LocationContext(int lineNumber, int columnNumber, string filePath)
{
   public int LineNumber { get; set; } = lineNumber;
   public int ColumnNumber { get; set; } = columnNumber;

   public string FilePath { get; set; } = filePath;
   /// <summary>
   /// The current action being performed during parsing.
   /// </summary>
   public string ToErrorString => $"in File \"{FilePath}\" at Line {LineNumber}:{ColumnNumber}";
   public static LocationContext Empty { get; } = new(int.MinValue, int.MinValue, string.Empty);

   public LocationContext GetInstance() => new(LineNumber, ColumnNumber, FilePath);

   public override string ToString()
   {
      return $"File: {Path.GetFileName(FilePath)}, Line: {LineNumber}, Column: {ColumnNumber}";
   }

   public void SetPosition(Token token)
   {
      LineNumber = token.Line;
      ColumnNumber = token.Column;
   }

   public void SetPosition(KeyNodeBase vn)
   {
      var (line, column) = vn.GetLocation();
      LineNumber = line;
      ColumnNumber = column;
   }

   public void SetPosition(ValueNode vn)
   {
      var (line, column) = vn.GetLocation();
      LineNumber = line;
      ColumnNumber = column;
   }

   public static LocationContext GetNew(Eu5FileObj fileObj) => new(0, 0, fileObj.Path.FullPath);
}