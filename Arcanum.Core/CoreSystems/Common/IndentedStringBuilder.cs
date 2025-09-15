using Arcanum.Core.CoreSystems.SavingSystem.AGS;

namespace Arcanum.Core.CoreSystems.Common;

using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// A StringBuilder that supports indentation, making it easy to build well-formatted, hierarchical strings.
/// It supports automatic indentation management via IDisposable scopes.
/// </summary>
public class IndentedStringBuilder(string indentString = "\t")
{
   private readonly StringBuilder _builder = new();
   private int _indentLevel;
   private string _cachedIndent = string.Empty; // Performance: cache the full indent string
   private bool _isAtStartOfLine = true;

   // default 4 spaces

   /// <summary>
   /// Gets the current indentation level.
   /// </summary>
   public int IndentLevel => _indentLevel;

   /// <summary>
   /// Increases the indentation level by one.
   /// Consider using the IDisposable returned by Indent() for automatic management.
   /// </summary>
   public void IncreaseIndent()
   {
      _indentLevel++;
      UpdateCachedIndent();
   }

   /// <summary>
   /// Decreases the indentation level by one.
   /// Consider using the IDisposable returned by Indent() for automatic management.
   /// </summary>
   public void DecreaseIndent()
   {
      _indentLevel = Math.Max(0, _indentLevel - 1);
      UpdateCachedIndent();
   }

   /// <summary>
   /// Appends the specified text to the builder. Indentation is added automatically
   /// if this is the beginning of a new line.
   /// </summary>
   public IndentedStringBuilder Append(string text)
   {
      if (string.IsNullOrEmpty(text))
         return this;

      PrependIndentIfNecessary();
      _builder.Append(text);
      return this;
   }

   /// <summary>
   /// Appends the specified text, followed by a newline. Indentation is added
   /// automatically.
   /// </summary>
   public IndentedStringBuilder AppendLine(string text = "")
   {
      PrependIndentIfNecessary();
      _builder.AppendLine(text);
      _isAtStartOfLine = true;
      return this;
   }

   /// <summary>
   /// Appends a sequence of strings, each on a new indented line.
   /// </summary>
   public IndentedStringBuilder AppendLines(IEnumerable<string> lines)
   {
      foreach (var line in lines)
         AppendLine(line);
      return this;
   }

   /// <summary>
   /// Conditionally appends a line.
   /// </summary>
   public IndentedStringBuilder AppendLineIf(bool condition, string text)
   {
      if (condition)
         AppendLine(text);
      return this;
   }

   /// <summary>
   /// Creates an indentation scope. When the returned IDisposable is disposed,
   /// the indentation level is automatically decreased.
   /// This is the recommended way to manage indentation.
   /// </summary>
   /// <example>
   /// using (builder.Indent())
   /// {
   ///     builder.AppendLine("This is indented.");
   /// }
   /// builder.AppendLine("This is not.");
   /// </example>
   public IDisposable Indent() => new IndentScope(this);

   /// <summary>
   /// Appends an opening brace, creates an indentation scope, and arranges for
   /// a closing brace to be appended when the scope is disposed.
   /// </summary>
   /// <example>
   /// using (builder.Block())
   /// {
   ///     builder.AppendLine("Content inside braces.");
   /// }
   /// // Produces:
   /// // {
   /// //     Content inside braces.
   /// // }
   /// </example>
   public IDisposable Block(string openingBrace = "{", string closingBrace = "}")
   {
      AppendLine(openingBrace);
      return new BlockScope(this, closingBrace);
   }

   public IDisposable BlockWithName(string blockName,
                                    string separator,
                                    string openingBrace = "{",
                                    string closingBrace = "}")
   {
      AppendLine($"{blockName} {separator} {openingBrace}");
      return new BlockScope(this, closingBrace);
   }

   public IDisposable BlockWithName(IAgs ags)
   {
      return BlockWithName(ags.SavingKey,
                           SavingUtil.GetSeparator(ags.ClassMetadata.Separator),
                           SavingUtil.GetBrace(ags.ClassMetadata.OpeningToken),
                           SavingUtil.GetBrace(ags.ClassMetadata.ClosingToken));
   }

   /// <summary>
   /// Clears the content of the builder and resets the indentation level.
   /// </summary>
   public void Clear()
   {
      _builder.Clear();
      _indentLevel = 0;
      _isAtStartOfLine = true;
      UpdateCachedIndent();
   }

   public override string ToString() => _builder.ToString();

   /// <summary>
   /// Allows implicit conversion to a string.
   /// </summary>
   public static implicit operator string(IndentedStringBuilder builder) => builder.ToString();

   private void PrependIndentIfNecessary()
   {
      if (_isAtStartOfLine)
      {
         _builder.Append(_cachedIndent);
         _isAtStartOfLine = false;
      }
   }

   private void UpdateCachedIndent()
   {
      if (indentString.Length == 0)
      {
         _cachedIndent = string.Empty;
         return;
      }

      _cachedIndent = string.Concat(Enumerable.Repeat(indentString, _indentLevel));
   }

   // Private struct for managing indent level with `using`
   private readonly struct IndentScope : IDisposable
   {
      private readonly IndentedStringBuilder _builder;

      public IndentScope(IndentedStringBuilder builder)
      {
         _builder = builder;
         _builder.IncreaseIndent();
      }

      public void Dispose() => _builder.DecreaseIndent();
   }

   // Private struct for managing code blocks with `using`
   private readonly struct BlockScope : IDisposable
   {
      private readonly IndentedStringBuilder _builder;
      private readonly string _closingBrace;

      public BlockScope(IndentedStringBuilder builder, string closingBrace)
      {
         _builder = builder;
         _closingBrace = closingBrace;
         _builder.IncreaseIndent();
      }

      public void Dispose()
      {
         _builder.DecreaseIndent();
         _builder.AppendLine(_closingBrace);
      }
   }
}