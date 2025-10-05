using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Jomini.AiTags;
using Arcanum.Core.CoreSystems.Jomini.AudioTags;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.GameObjects.AbstractMechanics;
using Arcanum.Core.GameObjects.LocationCollections;
using ModValInstance = Arcanum.Core.CoreSystems.Jomini.Modifiers.ModValInstance;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class CnHelpers
{
   /// <summary>
   /// Checks if the ContentNode has an Equals separator and a LiteralValueNode as value.
   /// If so, it extracts the string content from the LiteralValueNode.
   /// Logs warnings if the checks fail.
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="value"></param>
   /// <returns></returns>
   public static bool TryGetStringContentNode(this ContentNode node,
                                              LocationContext ctx,
                                              string actionName,
                                              string source,
                                              out string value)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator, TokenType.Equals, ctx, actionName))
      {
         value = string.Empty;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, out var lvn))
      {
         value = string.Empty;
         return false;
      }

      value = lvn.Value.GetLexeme(source);
      return true;
   }

   /// <summary>
   /// Tries to get an enum value from a ContentNode. <br/>
   /// The ContentNode must have an Equals separator and a LiteralValueNode as value. <br/>
   /// The string content of the LiteralValueNode is parsed to the specified enum type. <br/>
   /// <c>Logs warnings</c> if any of the checks fail or if the parsing fails. 
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="enumType"></param>
   /// <param name="enumValue"></param>
   /// <returns></returns>
   public static bool TryGetEnumValue(this ContentNode node,
                                      LocationContext ctx,
                                      string actionName,
                                      string source,
                                      Type enumType,
                                      out Enum enumValue)
   {
      enumValue = (Enum)Enum.GetValues(enumType).GetValue(0)!;

      if (!node.TryGetStringContentNode(ctx, actionName, source, out var strValue))
         return false;

      if (!Enum.TryParse(enumType, strValue, true, out var parsedEnum))
      {
         ctx.SetPosition(node.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidEnumValue,
                                        actionName,
                                        strValue,
                                        enumType.Name,
                                        Enum.GetNames(enumType));
         return false;
      }

      enumValue = (Enum)parsedEnum;
      return true;
   }

   public static bool TryGetIdentifierNode(this ContentNode node,
                                           LocationContext ctx,
                                           string actionName,
                                           string source,
                                           [MaybeNullWhen(false)] out string identifier)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(TryGetIdentifierNode)}"))
      {
         identifier = string.Empty;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, $"{actionName}.{nameof(TryGetIdentifierNode)}", out var lvn))
      {
         identifier = string.Empty;
         return false;
      }

      identifier = lvn.Value.GetLexeme(source);
      return true;
   }

   public static bool TryGetIdentifierNode(this ContentNode node,
                                           LocationContext ctx,
                                           string actionName,
                                           string source,
                                           out string identifier,
                                           ref bool validationResult)
   {
      var result = node.TryGetIdentifierNode(ctx, actionName, source, out identifier!);
      validationResult &= result;
      return result;
   }

   public static bool TryGetIntegerContentNode(this ContentNode node,
                                               LocationContext ctx,
                                               string actionName,
                                               string source,
                                               out int value)
   {
      if (!SeparatorHelper.IsAnySupportedSeparator(node.Separator,
                                                   ctx,
                                                   actionName,
                                                   TokenType.Equals,
                                                   TokenType.GreaterOrEqual,
                                                   TokenType.LessOrEqual))
      {
         value = 0;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, out var lvn))
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
   /// Tries to get a Location from a ContentNode. <br/>
   /// The ContentNode must have an Equals separator and a LiteralValueNode as value. <br/>
   /// The string content of the LiteralValueNode is parsed to a Location. <br/>
   /// <c>Logs warnings</c> if any of the checks fail or if the parsing fails. <br/>
   /// The <paramref name="validationValue"/> is updated to false if any of the checks fail.
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="validationValue"></param>
   /// <param name="location"></param>
   /// <returns></returns>
   public static bool TryGetLocation(this ContentNode node,
                                     LocationContext ctx,
                                     string actionName,
                                     string source,
                                     ref bool validationValue,
                                     [MaybeNullWhen(false)] out Location location)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(TryGetLocation)}",
                                             ref validationValue))
      {
         location = Location.Empty;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validationValue, out var lvn))
      {
         location = Location.Empty;
         return false;
      }

      if (lvn.GetLocation(ctx, $"{actionName}.{nameof(TryGetLocation)}", source, ref validationValue, out location))
         return true;

      location = Location.Empty;
      return false;
   }

   public static bool TryGetBoolFromCn(this ContentNode node,
                                       LocationContext ctx,
                                       string actionName,
                                       string source,
                                       out bool boolValue,
                                       bool defaultValue = false)
   {
      SeparatorHelper.IsSeparatorOfType(node.Separator, TokenType.Equals, ctx, actionName);

      if (!node.TryGetStringContentNode(ctx, actionName, source, out var strValue))
      {
         boolValue = defaultValue;
         return false;
      }

      if (NumberParsing.TryParseBool(strValue, ctx, out boolValue, defaultValue))
         return true;

      boolValue = defaultValue;
      return false;
   }

   public static bool TryGetBool(this ContentNode node,
                                 LocationContext ctx,
                                 string actionName,
                                 string source,
                                 ref bool validationValue,
                                 out bool value,
                                 bool defaultValue = false)
   {
      SeparatorHelper.IsSeparatorOfType(node.Separator, TokenType.Equals, ctx, actionName, ref validationValue);

      if (node.TryGetStringContentNode(ctx, actionName, source, out var strValue))
         if (NumberParsing.TryParseBool(strValue, ctx, out var boolValue))
         {
            value = boolValue;
            return true;
         }

      value = defaultValue;
      validationValue = false;
      return false;
   }

   public static bool HasFunctionNode(this ContentNode node,
                                      LocationContext ctx,
                                      string source,
                                      string className,
                                      ref bool validationResult,
                                      [MaybeNullWhen(false)] out FunctionCallNode value)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator, TokenType.Equals, ctx, className, ref validationResult))
      {
         value = null;
         return false;
      }

      if (node.Value is not FunctionCallNode fcn)
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        $"{className}.StatementNode.IsFunctionNode",
                                        $"{node.GetType().Name}({node.KeyNode.GetLexeme(source)})",
                                        nameof(FunctionCallNode),
                                        node.KeyNode.GetLexeme(source));
         value = null!;
         validationResult = false;
         return false;
      }

      value = fcn;
      return true;
   }

   public static bool TryParseModValInstance(this ContentNode node,
                                             LocationContext ctx,
                                             string actionName,
                                             string source,
                                             ref bool validationResult,
                                             [MaybeNullWhen(false)] out ModValInstance modValInstance)
   {
      modValInstance = null;

      if (!LUtil.ValidateNodeSeparatorAndNumberValue(node,
                                                     ctx,
                                                     actionName,
                                                     source,
                                                     ref validationResult,
                                                     out var value))
         return false;

      return ModifierManager.TryCreateModifierInstance(ctx,
                                                       node.KeyNode,
                                                       source,
                                                       value,
                                                       ref validationResult,
                                                       out modValInstance);
   }

   public static bool TryParseAudioTagInstance(this ContentNode node,
                                               LocationContext ctx,
                                               string actionName,
                                               string source,
                                               ref bool validationResult,
                                               [MaybeNullWhen(false)] out AudioTag audioTagInstance)
   {
      audioTagInstance = null;

      if (!LUtil.ValidateNodeSeparatorAndNumberValue(node,
                                                     ctx,
                                                     actionName,
                                                     source,
                                                     ref validationResult,
                                                     out var value))
         return false;

      return AudioTagsManager.TryCreateModifierInstance(ctx,
                                                        node.KeyNode,
                                                        source,
                                                        value,
                                                        ref validationResult,
                                                        out audioTagInstance);
   }

   public static bool TryParseAiTagInstance(this ContentNode node,
                                            LocationContext ctx,
                                            string actionName,
                                            string source,
                                            ref bool validationResult,
                                            [MaybeNullWhen(false)] out AiTag aiTagInstance)
   {
      aiTagInstance = null;

      if (!LUtil.ValidateNodeSeparatorAndNumberValue(node,
                                                     ctx,
                                                     actionName,
                                                     source,
                                                     ref validationResult,
                                                     out var value))
         return false;

      return AiTagManager.TryCreateAiTagInstance(ctx,
                                                 node.KeyNode,
                                                 source,
                                                 value,
                                                 ref validationResult,
                                                 out aiTagInstance);
   }

   public static bool TryParseAge(this ContentNode node,
                                  LocationContext ctx,
                                  string actionName,
                                  string source,
                                  ref bool validationResult,
                                  [MaybeNullWhen(false)] out Age value)
   {
      value = null;

      if (!SeparatorHelper.IsSeparatorOfType(node.Separator, TokenType.Equals, ctx, actionName, ref validationResult))
      {
         validationResult = false;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validationResult, out var lvn))
      {
         validationResult = false;
         return false;
      }

      if (lvn.TryParseAge(ctx, actionName, source, ref validationResult, out value))
         return true;

      validationResult = false;
      return false;
   }
}