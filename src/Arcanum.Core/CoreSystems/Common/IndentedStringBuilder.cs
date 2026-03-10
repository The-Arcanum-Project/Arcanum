#region

using System.Globalization;
using System.Text;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.Serialization;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

#endregion

namespace Arcanum.Core.CoreSystems.Common;

/// <summary>
///    A hyper-optimized, allocation-conscious StringBuilder that supports indentation for well-formatted, hierarchical
///    strings.
///    It supports automatic indentation management via IDisposable scopes.
/// </summary>
public class IndentedStringBuilder
{
   private readonly StringBuilder _indentCacheBuilder = new();
   private string _indentString = new(' ', Config.Settings.SavingConfig.SpacesPerIndent);
   private bool _isAtStartOfLine = true;
   private string _spacerString = new(' ', Config.Settings.SavingConfig.SpacesPerSpacing);

   public IndentedStringBuilder(int initialCapacity = 256)
   {
      InnerBuilder.EnsureCapacity(initialCapacity);
      _indentCacheBuilder.EnsureCapacity(32); // Preallocate some space for indentation levels
   }

   public StringBuilder InnerBuilder { get; } = new();

   public int this[Index index]
   {
      get => InnerBuilder[index];
      set => InnerBuilder[index] = (char)value;
   }

   public void SetIndent(int spaceCount) => _indentString = new(' ', spaceCount);

   /// <summary>
   ///    Sets the indentation to a specific level, overriding the current indentation.
   /// </summary>
   /// <param name = "level" >The desired number of indentation levels (0 for no indent).</param>
   public void SetIndentLevel(int level)
   {
      if (level < 0)
         level = 0;

      _indentCacheBuilder.Clear();

      // Rebuild the cache by appending the indent string 'level' times.
      for (var i = 0; i < level; i++)
         _indentCacheBuilder.Append(_indentString);
   }

   public void SetSpacer(int spaceCount) => _spacerString = new(' ', spaceCount);

   public IndentedStringBuilder AppendSpacer() => Append(_spacerString);

