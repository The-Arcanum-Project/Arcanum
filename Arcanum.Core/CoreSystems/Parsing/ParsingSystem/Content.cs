using System.Text;
using Arcanum.Core.CoreSystems.SavingSystem;

namespace Arcanum.Core.CoreSystems.ParsingSystem;

public class Content(string value, int startLine, int index) : IElement
{
   public string Value { get; set; } = value;
   public bool IsBlock { get; } = false;
   public int StartLine { get; } = startLine;
   public int Index { get; } = index;

   public string GetContent() => Value;

   public string GetFormattedString(int tabs, ref StringBuilder sb)
   {
      AppendFormattedContent(tabs, ref sb);
      return sb.ToString();
   }

   public void AppendFormattedContent(int tabs, ref StringBuilder sb)
   {
      var enumerator = GetLineKvpEnumerator(PathObj.Empty, false);
      foreach (var kvp in enumerator)
         SavingTemp.AddString(ref tabs, kvp.Value, kvp.Key, ref sb);
   }

   public IEnumerable<(string, int)> GetLineEnumerator() 
   {
      var lines = Value.Split('\n');
      var lineNum = StartLine;
      foreach (var line in lines)
      {
         if (string.IsNullOrWhiteSpace(line))
         {
            lineNum++;
            continue;
         }
         yield return (line, lineNum);
         lineNum++;
      }
   }
   
   public IEnumerable<(string, int)> GetStringListEnumerator()
   {
      var lines = Value.Split('\n');
      var lineNum = StartLine;
      foreach (var line in lines)
      {
         if (string.IsNullOrWhiteSpace(line))
         {
            lineNum++;
            continue;
         }
         var strings = line.Split(' ');
         foreach (var str in strings)
            yield return (str, lineNum);
         lineNum++;
      }
   }
   
   public IEnumerable<LineKvp<string, string>> GetLineKvpEnumerator(PathObj po,
                                                                    bool showError = true,
                                                                    bool trimQuotes = true)
   {
      var lines = Value.Split('\n');
      var lineNum = StartLine;
      foreach (var line in lines)
      {
         if (string.IsNullOrWhiteSpace(line))
         {
            lineNum++;
            continue;
         }

         foreach (var kvps in line.Split('\t'))
         {
            var split = kvps.Split('=');
            if (split.Length < 2)
            {
               if (showError)
                  // TODO: Update to use the new error handling system
                  Console.WriteLine($"Error in file x: Invalid line format at line {lineNum}: '{line}'");

               continue;
            }

            yield return new(split[0].Trim(), trimQuotes ? split[1].TrimQuotes() : split[1].Trim(), lineNum);
         }

         lineNum++;
      }
   }

   public override string ToString()
   {
      return Value;
   }
}