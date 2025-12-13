using System.Text;

namespace ParserGenerator.NexusGeneration;

public class IndentBuilder
{
   private readonly StringBuilder _sb;
   private readonly string _indentUnit;
   private int _indentLevel;
   private bool _isStartOfLine;

   /// <summary>
   /// Initializes a new instance of the IndentBuilder.
   /// </summary>
   /// <param name="indentUnit">The string to use for one level of indentation (default is 4 spaces).</param>
   public IndentBuilder(string indentUnit = "   ")
   {
      _sb = new StringBuilder();
      _indentUnit = indentUnit;
      _indentLevel = 0;
      _isStartOfLine = true;
   }

   public StringBuilder InnerBuilder => _sb;

   /// <summary>
   /// Appends text. Applies indentation only if currently at the start of a line.
   /// </summary>
   public IndentBuilder Append(string value)
   {
      if (_isStartOfLine && !string.IsNullOrEmpty(value))
         WriteIndent();

      _sb.Append(value);
      return this;
   }

   /// <summary>
   /// Appends text followed by a new line. Handles indentation automatically.
   /// </summary>
   public IndentBuilder AppendLine(string value)
   {
      if (_isStartOfLine)
         WriteIndent();

      _sb.AppendLine(value);
      _isStartOfLine = true;
      return this;
   }

   /// <summary>
   /// Appends an empty new line.
   /// </summary>
   public IndentBuilder AppendLine()
   {
      _sb.AppendLine();
      _isStartOfLine = true;
      return this;
   }

   /// <summary>
   /// Appends formatted string.
   /// </summary>
   public IndentBuilder AppendFormat(string format, params object[] args)
   {
      if (_isStartOfLine)
         WriteIndent();

      _sb.AppendFormat(format, args);
      return this;
   }

   /// <summary>
   /// Creates a disposable scope that increases indentation.
   /// When the scope is disposed (end of 'using'), indentation decreases.
   /// </summary>
   public IDisposable Indent()
   {
      _indentLevel++;
      return new IndentDisposable(this);
   }

   /// <summary>
   /// Helper: Writes "{", indents, runs the block, unindents, and writes "}".
   /// </summary>
   public void Block(Action<IndentBuilder> content)
   {
      AppendLine("{");
      using (Indent())
         content(this);

      AppendLine("}");
   }

   public override string ToString() => _sb.ToString();

   public void Clear()
   {
      _sb.Clear();
      _indentLevel = 0;
      _isStartOfLine = true;
   }

   private void WriteIndent()
   {
      for (var i = 0; i < _indentLevel; i++)
         _sb.Append(_indentUnit);

      _isStartOfLine = false;
   }

   /// <summary>
   /// Creates a disposable scope that wraps content in #region and #endregion.
   /// Note: This does not automatically increase indentation, matching standard VS behavior.
   /// </summary>
   public IDisposable Region(string name)
   {
      _sb.AppendLine($"#region {name}");
      return new RegionDisposable(this);
   }

   private readonly struct IndentDisposable(IndentBuilder builder) : IDisposable
   {
      public void Dispose()
      {
         if (builder._indentLevel > 0)
            builder._indentLevel--;
      }
   }

   private readonly struct RegionDisposable(IndentBuilder builder) : IDisposable
   {
      public void Dispose() => builder.InnerBuilder.AppendLine("#endregion");
   }
}