using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.GameObjects.LocationCollections;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;

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

      value = lvn!.Value.GetLexeme(source);
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

   public static void SetEnumIfValid(this ContentNode node,
                                     LocationContext ctx,
                                     string actionName,
                                     string source,
                                     INexus target,
                                     Enum nxProp,
                                     Type enumType)
   {
      if (node.TryGetEnumValue(ctx, actionName, source, enumType, out var enumValue))
         Nx.ForceSet(enumValue, target, nxProp);
   }

   public static bool TryGetIdentifierNode(this ContentNode node,
                                           LocationContext ctx,
                                           string actionName,
                                           string source,
                                           out string identifier)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator, TokenType.Equals, ctx, actionName))
      {
         identifier = string.Empty;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, out var lvn))
      {
         identifier = string.Empty;
         return false;
      }

      identifier = lvn!.Value.GetLexeme(source);
      return true;
   }

   /// <summary>
   /// Sets an identifier without validation for the identifier. Only use temporary until full validation is possible. <br/>
   /// If the ContentNode is not valid, nothing happens. <br/>
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="target"></param>
   /// <param name="nxProp"></param>
   public static void SetIdentifierIfValid(this ContentNode node,
                                           LocationContext ctx,
                                           string actionName,
                                           string source,
                                           INexus target,
                                           Enum nxProp)
   {
      if (node.TryGetIdentifierNode(ctx, actionName, source, out var id) && !string.IsNullOrEmpty(id))
         Nx.ForceSet(id, target, nxProp);
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

      var lexeme = lvn!.Value.GetLexeme(source);
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

   public static void SetIntegerIfNotX(this ContentNode node,
                                       LocationContext ctx,
                                       string actionName,
                                       string source,
                                       INexus target,
                                       Enum nxProp,
                                       int notSetValue = 0,
                                       bool complainIfNotSet = false)
   {
      if (!node.TryGetIntegerContentNode(ctx, actionName, source, out var intValue))
         return;

      if (intValue != notSetValue)
         Nx.ForceSet(intValue, target, nxProp);
      else if (complainIfNotSet)
      {
         ctx.SetPosition(node.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.ForbiddenIntegerValue,
                                        actionName,
                                        intValue.ToString(),
                                        nxProp);
      }
   }

   public static bool TryParseLocationFromCn(this ContentNode node,
                                             LocationContext ctx,
                                             string actionName,
                                             string source,
                                             out Location location)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator, TokenType.Equals, ctx, actionName))
      {
         location = Location.Empty;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, out var lvn))
      {
         location = Location.Empty;
         return false;
      }

      return lvn!.TryParseLocationFromLvn(ctx, actionName, source, out location);
   }

   public static bool TryGetBoolFromCn(this ContentNode node,
                                       LocationContext ctx,
                                       string actionName,
                                       string source,
                                       out bool boolValue,
                                       bool defaultValue = false)
   {
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
   
   public static void SetBoolIfValid(this ContentNode node,
                                     LocationContext ctx,
                                     string actionName,
                                     string source,
                                     INexus target,
                                     Enum nxProp,
                                     bool defaultValue = false)
   {
      if (node.TryGetBoolFromCn(ctx, actionName, source, out var boolValue, defaultValue))
         Nx.ForceSet(boolValue, target, nxProp);
   }
}