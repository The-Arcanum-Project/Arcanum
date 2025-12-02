using System.Text;

namespace ParserGenerator.ParserGen;

public static class IntegerCheckGenerator
{
   public static string Generate(string keyword, string padding)
   {
      var sb = new StringBuilder();
      var bytes = Encoding.Unicode.GetBytes(keyword);

      var offset = 0;
      var remainingBytes = bytes.Length;
      var isFirst = true;

      // 1. 64-bit chunks
      while (remainingBytes >= 8)
      {
         var val = BitConverter.ToUInt64(bytes, offset);
         AppendCheck("ulong", "UL", 8, $"{val:X}");
      }

      // 2. 32-bit chunks
      if (remainingBytes >= 4)
      {
         var val = BitConverter.ToUInt32(bytes, offset);
         AppendCheck("uint", "U", 4, $"{val:X}");
      }

      // 3. 16-bit chunks
      if (remainingBytes >= 2)
      {
         var val = BitConverter.ToUInt16(bytes, offset);
         AppendCheck("ushort", "", 2, $"{val:X}");
      }

      return sb.ToString();

      void AppendCheck(string type, string suffix, int size, object hexValue)
      {
         if (!isFirst)
            sb.Append($"\n{padding}&& ");

         if (offset == 0)
            sb.Append($"*({type}*)ptr == 0x{hexValue}{suffix}");
         else
            sb.Append($"*({type}*)(ptr + {offset}) == 0x{hexValue}{suffix}");

         offset += size;
         remainingBytes -= size;
         isFirst = false;
      }
   }
}