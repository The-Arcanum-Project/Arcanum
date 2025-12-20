using System.Text;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

namespace Arcanum.Core.CoreSystems.Common;

/// <summary>
/// A hyper-optimized, allocation-conscious StringBuilder that supports indentation for well-formatted, hierarchical strings.
/// It supports automatic indentation management via IDisposable scopes.
/// </summary>
public class IndentedStringBuilder
{
   private readonly StringBuilder _builder = new();
   private string _indentString = "   "; // 3 spaces per indent level
   private readonly StringBuilder _indentCacheBuilder = new();
   private bool _isAtStartOfLine = true;

   public IndentedStringBuilder(int initialCapacity = 256)
   {
      _builder.EnsureCapacity(initialCapacity);
      _indentCacheBuilder.EnsureCapacity(32); // Preallocate some space for indentation levels
   }

   public void SetIndent(int spaceCount) => _indentString = new(' ', spaceCount);

   /// <summary>
   /// Sets the indentation to a specific level, overriding the current indentation.
   /// </summary>
   /// <param name="level">The desired number of indentation levels (0 for no indent).</param>
   public void SetIndentLevel(int level)
   {
      if (level < 0)
         level = 0;

      _indentCacheBuilder.Clear();

      // Rebuild the cache by appending the indent string 'level' times.
      for (var i = 0; i < level; i++)
         _indentCacheBuilder.Append(_indentString);
   }

   public StringBuilder InnerBuilder => _builder;

   #region High-Performance Merging

   public void Merge(StringBuilder sb)
   {
      // Using ReadOnlySpan<char> is the most direct way to copy chunks.
      foreach (var chunk in _builder.GetChunks())
         sb.Append(chunk.Span);
   }

   public static void Merge(StringBuilder target, StringBuilder source)
   {
      foreach (var chunk in source.GetChunks())
         target.Append(chunk.Span);
   }

   public void Merge(IndentedStringBuilder sb)
   {
      foreach (var chunk in _builder.GetChunks())
         sb.InnerBuilder.Append(chunk.Span);
   }

   #endregion

   #region Configuration Properties (Unchanged)

   public int MaxItemsInCollectionLine { get; set; } = 7;
   public int MaxCollectionLineLength { get; init; } = 130;
   public bool OneItemPerLine { get; init; } = false;
   public bool PadCollectionItems { get; set; } = false;
   public int CollectionItemPadding { get; set; } = 5;
   public bool AutoCollectionPadding { get; set; } = true;

   #endregion

   #region Core Append Logic (Optimized)

   /// <summary> Appends the specified text. Indentation is added automatically. </summary>
   public IndentedStringBuilder Append(string? text)
   {
      if (string.IsNullOrEmpty(text))
         return this;

      PrependIndentIfNecessary();
      _builder.Append(text);
      return this;
   }

   /// <summary> Appends the specified character sequence. Indentation is added automatically. </summary>
   public IndentedStringBuilder Append(ReadOnlySpan<char> text)
   {
      if (text.IsEmpty)
         return this;

      PrependIndentIfNecessary();
      _builder.Append(text);
      return this;
   }

   /// <summary> Appends the specified char. Indentation is added automatically. </summary>
   public IndentedStringBuilder Append(char c)
   {
      PrependIndentIfNecessary();
      _builder.Append(c);
      return this;
   }

   /// <summary> Appends a newline, moving to the next indented line. </summary>
   public IndentedStringBuilder AppendLine()
   {
      _builder.AppendLine();
      _isAtStartOfLine = true;
      return this;
   }

   /// <summary> Appends the specified text, followed by a newline. </summary>
   public IndentedStringBuilder AppendLine(string text)
   {
      PrependIndentIfNecessary();
      _builder.AppendLine(text);
      _isAtStartOfLine = true;
      return this;
   }

   /// <summary> Appends the specified character sequence, followed by a newline. </summary>
   public IndentedStringBuilder AppendLine(ReadOnlySpan<char> text)
   {
      PrependIndentIfNecessary();
      _builder.Append(text);
      _builder.AppendLine();
      _isAtStartOfLine = true;
      return this;
   }

   /// <summary> Appends a formatted comment line without intermediate string allocation. </summary>
   public IndentedStringBuilder AppendComment(string commentChar, string comment)
   {
      PrependIndentIfNecessary();
      _builder.Append(commentChar).Append(' ').Append(comment);
      _builder.AppendLine();
      _isAtStartOfLine = true;
      return this;
   }

   internal void PrependIndentIfNecessary()
   {
      if (_isAtStartOfLine)
      {
         if (_indentCacheBuilder.Length > 0)
            _builder.Append(_indentCacheBuilder);
         _isAtStartOfLine = false;
      }
   }

   #endregion

   #region Indentation Management and Scopes (Optimized)

   private void IncreaseIndent() => _indentCacheBuilder.Append(_indentString);

   private void DecreaseIndent()
   {
      if (_indentCacheBuilder.Length >= _indentString.Length)
         _indentCacheBuilder.Length -= _indentString.Length;
   }

   public IndentScope Indent() => new(this);

   public BlockScope Block(string openingBrace = "{", string closingBrace = "}")
   {
      AppendLine(openingBrace);
      return new(this, closingBrace);
   }