   #region List Appending

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
            padding = CollectionItemPadding;
      }

      var lineItemCount = 0;
      PrependIndentIfNecessary();
      var currentLineStartPos = InnerBuilder.Length;

      for (var i = 0; i < items.Count; i++)
      {
         var item = items[i];

         var needsLineBreak = (lineItemCount > 0 &&
                               InnerBuilder.Length - currentLineStartPos + item.Length + separator.Length >
                               MaxCollectionLineLength) ||
                              lineItemCount >= MaxItemsInCollectionLine;

         if (needsLineBreak)
         {
            InnerBuilder.AppendLine();
            _isAtStartOfLine = true;
            PrependIndentIfNecessary();
            currentLineStartPos = InnerBuilder.Length;
            lineItemCount = 0;
         }

         if (lineItemCount > 0)
            InnerBuilder.Append(separator);

         InnerBuilder.Append(item);
         if (PadCollectionItems)
         {
            var padCount = padding - item.Length;
            if (padCount > 0)
               InnerBuilder.Append(' ', padCount);
         }

         lineItemCount++;
      }
   }

   #endregion

   public IndentedStringBuilder AppendOpeningBrace(string brace = "{", string separator = "=", bool asOneLine = false, bool isArray = false)
   {
      if (asOneLine)
      {
         if (!isArray)
            AppendSpacer().AppendSeparator(separator);
         return Append(brace);
      }

      return Config.Settings.SavingConfig.OpeningBraceLocation switch
      {
         BraceLocation.SameLine => AppendSeparator(separator).Append(brace),
         BraceLocation.NewLine => AppendSeparator(separator).AppendLine().Append(brace),
         BraceLocation.NewLineWithEquals => AppendLine().Append(separator).AppendSpacer().Append(brace),
         _ => throw new ArgumentOutOfRangeException(),
      };
   }

   public IndentedStringBuilder AppendClosingBrace(char brace = '}', bool asOneLine = false)
      => asOneLine ? AppendSpacer().Append(brace) : AppendLine().Append(brace);

   public IndentedStringBuilder AppendSeparator(char separator = '=') => AppendSpacer().Append(separator).AppendSpacer();

   public IndentedStringBuilder AppendSeparator(string separator) => AppendSpacer().Append(separator).AppendSpacer();

   public void Clear()
   {
      InnerBuilder.Clear();
      _indentCacheBuilder.Clear();
      _isAtStartOfLine = true;
   }

   public override string ToString() => InnerBuilder.ToString();

   private void AppendIfNotInComment(string text)
   {
      var hasComment = false;

      for (var i = InnerBuilder.Length - 1; i >= 0; i--)
      {
         var c = InnerBuilder[i];

         if (c == '#')
         {
            hasComment = true;
            break;
         }

         // We reached the start of the line without finding a #
         if (c == '\n')
            break;
      }

      // Line has a comment, start a new line to keep 'text' clean
      if (hasComment)
         InnerBuilder.AppendLine().Append(text);
      // Line is "clean", append directly to the current line
      else
         InnerBuilder.Append(text);
   }

   private void AppendLineIfNotInComment(string text)
   {
      var hasComment = false;

      for (var i = InnerBuilder.Length - 1; i >= 0; i--)
      {
         var c = InnerBuilder[i];

         if (c == '#')
         {
            hasComment = true;
            break;
         }

         // We reached the start of the line without finding a #
         if (c == '\n')
            break;
      }

      // Line has a comment, start a new line to keep 'text' clean
      if (hasComment)
         InnerBuilder.AppendLine().AppendLine(text);
      // Line is "clean", append directly to the current line
      else
         InnerBuilder.AppendLine(text);
   }

   public void AppendInjRepType(InjRepType agsInjRepType)
   {
      InnerBuilder.Append(SavingUtil.FormatInjectionType(agsInjRepType)).Append(':');
   }

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
         if (_asOneLine)
            _builder.AppendIfNotInComment(_closingBrace);
         else
            _builder.AppendLineIfNotInComment(_closingBrace);
      }
   }

   #region High-Performance Merging

   public void Merge(StringBuilder sb)
   {
      // Using ReadOnlySpan<char> is the most direct way to copy chunks.
      foreach (var chunk in InnerBuilder.GetChunks())
         sb.Append(chunk.Span);
   }

   public static void Merge(StringBuilder target, StringBuilder source)
   {
      foreach (var chunk in source.GetChunks())
         target.Append(chunk.Span);
   }

   public void Merge(IndentedStringBuilder sb)
   {
      foreach (var chunk in InnerBuilder.GetChunks())
         sb.InnerBuilder.Append(chunk.Span);
   }

   #endregion

   #region Configuration Properties (Unchanged)

   public int MaxItemsInCollectionLine { get; set; } = 7;
   public int MaxCollectionLineLength { get; init; } = 250;
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
      InnerBuilder.Append(text);
      return this;
   }

   /// <summary> Appends the specified character sequence. Indentation is added automatically. </summary>
   public IndentedStringBuilder Append(ReadOnlySpan<char> text)
   {
      if (text.IsEmpty)
         return this;

      PrependIndentIfNecessary();
      InnerBuilder.Append(text);
      return this;
   }

   /// <summary> Appends the specified char. Indentation is added automatically. </summary>
   public IndentedStringBuilder Append(char c)
   {
      PrependIndentIfNecessary();
      InnerBuilder.Append(c);
      return this;
   }

   /// <summary> Appends a newline, moving to the next indented line. </summary>
   public IndentedStringBuilder AppendLine()
   {
      InnerBuilder.AppendLine();
      _isAtStartOfLine = true;
      return this;
   }

   /// <summary> Appends the specified text, followed by a newline. </summary>
   public IndentedStringBuilder AppendLine(string text)
   {
      PrependIndentIfNecessary();
      InnerBuilder.AppendLine(text);
      _isAtStartOfLine = true;
      return this;
   }

   public IndentedStringBuilder AppendCommentLine(string comment, string commentChar = "#")
   {
      PrependIndentIfNecessary();
      if (!comment.StartsWith(commentChar))
         InnerBuilder.Append(commentChar);
      InnerBuilder.Append(' ').Append(comment);
      InnerBuilder.AppendLine();
      _isAtStartOfLine = true;
      return this;
   }

   /// <summary> Appends the specified character sequence, followed by a newline. </summary>
   public IndentedStringBuilder AppendLine(ReadOnlySpan<char> text)
   {
      PrependIndentIfNecessary();
      InnerBuilder.Append(text);
      InnerBuilder.AppendLine();
      _isAtStartOfLine = true;
      return this;
   }

   /// <summary> Appends a formatted comment line without intermediate string allocation. </summary>
   public IndentedStringBuilder AppendComment(string commentChar, string comment)
   {
      PrependIndentIfNecessary();
      InnerBuilder.Append(commentChar).Append(' ').Append(comment);
      InnerBuilder.AppendLine();
      _isAtStartOfLine = true;
      return this;
   }

   internal void PrependIndentIfNecessary()
   {
      if (_isAtStartOfLine)
      {
         if (_indentCacheBuilder.Length > 0)
            InnerBuilder.Append(_indentCacheBuilder);
         _isAtStartOfLine = false;
      }
   }

   public IndentedStringBuilder AppendLineFormat(PropertySavingMetadata? meta, bool asOneLine)
   {
      if (meta == null)
      {
         // no metadata => between objects
         if (!asOneLine)
            EnforceNewLineCount(Config.Settings.SavingConfig.NewLinesBetweenObjects);
         return this;
      }

      // if we are a collection or embedded object and not inlined, we want block rule
      if ((meta.IsCollection || meta.IsEmbeddedObject) && !asOneLine)
      {
         EnforceNewLineCount(Config.Settings.SavingConfig.NewLinesBeforeBlock);
         return this;
      }

      // we are a normal property and if we are inlined we don't want any new lines
      EnforceNewLineCount(asOneLine ? 0 : Config.Settings.SavingConfig.NewLinesBetweenProperties);
      return this;
   }

   public IndentedStringBuilder AppendBlockNewLines() => EnforceNewLineCount(Config.Settings.SavingConfig.NewLinesBeforeBlock);

   public IndentedStringBuilder AppendNewLineIfNone() => EnforceNewLineCount(1, false);

   // Removes or adds new lines to ensure the specified count of consecutive new lines at the end of the builder.
   public IndentedStringBuilder EnforceNewLineCount(int count, bool applllyFallback = true)
   {
      if (count < 0)
         count = 0;

      var existingNewLines = 0;
      for (var i = InnerBuilder.Length - 1; i >= 0; i--)
         if (InnerBuilder[i] == '\n')
            existingNewLines++;
         else if (InnerBuilder[i] != '\r')
            break;

      if (existingNewLines < count)
      {
         var toAdd = count - existingNewLines;
         for (var i = 0; i < toAdd; i++)
            InnerBuilder.AppendLine();
         _isAtStartOfLine = true;
      }
      else if (existingNewLines > count)
      {
         var toRemove = existingNewLines - count;
         var newLength = InnerBuilder.Length;

         for (var i = InnerBuilder.Length - 1; i >= 0 && toRemove > 0; i--)
            if (InnerBuilder[i] == '\n')
            {
               newLength--;
               toRemove--;
            }
            else if (InnerBuilder[i] == '\r')
               newLength--;
            else
               break;

         InnerBuilder.Length = newLength;
         _isAtStartOfLine = true;
      }

      if (applllyFallback && InnerBuilder.Length > 0 && InnerBuilder[^1] != '\n')
         AppendSpacer();

      return this;
   }

   public IndentedStringBuilder AppendPropertyNewLineOrSpacer(bool asOneLine)
      => asOneLine ? AppendSpacer() : EnforceNewLineCount(Config.Settings.SavingConfig.NewLinesBetweenProperties);

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
         InnerBuilder.AppendLine();
         _isAtStartOfLine = true;
      }

      return new(this, closingBrace, addNewLineBeforeClosing, asOneLine);
   }

   public BlockScope BlockWithName(IAgs ags, SavingFormat format, bool asOneLine)
   {
      var addNewLine = format == SavingFormat.Spacious;
      if (addNewLine && !asOneLine)
         AppendLine();

      var key = ags.SavingKey;
      var separator = SavingUtil.GetSeparator(ags.ClassMetadata.Separator);
      var openingBrace = SavingUtil.GetBrace(ags.ClassMetadata.OpeningToken);
      var closingBrace = SavingUtil.GetBrace(ags.ClassMetadata.ClosingToken);

      PrependIndentIfNecessary();
      InnerBuilder.Append(key).Append(' ').Append(separator).Append(' ').Append(openingBrace);
      if (!asOneLine)
      {
         InnerBuilder.AppendLine();
         _isAtStartOfLine = true;
      }
      else
         Append(" ");

      return new(this, closingBrace, addNewLine, asOneLine);
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
      InnerBuilder.AppendInjectionType(injRepType).Append(':');
      InnerBuilder.Append(key).Append(' ').Append(separator).Append(' ').Append(openingBrace);
      InnerBuilder.AppendLine();
      _isAtStartOfLine = true;

      return new(this, closingBrace, addNewLine, asOneLine);
   }

   #endregion

   #region Primitive Append Methods

   /// <summary> Appends an integer. Indentation is added automatically. </summary>
   public IndentedStringBuilder Append(int value)
   {
      PrependIndentIfNecessary();
      InnerBuilder.Append(value);
      return this;
   }

   /// <summary> Appends a boolean. Indentation is added automatically. </summary>
   public IndentedStringBuilder Append(bool value)
   {
      PrependIndentIfNecessary();
      InnerBuilder.Append(value); // Appends "True" or "False" by default
      return this;
   }

   /// <summary> Appends a double. Indentation is added automatically. </summary>
   public IndentedStringBuilder Append(double value)
   {
      PrependIndentIfNecessary();
      // Use invariant culture to ensure dot separator (not comma) for serialization
      InnerBuilder.Append(value.ToString(CultureInfo.InvariantCulture));
      return this;
   }

   /// <summary> Appends a long. Indentation is added automatically. </summary>
   public IndentedStringBuilder Append(long value)
   {
      PrependIndentIfNecessary();
      InnerBuilder.Append(value);
      return this;
   }

   /// <summary> Appends an object using ToString(). Indentation is added automatically. </summary>
   public IndentedStringBuilder Append(object? value)
   {
      if (value == null)
         return this;

      PrependIndentIfNecessary();
      InnerBuilder.Append(value);
      return this;
   }

   /// <summary> Appends a boolean as "yes" or "no". </summary>
   public IndentedStringBuilder AppendYesNo(bool value)
   {
      PrependIndentIfNecessary();
      InnerBuilder.Append(value ? "yes" : "no");
      return this;
   }

   public int Length => InnerBuilder.Length;

   #endregion
}