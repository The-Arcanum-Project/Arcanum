namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;

public static class DynamicPredicates
{
   public static bool IsEstateKey(string key)
   {
      return Globals.Estates.ContainsKey(key);
   }
}