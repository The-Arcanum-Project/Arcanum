using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Arcanum.Core.CoreSystems.ParsingSystem;

public readonly struct LineKvp<T, TQ>
{
   public LineKvp(T key, TQ value, int line)
   {
      Key = key;
      Value = value;
      Line = line;
   }

   public T Key { get; }
   public TQ Value { get; }
   public int Line { get; }

   public void Deconstruct(out T key, out TQ value, out int line)
   {
      key = Key;
      value = Value;
      line = Line;
   }

   public override string ToString() => $"{Key} = {Value}";

   public static implicit operator LineKvp<T, TQ>((T key, TQ value, int line) tuple)
      => new(tuple.key, tuple.value, tuple.line);

   public override bool Equals([NotNullWhen(true)] object? obj)
   {
      if (obj is LineKvp<T, TQ> kvp)
         return Key != null && Key.Equals(kvp.Key) && Value != null && Value.Equals(kvp.Value) && Line == kvp.Line;

      return false;
   }

   public override int GetHashCode()
   {
      var hash = new HashCode();
      hash.Add(Key);
      hash.Add(Value);
      hash.Add(Line);
      return hash.ToHashCode();
   }
}

public static class StringExtensions
{
   public static string TrimQuotes(this string str)
   {
      if (string.IsNullOrEmpty(str))
         return string.Empty;
      return str is ['"', .., '"'] ? str[1..^1] : str.Trim();
   }
}

public static class SavingTemp
{
   public static void AddString(ref int tabs, string s, string stringName, ref StringBuilder sb)
   {
      if (string.IsNullOrEmpty(s))
         return;

      AddTabs(ref tabs, ref sb);
      sb.AppendLine($"{stringName} = {s}");
   }

   public static void AddTabs(ref int tabs, ref StringBuilder sb)
   {
      for (var i = 0; i < tabs; i++)
         sb.Append('\t');
   }

   public static void OpenBlock(ref int tabs, string blockName, ref StringBuilder sb)
   {
      AddTabs(ref tabs, ref sb);
      sb.AppendLine($"{blockName} = {{");
      tabs++;
   }

   public static void CloseBlock(ref int tabs, ref StringBuilder sb)
   {
      tabs--;
      AddTabs(ref tabs, ref sb);
      sb.AppendLine("}");
   }
}