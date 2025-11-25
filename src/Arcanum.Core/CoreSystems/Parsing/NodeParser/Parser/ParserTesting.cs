using System.IO;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

public enum FilesToTest
{
   ArrayDeclarations,
   Block,
   BlockWithContent,
   BlockWithSeveralContents,
   BracesOnNextLine,
   ColorCases,
   ComplexCase,
   ContentSeparators,
   InlineMath,
   NestedBlocks,
}

public static class ParserTesting
{
   public static string GetFilePath(FilesToTest file)
   {
      const string location =
         @"C:\Users\david\source\repos\Arcanum\src\Arcanum.Core\CoreSystems\Parsing\NodeParser\TestFiles";
      return Path.Combine(location, file + ".txt");
   }
}