using System.Diagnostics;
using System.Text;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingSystem;

public class Block(string name, int startLine, int index) : IElement
{
   public string Name { get; } = name;
   public bool IsBlock => true;
   public int StartLine { get; } = startLine;
   public int Index { get; } = index;

   public List<Content> ContentElements { get; } = [];
   public List<Block> SubBlocks { get; } = [];
   public int Count => ContentElements.Count + SubBlocks.Count;

   public List<Block> GetSubBlocks(bool onlyBlocks)
   {
      if (onlyBlocks && ContentElements.Count > 0)
      {
         throw new ArgumentException("Expected no content elements in block: " + Name, nameof(onlyBlocks));
      }

      return SubBlocks;
   }

   public List<Content> GetContentElements(bool onlyContent, PathObj po)
   {
      if (onlyContent && SubBlocks.Count > 0)
      {
         // TODO: Update to use the new error handling system
         //_ = new LoadingError(po, $"Expected no subBlocks in block: {Name}!", StartLine, 0, ErrorType.UnexpectedBlockElement);
      }

      return ContentElements;
   }

   public List<LineKvp<string, string>> GetContentLines(PathObj po)
   {
      Debug.Assert(SubBlocks.Count == 0, "SubBlocks should be empty when calling GetContentLines");
      return [..GetContentElements(true, po).SelectMany(c => c.GetLineKvpEnumerator(po)),];
   }

   protected void AppendContent(int tabs, StringBuilder sb)
   {
      var newTabs = tabs + 1;
      for (var i = 0; i < SubBlocks.Count; i++)
         SubBlocks[i].GetFormattedString(newTabs, ref sb);

      for (var i = 0; i < ContentElements.Count; i++)
         ContentElements[i].GetFormattedString(newTabs, ref sb);
   }

   public string GetContent()
   {
      using var psb = StringBuilderPool.Get();
      AppendContent(0, psb.Builder);
      return psb.ToString();
   }

   public string GetFormattedString(int tabs, ref StringBuilder sb)
   {
      AppendFormattedContent(tabs, ref sb);
      return sb.ToString();
   }

   public void AppendFormattedContent(int tabs, ref StringBuilder sb)
   {
      // TODO: Update this to use a new and more efficient way of writing files
      SavingTemp.OpenBlock(ref tabs, Name, ref sb);
      AppendContent(tabs, sb);
      SavingTemp.CloseBlock(ref tabs, ref sb);
   }

   public bool GetSubBlockByName(string name, out Block block)
   {
      return GetBlockByName(name, SubBlocks, out block!);
   }

   public bool GetAllSubBlockByName(string name, out List<Block> blocks)
   {
      return GetAllBlockByName(name, SubBlocks, out blocks);
   }

   /// <summary>
   /// Returns the elements of this block in the order they appear in the file
   /// </summary>
   /// <returns></returns>
   public IEnumerable<IElement> GetElements()
   {
      List<IElement> output = [];
      IElement.MergeInto(SubBlocks, ContentElements, output);
      return output;
   }

   public static bool GetBlockByName(string name, ICollection<Block> blocks, out Block? result)
   {
      result = blocks.FirstOrDefault(b => b.Name.Equals(name));
      return result is not null;
   }

   public static bool GetAllBlockByName(string name, ICollection<Block> blocks, out List<Block> result)
   {
      result = blocks.Where(b => b.Name.Equals(name)).ToList();
      return result.Count > 0;
   }

   public bool GetSubBlocksByName(string name, out List<Block> blocks)
   {
      return GetBlocksByName(name, SubBlocks, out blocks);
   }

   public static bool GetBlocksByName(string name, ICollection<Block> blocks, out List<Block> result)
   {
      result = blocks.Where(b => b.Name == name).ToList();
      return result.Count > 0;
   }

   public override string ToString()
   {
      return Name;
   }
}