using System.IO;

namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser;

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
         @"C:\Users\david\source\repos\Arcanum\Arcanum.Core\CoreSystems\Parsing\CeasarParser\TestFiles";
      return Path.Combine(location, file + ".txt");
   }
}