   public BlockScope BlockWithName(string blockName,
                                   string separator = "=",
                                   string openingBrace = "{",
                                   string closingBrace = "}",
                                   bool addNewLineBeforeClosing = false,
                                   bool asOneLine = false)
   {
      Append(blockName).Append(' ').Append(separator).Append(' ').Append(openingBrace);
      if (!asOneLine)
      {
         _builder.AppendLine();
         _isAtStartOfLine = true;
      }

      return new(this, closingBrace, addNewLineBeforeClosing, asOneLine);
   }

   public BlockScope BlockWithName(IAgs ags, SavingFormat format, bool asOneLine)
   {
      var addNewLine = format == SavingFormat.Spacious;
      if (addNewLine && !asOneLine)
      {
         AppendLine();
      }

      var key = ags.SavingKey;
      var separator = SavingUtil.GetSeparator(ags.ClassMetadata.Separator);
      var openingBrace = SavingUtil.GetBrace(ags.ClassMetadata.OpeningToken);
      var closingBrace = SavingUtil.GetBrace(ags.ClassMetadata.ClosingToken);

      PrependIndentIfNecessary();
      _builder.Append(key).Append(' ').Append(separator).Append(' ').Append(openingBrace);
      if (!asOneLine)
      {
         _builder.AppendLine();
         _isAtStartOfLine = true;
      }
      else
         Append(" ");

      return new(this, closingBrace, addNewLine);
   }

   public BlockScope BlockWithNameAndInjection(IAgs ags, InjRepType injRepType, bool asOneLine)
   {
      if (injRepType == InjRepType.None)
         return BlockWithName(ags, ags.AgsSettings.Format, asOneLine);

      var addNewLine = ags.AgsSettings.Format == SavingFormat.Spacious;
      if (addNewLine)
         AppendLine();

      var key = ags.SavingKey;
      var separator = SavingUtil.GetSeparator(ags.ClassMetadata.Separator);
      var openingBrace = SavingUtil.GetBrace(ags.ClassMetadata.OpeningToken);
      var closingBrace = SavingUtil.GetBrace(ags.ClassMetadata.ClosingToken);

      PrependIndentIfNecessary();
      _builder.AppendInjectionType(injRepType).Append(':');
      _builder.Append(key).Append(' ').Append(separator).Append(' ').Append(openingBrace);
      _builder.AppendLine();
      _isAtStartOfLine = true;

      return new(this, closingBrace, addNewLine);
   }

   #endregion

   #region Hyper-Optimized List Appending

   public void AppendList(IReadOnlyList<string> items, string separator)
   {
      if (items == null! || items.Count == 0)
         return;

      if (OneItemPerLine)
      {
         for (var i = 0; i < items.Count; i++)
            AppendLine(items[i]);
         return;
      }

      var padding = 0;
      if (PadCollectionItems)
      {
         if (AutoCollectionPadding)
         {
            var maxLength = 0;
            for (var i = 0; i < items.Count; i++)
            {
               var len = items[i].Length;
               if (len > maxLength)
                  maxLength = len;
            }

            padding = maxLength + 1;
         }
         else
         {
            padding = CollectionItemPadding;
         }
      }

      var lineItemCount = 0;
      PrependIndentIfNecessary();
      var currentLineStartPos = _builder.Length;

      for (var i = 0; i < items.Count; i++)
      {
         var item = items[i];

         var needsLineBreak = (lineItemCount > 0 &&
                               (_builder.Length - currentLineStartPos + item.Length + separator.Length >
                                MaxCollectionLineLength)) ||
                              (lineItemCount >= MaxItemsInCollectionLine);

         if (needsLineBreak)
         {
            _builder.AppendLine();
            _isAtStartOfLine = true;
            PrependIndentIfNecessary();
            currentLineStartPos = _builder.Length;
            lineItemCount = 0;
         }

         if (lineItemCount > 0)
            _builder.Append(separator);

         _builder.Append(item);
         if (PadCollectionItems)
         {
            var padCount = padding - item.Length;
            if (padCount > 0)
               _builder.Append(' ', padCount);
         }

         lineItemCount++;
      }
   }

   #endregion

   public void Clear()
   {
      _builder.Clear();
      _indentCacheBuilder.Clear();
      _isAtStartOfLine = true;
   }

   public override string ToString() => _builder.ToString();

   public readonly ref struct IndentScope
   {
      private readonly IndentedStringBuilder _builder;

      public IndentScope(IndentedStringBuilder builder)
      {
         _builder = builder;
         _builder.IncreaseIndent();
      }

      public void Dispose() => _builder.DecreaseIndent();
   }

   public readonly ref struct BlockScope
   {
      private readonly IndentedStringBuilder _builder;
      private readonly string _closingBrace;
      private readonly bool _addNewLineBeforeClosing;
      private readonly bool _asOneLine;

      public BlockScope(IndentedStringBuilder builder, string closingBrace, bool addNewLineBeforeClosing = false, bool asOneLine = false)
      {
         _asOneLine = asOneLine;
         _builder = builder;
         _closingBrace = closingBrace;
         _addNewLineBeforeClosing = addNewLineBeforeClosing;
         _builder.IncreaseIndent();
      }

      public void Dispose()
      {
         _builder.DecreaseIndent();
         if (_addNewLineBeforeClosing && !_asOneLine)
            _builder.AppendLine();
         _builder.AppendLine(_closingBrace);
      }
   }
}