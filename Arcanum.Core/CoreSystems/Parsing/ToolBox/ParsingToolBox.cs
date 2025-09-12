using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;

namespace Arcanum.Core.CoreSystems.Parsing.ToolBox;

/// <summary>
/// Any method in this class matching the signature of 'ArcTryParse{Type}' can be used in generated parsers to parse values of type {Type}.
/// The method should return true if parsing was successful, and false otherwise.
/// The method should also log any warnings or errors to the provided LocationContext.
/// The method should have the following parameters:
/// - ContentNode node: The content node to parse. / BlockNode node: The block node to parse. / KeyOnlyNode node: The key-only node to parse.
/// - LocationContext ctx: The context to log warnings and errors.
/// - string actionName: The name of the action being performed, used for logging.
/// - string source: The original source code being parsed.
/// - out {Type} value: The parsed value, if successful.
/// </summary>
public static class ParsingToolBox
{
   private static readonly string[] Args = ["hsv", "rgb", "hsv360", "any color key"];

   public static bool ArcTryParse_String(ContentNode node,
                                         LocationContext ctx,
                                         string actionName,
                                         string source,
                                         [MaybeNullWhen(false)] out string value,
                                         ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_String)}"))
         validation = false;

      if (node.Value is not LiteralValueNode lvn)
      {
         ctx.SetPosition(node.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        actionName,
                                        node.Value.GetType().Name,
                                        nameof(LiteralValueNode),
                                        node.KeyNode.GetLexeme(source));
         value = null;
         validation = false;
         return false;
      }

      value = lvn.Value.GetLexeme(source);
      return true;
   }

   public static bool ArcTryParse_JominiColor(ContentNode node,
                                              LocationContext ctx,
                                              string actionName,
                                              string source,
                                              [MaybeNullWhen(false)] out JominiColor value,
                                              ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_JominiColor)}"))
         validation = false;

      if (node.Value is LiteralValueNode lvn)
      {
         value = new JominiColor.ColorKey(lvn.Value.GetLexeme(source));
         return true;
      }

      if (node.Value is FunctionCallNode fcn)
         if (!fcn.GetColorDefinition(ctx,
                                     source,
                                     actionName,
                                     ref validation,
                                     out value))
         {
            validation = false;
            return false;
         }
         else
            return true;

      ctx.SetPosition(node.Value);
      DiagnosticException.LogWarning(ctx.GetInstance(),
                                     ParsingError.Instance.InvalidColorMarkUp,
                                     actionName,
                                     node.KeyNode.GetLexeme(source),
                                     Args);
      value = null;
      validation = false;
      return false;
   }

   /// <summary>
   /// Parses a BlockNode containing a list of KeyOnlyNodes into an ObservableRangeCollection of strings.
   /// Each KeyOnlyNode's key is added to the collection.
   /// Logs warnings to the provided LocationContext if any issues are encountered during parsing.
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="value"></param>
   /// <param name="validation"></param>
   /// <returns></returns>
   public static bool ArcTryParse_ObservableRangeCollectionString(
      BlockNode node,
      LocationContext ctx,
      string actionName,
      string source,
      [MaybeNullWhen(false)] out ObservableRangeCollection<string> value,
      ref bool validation)
   {
      if (ArcTryParse_StringList(node, ctx, actionName, source, out var stringList, ref validation))
      {
         value = [];
         value.AddRange(stringList);
         return true;
      }

      value = null;
      return false;
   }

   /// <summary>
   /// Parses a BlockNode containing a list of KeyOnlyNodes into a List of strings.
   /// Each KeyOnlyNode's key is added to the list.
   /// Logs warnings to the provided LocationContext if any issues are encountered during parsing.
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="value"></param>
   /// <param name="validation"></param>
   /// <returns></returns>
   public static bool ArcTryParse_StringList(
      BlockNode node,
      LocationContext ctx,
      string actionName,
      string source,
      [MaybeNullWhen(false)] out List<string> value,
      ref bool validation)
   {
      var results = new List<string>();

      foreach (var statement in node.Children)
         if (statement.IsKeyOnlyNode(ctx, source, actionName, ref validation, out var keyOnlyNode))
            results.Add(keyOnlyNode.KeyNode.GetLexeme(source));

      value = results;
      return validation;
   }

   /// <summary>
   /// Parses a ContentNode containing a literal integer value into an int.
   /// Validates that the ContentNode's separator is one of the supported types for integer values.
   /// Logs warnings to the provided LocationContext if any issues are encountered during parsing.
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="value"></param>
   /// <param name="validation"></param>
   /// <returns></returns>
   public static bool ArcTryParse_Int32(ContentNode node,
                                        LocationContext ctx,
                                        string actionName,
                                        string source,
                                        out int value,
                                        ref bool validation)
   {
      if (!SeparatorHelper.IsAnySupportedSeparator(node.Separator,
                                                   ctx,
                                                   actionName,
                                                   TokenType.Equals,
                                                   TokenType.GreaterOrEqual,
                                                   TokenType.LessOrEqual,
                                                   TokenType.Greater,
                                                   TokenType.Less))
      {
         value = 0;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         value = 0;
         return false;
      }

      var lexeme = lvn.Value.GetLexeme(source);
      if (!int.TryParse(lexeme, out value))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidIntegerValue,
                                        actionName,
                                        lexeme);
         value = 0;
         return false;
      }

      return true;
   }

   /// <summary>
   /// Parses a ContentNode containing a literal boolean value into a bool.
   /// Validates that the ContentNode's separator is an equals sign.
   /// Logs warnings to the provided LocationContext if any issues are encountered during parsing.
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="value"></param>
   /// <param name="validation"></param>
   /// <returns></returns>
   public static bool ArcTryParse_Boolean(ContentNode node,
                                          LocationContext ctx,
                                          string actionName,
                                          string source,
                                          out bool value,
                                          ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_Boolean)}"))
         validation = false;

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         value = false;
         return false;
      }

      var lexeme = lvn.Value.GetLexeme(source);
      if (!NumberParsing.TryParseBool(lexeme, ctx, out value))
      {
         value = false;
         validation = false;
         return false;
      }

      return true;
   }

   public static bool ArcTryParse_Double(ContentNode node,
                                         LocationContext ctx,
                                         string actionName,
                                         string source,
                                         out double value,
                                         ref bool validation)
   {
      if (!SeparatorHelper.IsAnySupportedSeparator(node.Separator,
                                                   ctx,
                                                   actionName,
                                                   TokenType.Equals,
                                                   TokenType.GreaterOrEqual,
                                                   TokenType.LessOrEqual,
                                                   TokenType.Greater,
                                                   TokenType.Less))
      {
         value = 0;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         value = 0;
         return false;
      }

      var lexeme = lvn.Value.GetLexeme(source);
      if (!double.TryParse(lexeme, out value))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidDoubleValue,
                                        actionName,
                                        lexeme);
         value = 0;
         return false;
      }

      return true;
   }

   public static bool ArcTryParse_Single(ContentNode node,
                                         LocationContext ctx,
                                         string actionName,
                                         string source,
                                         out float value,
                                         ref bool validation)
   {
      if (!SeparatorHelper.IsAnySupportedSeparator(node.Separator,
                                                   ctx,
                                                   actionName,
                                                   TokenType.Equals,
                                                   TokenType.GreaterOrEqual,
                                                   TokenType.LessOrEqual,
                                                   TokenType.Greater,
                                                   TokenType.Less))
      {
         value = 0;
         return false;
      }

      if (node.Value is LiteralValueNode lvn)
      {
         var lexeme = lvn.Value.GetLexeme(source);
         if (!NumberParsing.TryParseFloat(lexeme, ctx, out value))
         {
            ctx.SetPosition(lvn.Value);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.InvalidFloatValue,
                                           actionName,
                                           lexeme);
            value = 0;
            validation = false;
            return false;
         }
      }
      else if (node.Value is UnaryNode un)
      {
         if (un.Operator.Type != TokenType.Minus)
         {
            ctx.SetPosition(un.Operator);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.InvalidFloatOperator,
                                           actionName,
                                           un.Operator.GetLexeme(source),
                                           nameof(TokenType.Minus));
            value = 0;
            return false;
         }

         if (un.Value is not LiteralValueNode lvn2)
         {
            ctx.SetPosition(un.Value);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.InvalidNodeType,
                                           actionName,
                                           un.Value.GetType().Name,
                                           nameof(LiteralValueNode),
                                           node.KeyNode.GetLexeme(source));
            value = 0;
            validation = false;
            return false;
         }

         var lexeme = lvn2.Value.GetLexeme(source);
         if (!NumberParsing.TryParseFloat(lexeme, ctx, out value))
         {
            ctx.SetPosition(lvn2.Value);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.InvalidFloatValue,
                                           actionName,
                                           lexeme);
            value = 0;
            return false;
         }

         value = -value;
      }
      else
      {
         ctx.SetPosition(node.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        actionName,
                                        node.Value.GetType().Name,
                                        $"{nameof(LiteralValueNode)} or {nameof(UnaryNode)}",
                                        node.KeyNode.GetLexeme(source));
         value = 0;
         validation = false;
         return false;
      }

      return true;
   }

   public static bool ArcTryParse_Enum<TEnum>(ContentNode node,
                                              LocationContext ctx,
                                              string actionName,
                                              string source,
                                              out TEnum value,
                                              ref bool validation) where TEnum : struct, Enum
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_Enum)}"))
         validation = false;

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         value = default;
         return false;
      }

      var lexeme = lvn.Value.GetLexeme(source);
      if (!Enum.TryParse(lexeme, true, out value))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidEnumValue,
                                        actionName,
                                        lexeme,
                                        typeof(TEnum).Name,
                                        Enum.GetNames(typeof(TEnum)));
         value = default;
         validation = false;
         return false;
      }

      return true;
   }

   public static bool ArcTryParse_FlagsEnum<TEnum>(ContentNode node,
                                                   LocationContext ctx,
                                                   string actionName,
                                                   string source,
                                                   out TEnum value,
                                                   ref bool validation) where TEnum : struct, Enum
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_FlagsEnum)}"))
         validation = false;

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         value = default;
         return false;
      }

      var lexeme = lvn.Value.GetLexeme(source);
      if (!Enum.TryParse(lexeme, true, out value))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidEnumValue,
                                        actionName,
                                        lexeme,
                                        typeof(TEnum).Name,
                                        Enum.GetNames(typeof(TEnum)));
         value = default;
         validation = false;
         return false;
      }

      return true;
   }
}