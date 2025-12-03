using System.Diagnostics.CodeAnalysis;
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
   public static bool TryGetStringContentNode(this ContentNode node,
                                              ref ParsingContext pc,
                                              out string value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator, TokenType.Equals, ref pc))
      {
         value = string.Empty;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         value = string.Empty;
         return false;
      }

      value = pc.SliceString(lvn);
      return true;
   }

   public static bool TryGetIdentifierNode(this ContentNode node,
                                           ref ParsingContext pc,
                                           [MaybeNullWhen(false)] out string identifier)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         identifier = string.Empty;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         identifier = string.Empty;
         return false;
      }

      identifier = pc.SliceString(lvn);
      return true;
   }

   public static bool TryGetIdentifierNode(this ContentNode node,
                                           ref ParsingContext pc,
                                           out string identifier,
                                           ref bool validationResult)
   {
      using var scope = pc.PushScope();
      var result = node.TryGetIdentifierNode(ref pc, out identifier!);
      validationResult &= result;
      return result;
   }

   public static bool TryGetIntegerContentNode(this ContentNode node,
                                               ref ParsingContext pc,
                                               out int value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsAnySupportedSeparator(node.Separator,
                                                   ref pc,
                                                   TokenType.Equals,
                                                   TokenType.GreaterOrEqual,
                                                   TokenType.LessOrEqual))
      {
         value = 0;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         value = 0;
         return false;
      }

      var lexeme = pc.SliceString(lvn);
      if (!int.TryParse(lexeme, out value))
      {
         pc.SetContext(lvn);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidIntegerValue,
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
   /// <c>Logs warnings</c> if any of the checks fail or if the parsing fails. 
   /// </summary>
   public static bool TryGetLocation(this ContentNode node,
                                     ref ParsingContext pc,
                                     [MaybeNullWhen(false)] out Location location)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         location = Location.Empty;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         location = Location.Empty;
         return false;
      }

      if (lvn.GetLocation(ref pc, out location))
         return true;

      location = Location.Empty;
      return false;
   }

   public static bool TryGetBoolFromCn(this ContentNode node,
                                       ref ParsingContext pc,
                                       out bool boolValue,
                                       bool defaultValue = false)
   {
      using var scope = pc.PushScope();
      SeparatorHelper.IsSeparatorOfType(node.Separator, TokenType.Equals, ref pc);

      if (!node.TryGetStringContentNode(ref pc, out var strValue))
      {
         boolValue = defaultValue;
         return false;
      }

      if (NumberParsing.TryParseBool(strValue, ref pc, out boolValue, defaultValue))
         return true;

      boolValue = defaultValue;
      return false;
   }

   public static bool TryGetBool(this ContentNode node,
                                 ref ParsingContext pc,
                                 out bool value,
                                 bool defaultValue = false)
   {
      using var scope = pc.PushScope();
      SeparatorHelper.IsSeparatorOfType(node.Separator, TokenType.Equals, ref pc);

      if (node.TryGetStringContentNode(ref pc, out var strValue))
         if (NumberParsing.TryParseBool(strValue, ref pc, out var boolValue))
         {
            value = boolValue;
            return true;
         }

      value = defaultValue;
      return pc.Fail();
   }

   public static bool HasFunctionNode(this ContentNode node,
                                      ref ParsingContext pc,
                                      [MaybeNullWhen(false)] out FunctionCallNode value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator, TokenType.Equals, ref pc))
      {
         value = null;
         return false;
      }

      if (node.Value is not FunctionCallNode fcn)
      {
         pc.SetContext(node);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidNodeType,
                                        $"{node.GetType().Name}({pc.SliceString(node)})",
                                        nameof(FunctionCallNode),
                                        pc.SliceString(node));
         value = null!;
         return pc.Fail();
      }

      value = fcn;
      return true;
   }

   public static bool TryParseModValInstance(this ContentNode node,
                                             ref ParsingContext pc,
                                             [MaybeNullWhen(false)] out ModValInstance modValInstance)
   {
      using var scope = pc.PushScope();
      modValInstance = null;

      if (!LUtil.ValidateNodeSeparatorAndNumberValue(node,
                                                     ref pc,
                                                     out var value))
         return false;

      if (!node.KeyNode.IsSimpleKeyNode(ref pc, out var skn))
         return false;

      return ModifierManager.TryCreateModifierInstance(ref pc,
                                                       skn.KeyToken,
                                                       value,
                                                       out modValInstance);
   }

   public static bool TryParseAudioTagInstance(this ContentNode node,
                                               ref ParsingContext pc,
                                               [MaybeNullWhen(false)] out AudioTag audioTagInstance)
   {
      using var scope = pc.PushScope();
      audioTagInstance = null;

      if (!LUtil.ValidateNodeSeparatorAndNumberValue(node,
                                                     ref pc,
                                                     out var value))
         return false;

      if (!node.KeyNode.IsSimpleKeyNode(ref pc, out var skn))
         return false;

      return AudioTagsManager.TryCreateModifierInstance(ref pc,
                                                        skn.KeyToken,
                                                        value,
                                                        out audioTagInstance);
   }

   public static bool TryParseAiTagInstance(this ContentNode node,
                                            ref ParsingContext pc,
                                            [MaybeNullWhen(false)] out AiTag aiTagInstance)
   {
      using var scope = pc.PushScope();
      aiTagInstance = null;

      if (!LUtil.ValidateNodeSeparatorAndNumberValue(node,
                                                     ref pc,
                                                     out var value))
         return false;

      if (!node.KeyNode.IsSimpleKeyNode(ref pc, out var skn))
         return false;

      return AiTagManager.TryCreateAiTagInstance(ref pc,
                                                 skn.KeyToken,
                                                 value,
                                                 out aiTagInstance);
   }

   public static bool TryParseAge(this ContentNode node,
                                  ref ParsingContext pc,
                                  [MaybeNullWhen(false)] out Age value)
   {
      using var scope = pc.PushScope();
      value = null;

      if (!SeparatorHelper.IsSeparatorOfType(node.Separator, TokenType.Equals, ref pc) || !node.Value.IsLiteralValueNode(ref pc, out var lvn))
         return pc.Fail();

      if (lvn.TryParseAge(ref pc, out value))
         return true;

      return pc.Fail();
   }
}