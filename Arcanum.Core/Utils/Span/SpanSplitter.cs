namespace Arcanum.Core.Utils.Span;

public ref struct SpanSplitter(ReadOnlySpan<char> span, char separator)
{
   private ReadOnlySpan<char> _span = span;
   private int _pos = 0;

   public ReadOnlySpan<char> Current { get; private set; }

   public bool MoveNext()
   {
      if (_pos > _span.Length)
         return false;

      var remaining = _span[_pos..];
      var idx = remaining.IndexOf(separator);
      if (idx == -1)
      {
         Current = remaining;
         _pos = _span.Length + 1;
         return true;
      }

      Current = remaining[..idx];
      _pos += idx + 1;
      return true;
   }
}