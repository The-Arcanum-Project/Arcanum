using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.AgsRegistry;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Jomini.AiTags;
using Arcanum.Core.CoreSystems.Jomini.AudioTags;
using Arcanum.Core.CoreSystems.Jomini.CurrencyDatas;
using Arcanum.Core.CoreSystems.Jomini.Date;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.GameObjects.AbstractMechanics;
using Arcanum.Core.GameObjects.Common;
using Arcanum.Core.GameObjects.CountryLevel;
using Arcanum.Core.GameObjects.Court;
using Arcanum.Core.GameObjects.Court.State;
using Arcanum.Core.GameObjects.Court.State.SubClasses;
using Arcanum.Core.GameObjects.Cultural;
using Arcanum.Core.GameObjects.Cultural.SubObjects;
using Arcanum.Core.GameObjects.Economy;
using Arcanum.Core.GameObjects.Economy.SubClasses;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.GameObjects.Pops;
using Arcanum.Core.GameObjects.Religious;
using Arcanum.Core.GameObjects.Religious.SubObjects;
using ParliamentType = Arcanum.Core.GameObjects.Court.ParliamentType;
using Region = Arcanum.Core.GameObjects.LocationCollections.Region;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;

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

      if (node.Value is LiteralValueNode lvn)
      {
         var lexeme = lvn.Value.GetLexeme(source);
         if (!int.TryParse(lexeme, out value))
         {
            ctx.SetPosition(lvn.Value);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.InvalidIntegerValue,
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
         if (!int.TryParse(lexeme, out value))
         {
            ctx.SetPosition(lvn2.Value);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.InvalidIntegerValue,
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

      if (!EnumAgsRegistry.TryParse(lexeme, out value))
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

   /// <summary>
   /// Parses a ContentNode containing a location definition into a Location object.
   /// Utilizes the LocationContext to resolve the location.
   /// Logs warnings to the provided LocationContext if any issues are encountered during parsing.
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="value"></param>
   /// <param name="validation"></param>
   /// <returns></returns>
   public static bool ArcTryParse_Location(ContentNode node,
                                           LocationContext ctx,
                                           string actionName,
                                           string source,
                                           [MaybeNullWhen(false)] out Location value,
                                           ref bool validation)
   {
      return node.TryGetLocation(ctx, actionName, source, ref validation, out value);
   }

   /// <summary>
   /// Parses a ContentNode containing a location definition into a Location object.
   /// Utilizes the LocationContext to resolve the location.
   /// Logs warnings to the provided LocationContext if any issues are encountered during parsing.
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="value"></param>
   /// <param name="validation"></param>
   /// <returns></returns>
   public static bool ArcTryParse_Location(KeyOnlyNode node,
                                           LocationContext ctx,
                                           string actionName,
                                           string source,
                                           [MaybeNullWhen(false)] out Location value,
                                           ref bool validation)
   {
      return node.TryGetLocation(ctx, source, actionName, ref validation, out value);
   }

   public static bool ArcTryParse_ModValInstance(ContentNode node,
                                                 LocationContext ctx,
                                                 string actionName,
                                                 string source,
                                                 [MaybeNullWhen(false)] out ModValInstance value,
                                                 ref bool validation)
   {
      return node.TryParseModValInstance(ctx, actionName, source, ref validation, out value);
   }

   public static bool ArcTryParse_AudioTag(ContentNode node,
                                           LocationContext ctx,
                                           string actionName,
                                           string source,
                                           [MaybeNullWhen(false)] out AudioTag value,
                                           ref bool validation)
   {
      return node.TryParseAudioTagInstance(ctx, actionName, source, ref validation, out value);
   }

   public static bool ArcTryParse_ReligiousFaction(KeyOnlyNode node,
                                                   LocationContext ctx,
                                                   string actionName,
                                                   string source,
                                                   [MaybeNullWhen(false)] out ReligiousFaction value,
                                                   ref bool validation)
   {
      return node.TryGetReligiousFaction(ctx, source, actionName, ref validation, out value);
   }

   public static bool ArcTryParse_CurrencyData(ContentNode node,
                                               LocationContext ctx,
                                               string actionName,
                                               string source,
                                               [MaybeNullWhen(false)] out CurrencyData value,
                                               ref bool validation)
   {
      value = null;
      return false;
   }

   public static bool ArcTryParse_Age(ContentNode node,
                                      LocationContext ctx,
                                      string actionName,
                                      string source,
                                      [MaybeNullWhen(false)] out Age value,
                                      ref bool validation)
   {
      return node.TryParseAge(ctx, source, actionName, ref validation, out value);
   }

   public static bool ArcTryParse_Province(KeyOnlyNode node,
                                           LocationContext ctx,
                                           string actionName,
                                           string source,
                                           [MaybeNullWhen(false)] out Province value,
                                           ref bool validation)
   {
      return node.TryGetProvince(ctx, source, actionName, ref validation, out value);
   }

   public static bool ArcTryParse_Area(KeyOnlyNode node,
                                       LocationContext ctx,
                                       string actionName,
                                       string source,
                                       [MaybeNullWhen(false)] out Area value,
                                       ref bool validation)
   {
      return node.TryGetArea(ctx, source, actionName, ref validation, out value);
   }

   public static bool ArcTryParse_Region(KeyOnlyNode node,
                                         LocationContext ctx,
                                         string actionName,
                                         string source,
                                         [MaybeNullWhen(false)] out Region value,
                                         ref bool validation)
   {
      return node.TryGetRegion(ctx, source, actionName, ref validation, out value);
   }

   public static bool ArcTryParse_AiTag(ContentNode node,
                                        LocationContext ctx,
                                        string actionName,
                                        string source,
                                        [MaybeNullWhen(false)] out AiTag value,
                                        ref bool validation)
   {
      return node.TryParseAiTagInstance(ctx, actionName, source, ref validation, out value);
   }

   public static bool ArcTryParse_String(KeyOnlyNode node,
                                         LocationContext ctx,
                                         string actionName,
                                         string source,
                                         [MaybeNullWhen(false)] out string value,
                                         ref bool validation)
   {
      value = node.KeyNode.GetLexeme(source);
      return true;
   }

   /// <summary>
   /// Parses a ContentNode containing a religious school identifier into a ReligiousSchool object.
   /// Utilizes the global ReligiousSchools dictionary to resolve the identifier.
   /// Logs warnings to the provided LocationContext if any issues are encountered during parsing.
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="value"></param>
   /// <param name="validation"></param>
   /// <returns></returns>
   public static bool ArcTryParse_ReligiousSchool(ContentNode node,
                                                  LocationContext ctx,
                                                  string actionName,
                                                  string source,
                                                  [MaybeNullWhen(false)] out ReligiousSchool value,
                                                  ref bool validation)
   {
      if (node.TryGetIdentifierNode(ctx, actionName, source, out var rsName))
      {
         if (!Globals.ReligiousSchools.TryGetValue(rsName, out var rs))
         {
            ctx.SetPosition(node.Value);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.UnknownObjectKey,
                                           actionName,
                                           rsName,
                                           nameof(ReligiousSchool));
            value = null;
            validation = false;
            return false;
         }

         value = rs;
         return true;
      }

      value = null;
      validation = false;
      return false;
   }

   /// <summary>
   /// Parses a ContentNode containing a country rank identifier into a CountryRank object.
   /// Utilizes the global CountryRanks list to resolve the identifier.
   /// Logs warnings to the provided LocationContext if any issues are encountered during parsing.
   /// </summary>
   /// <param name="cn"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="value"></param>
   /// <param name="validation"></param>
   /// <returns></returns>
   public static bool ArcTryParse_CountryRank(ContentNode cn,
                                              LocationContext ctx,
                                              string actionName,
                                              string source,
                                              [MaybeNullWhen(false)] out CountryRank value,
                                              ref bool validation)
   {
      if (!cn.TryGetIdentifierNode(ctx, actionName, source, out var crlName))
      {
         value = null;
         validation = false;
         return false;
      }

      Globals.CountryRanks.TryGetValue(crlName, out value);
      if (value != null)
         return true;

      {
         ctx.SetPosition(cn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidCountryRankKey,
                                        actionName,
                                        crlName,
                                        Globals.CountryRanks.Keys);
         value = null;
         validation = false;
         return false;
      }
   }

   public static bool ArcTryParse_JominiDate(ContentNode node,
                                             LocationContext ctx,
                                             string actionName,
                                             string source,
                                             [MaybeNullWhen(false)] out JominiDate value,
                                             ref bool validation)
   {
      value = JominiDate.Empty;
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_JominiDate)}"))
      {
         validation = false;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         return false;
      }

      if (!lvn.TryParseJominiDate(ctx, actionName, source, ref validation, out value))
         return false;

      return true;
   }

   public static bool ArcTryParse_EnactedLaw(ContentNode node,
                                             LocationContext ctx,
                                             string actionName,
                                             string source,
                                             [MaybeNullWhen(false)] out EnactedLaw value,
                                             ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_EnactedLaw)}"))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         value = null;
         return false;
      }

      value = new() { Key = node.KeyNode.GetLexeme(source), Value = lvn.Value.GetLexeme(source) };
      return true;
   }

   public static bool ArcTryParse_RegnalNumber(ContentNode node,
                                               LocationContext ctx,
                                               string actionName,
                                               string source,
                                               [MaybeNullWhen(false)] out RegnalNumber value,
                                               ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_RegnalNumber)}"))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         value = null;
         return false;
      }

      value = new() { Key = node.KeyNode.GetLexeme(source), Value = lvn.Value.GetLexeme(source) };
      return true;
   }

   public static bool ArcTryParse_Country(ContentNode node,
                                          LocationContext ctx,
                                          string actionName,
                                          string source,
                                          [MaybeNullWhen(false)] out Country value,
                                          ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_RegnalNumber)}"))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         value = null;
         return false;
      }

      return lvn.TryParseCountry(ctx, actionName, source, ref validation, out value);
   }

   public static bool ArcTryParse_Language(ContentNode node,
                                           LocationContext ctx,
                                           string actionName,
                                           string source,
                                           [MaybeNullWhen(false)] out Language value,
                                           ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_Language)}"))
         validation = false;

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         value = null;
         return false;
      }

      var lexeme = lvn.Value.GetLexeme(source);
      if (!Globals.Languages.TryGetValue(lexeme, out value) && !Globals.Dialects.TryGetValue(lexeme, out value))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.UnknownObjectKey,
                                        actionName,
                                        lexeme,
                                        nameof(Language));
         value = null;
         validation = false;
         return false;
      }

      return true;
   }

   public static bool ArcTryParse_CultureOpinionValue(ContentNode node,
                                                      LocationContext ctx,
                                                      string actionName,
                                                      string source,
                                                      [MaybeNullWhen(false)] out CultureOpinionValue value,
                                                      ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_CultureOpinionValue)}"))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!Globals.Cultures.TryGetValue(node.KeyNode.GetLexeme(source), out var culture))
      {
         ctx.SetPosition(node.KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.UnknownObjectKey,
                                        actionName,
                                        node.KeyNode.GetLexeme(source),
                                        nameof(Culture));
         value = null;
         validation = false;
         return false;
      }

      var lexeme = lvn.Value.GetLexeme(source);
      if (!EnumAgsRegistry.TryParse<Opinion>(lexeme, out var opinion))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidEnumValue,
                                        actionName,
                                        lexeme,
                                        nameof(Opinion),
                                        Enum.GetNames(typeof(Opinion)));
         value = null;
         validation = false;
         return false;
      }

      value = new() { Key = culture, Value = opinion };
      return true;
   }

   public static bool ArcTryParse_ReligionOpinionValue(ContentNode node,
                                                       LocationContext ctx,
                                                       string actionName,
                                                       string source,
                                                       [MaybeNullWhen(false)] out ReligionOpinionValue value,
                                                       ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_ReligionOpinionValue)}"))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!Globals.Religions.TryGetValue(node.KeyNode.GetLexeme(source), out var religion))
      {
         ctx.SetPosition(node.KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.UnknownObjectKey,
                                        actionName,
                                        node.KeyNode.GetLexeme(source),
                                        nameof(Religion));
         value = null;
         validation = false;
         return false;
      }

      var lexeme = lvn.Value.GetLexeme(source);
      if (!EnumAgsRegistry.TryParse<Opinion>(lexeme, out var opinion))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidEnumValue,
                                        actionName,
                                        lexeme,
                                        nameof(Opinion),
                                        Enum.GetNames(typeof(Opinion)));
         value = null;
         validation = false;
         return false;
      }

      value = new() { Key = religion, Value = opinion };
      return true;
   }

   public static bool ArcTryParse_ReligiousSchoolOpinionValue(ContentNode node,
                                                              LocationContext ctx,
                                                              string actionName,
                                                              string source,
                                                              [MaybeNullWhen(false)]
                                                              out ReligiousSchoolOpinionValue value,
                                                              ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_ReligiousSchoolOpinionValue)}"))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!Globals.ReligiousSchools.TryGetValue(node.KeyNode.GetLexeme(source), out var rs))
      {
         ctx.SetPosition(node.KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.UnknownObjectKey,
                                        actionName,
                                        node.KeyNode.GetLexeme(source),
                                        nameof(ReligiousSchoolOpinionValue));
         value = null;
         validation = false;
         return false;
      }

      var lexeme = lvn.Value.GetLexeme(source);
      if (!EnumAgsRegistry.TryParse<Opinion>(lexeme, out var opinion))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidEnumValue,
                                        actionName,
                                        lexeme,
                                        nameof(Opinion),
                                        Enum.GetNames(typeof(Opinion)));
         value = null;
         validation = false;
         return false;
      }

      value = new() { Key = rs, Value = opinion };
      return true;
   }

   public static bool ArcTryParse_Character(ContentNode node,
                                            LocationContext ctx,
                                            string actionName,
                                            string source,
                                            [MaybeNullWhen(false)] out Character value,
                                            ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_Character)}"))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         value = null;
         return false;
      }

      return lvn.TryParseCharacter(ctx, actionName, source, ref validation, out value);
   }

   public static bool ArcTryParse_CharacterNameDeclaration(ContentNode node,
                                                           LocationContext ctx,
                                                           string actionName,
                                                           string source,
                                                           [MaybeNullWhen(false)] out CharacterNameDeclaration value,
                                                           ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_CharacterNameDeclaration)}"))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         value = null;
         return false;
      }

      var name = lvn.Value.GetLexeme(source);

      value = new()
      {
         SavingKey = node.KeyNode.GetLexeme(source),
         Name = name,
         IsRandom = true,
      };
      return true;
   }

   public static bool ArcTryParse_CharacterNameDeclaration(BlockNode node,
                                                           LocationContext ctx,
                                                           string actionName,
                                                           string source,
                                                           [MaybeNullWhen(false)] out CharacterNameDeclaration value,
                                                           ref bool validation)
   {
      var key = node.KeyNode.GetLexeme(source);

      if (node.Children.Count != 1)
      {
         ctx.SetPosition(node.KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        actionName,
                                        $"Expected exactly one child node in block for CharacterNameDeclaration with key '{key}', found {node.Children.Count}.",
                                        nameof(ContentNode),
                                        key);
         value = null;
         validation = false;
         return false;
      }

      if (node.Children[0] is not ContentNode cn)
      {
         ctx.SetPosition(node.Children[0].KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        actionName,
                                        $"Expected child node in block for CharacterNameDeclaration with key '{key}' to be a {nameof(ContentNode)}, found {node.Children[0].GetType().Name}.",
                                        nameof(ContentNode),
                                        key);
         value = null;
         validation = false;
         return false;
      }

      if (!SeparatorHelper.IsSeparatorOfType(cn.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_CharacterNameDeclaration)}"))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!cn.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         value = null;
         return false;
      }

      value = new() { SavingKey = key, Name = lvn.Value.GetLexeme(source) };
      return true;
   }

   public static bool ArcTryParse_CharacterNameDeclaration(StatementNode node,
                                                           LocationContext ctx,
                                                           string actionName,
                                                           string source,
                                                           [MaybeNullWhen(false)] out CharacterNameDeclaration value,
                                                           ref bool validation)
   {
      if (node is BlockNode bn)
         return ArcTryParse_CharacterNameDeclaration(bn, ctx, actionName, source, out value, ref validation);
      if (node.IsContentNode(ctx, source, actionName, ref validation, out var cn))
         return ArcTryParse_CharacterNameDeclaration(cn, ctx, actionName, source, out value, ref validation);

      value = null;
      validation = false;
      return false;
   }

   public static bool ArcTryParse_PopType(ContentNode node,
                                          LocationContext ctx,
                                          string actionName,
                                          string source,
                                          [MaybeNullWhen(false)] out PopType value,
                                          ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_PopType)}"))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         value = null;
         return false;
      }

      return lvn.TryParsePopType(ctx, actionName, source, ref validation, out value);
   }

   public static bool ArcTryParse_Culture(ContentNode node,
                                          LocationContext ctx,
                                          string actionName,
                                          string source,
                                          [MaybeNullWhen(false)] out Culture value,
                                          ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_Culture)}"))
         validation = false;

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         value = null;
         return false;
      }

      return lvn.TryParseCulture(ctx, actionName, source, ref validation, out value);
   }

   public static bool ArcTryParse_ReligionGroup(ContentNode node,
                                                LocationContext ctx,
                                                string actionName,
                                                string source,
                                                [MaybeNullWhen(false)] out ReligionGroup value,
                                                ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_ReligionGroup)}"))
         validation = false;

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         value = null;
         return false;
      }

      return lvn.TryParseReligionGroup(ctx, actionName, source, ref validation, out value);
   }

   public static bool ArcTryParse_Religion(ContentNode node,
                                           LocationContext ctx,
                                           string actionName,
                                           string source,
                                           [MaybeNullWhen(false)] out Religion value,
                                           ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_Religion)}"))
         validation = false;

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         value = null;
         return false;
      }

      return lvn.TryParseReligion(ctx, actionName, source, ref validation, out value);
   }

   public static bool ArcTryParse_SoundToll(ContentNode node,
                                            LocationContext ctx,
                                            string actionName,
                                            string source,
                                            [MaybeNullWhen(false)] out SoundToll value,
                                            ref bool validation)
   {
      value = null;
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_SoundToll)}",
                                             ref validation))
         return false;

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
         return false;

      if (!lvn.TryParseLocationFromLvn(ctx, actionName, source, ref validation, out var loc))
         return false;

      if (!node.KeyNode.IsSimpleKeyNode(ctx, source, actionName, out var skn))
         return false;

      if (!skn.KeyToken.TryGetLocationFromToken(ctx, source, actionName, ref validation, out var from))
         return false;

      value = new() { StraitLocationOne = from, StraitLocationTwo = loc };
      return true;
   }

   public static bool ArcTryParse_ReligiousFocus(KeyOnlyNode node,
                                                 LocationContext ctx,
                                                 string actionName,
                                                 string source,
                                                 [MaybeNullWhen(false)] out ReligiousFocus value,
                                                 ref bool validation)
   {
      return node.TryGetReligiousFocus(ctx, source, actionName, ref validation, out value);
   }

   public static bool ArcTryParse_DesignateHeirReason(ContentNode node,
                                                      LocationContext ctx,
                                                      string actionName,
                                                      string source,
                                                      [MaybeNullWhen(false)] out DesignateHeirReason value,
                                                      ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_DesignateHeirReason)}"))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         value = null;
         return false;
      }

      return lvn.TryParseDesignateHeirReason(ctx, actionName, source, ref validation, out value);
   }

   public static bool ArcTryParse_Estate(ContentNode node,
                                         LocationContext ctx,
                                         string actionName,
                                         string source,
                                         [MaybeNullWhen(false)] out Estate value,
                                         ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_Estate)}"))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         value = null;
         return false;
      }

      return lvn.TryParseEstate(ctx, actionName, source, ref validation, out value);
   }

   public static bool ArcTryParse_Trait(KeyOnlyNode node,
                                        LocationContext ctx,
                                        string actionName,
                                        string source,
                                        [MaybeNullWhen(false)] out Trait value,
                                        ref bool validation)
   {
      return node.TryParseTrait(ctx, actionName, source, ref validation, out value);
   }

   public static bool ArcTryParse_Trait(ContentNode node,
                                        LocationContext ctx,
                                        string actionName,
                                        string source,
                                        [MaybeNullWhen(false)] out Trait value,
                                        ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_Trait)}"))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         value = null;
         return false;
      }

      return lvn.TryParseTrait(ctx, actionName, source, ref validation, out value);
   }

   public static bool ArcTryParse_ParliamentType(ContentNode node,
                                                 LocationContext ctx,
                                                 string actionName,
                                                 string source,
                                                 [MaybeNullWhen(false)] out ParliamentType value,
                                                 ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_ParliamentType)}"))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         value = null;
         return false;
      }

      return lvn.TryParseParliamentType(ctx, actionName, source, ref validation, out value);
   }

   public static bool ArcTryParse_DemandData(ContentNode node,
                                             LocationContext ctx,
                                             string actionName,
                                             string source,
                                             [MaybeNullWhen(false)] out DemandData value,
                                             ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_DemandData)}"))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         value = null;
         return false;
      }

      var key = node.KeyNode.GetLexeme(source);
      value = Eu5Activator.CreateEmbeddedInstance<DemandData>(null, node);

      switch (key)
      {
         case "all":
            if (!lvn.TryParseFloat(ctx, actionName, source, ref validation, out var all))
            {
               value = null;
               return false;
            }

            value.TargetAll = all;
            break;
         case "upper":
            if (!lvn.TryParseFloat(ctx, actionName, source, ref validation, out var upper))
            {
               value = null;
               return false;
            }

            value.TargetUpper = upper;
            break;
         default:
            if (!Globals.PopTypes.TryGetValue(key, out var popType))
            {
               ctx.SetPosition(node.KeyNode);
               DiagnosticException.LogWarning(ctx.GetInstance(),
                                              ParsingError.Instance.UnknownObjectKey,
                                              actionName,
                                              key,
                                              nameof(PopType));
               value = null;
               validation = false;
               return false;
            }

            value.PopType = popType;
            break;
      }

      return true;
   }

   public static bool ArcTryParse_EstateCountDefinition(ContentNode node,
                                                        LocationContext ctx,
                                                        string actionName,
                                                        string source,
                                                        [MaybeNullWhen(false)] out EstateCountDefinition value,
                                                        ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_EstateCountDefinition)}"))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         value = null;
         return false;
      }

      var key = node.KeyNode.GetLexeme(source);
      if (!Globals.Estates.TryGetValue(key, out var estate))
      {
         ctx.SetPosition(node.KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.UnknownKey,
                                        actionName,
                                        key,
                                        nameof(Estate));
         value = null;
         validation = false;
         return false;
      }

      value = Eu5Activator.CreateEmbeddedInstance<EstateCountDefinition>(null, node);
      value.Estate = estate;

      if (!lvn.TryParseInt(ctx, actionName, source, ref validation, out var count))
         return false;

      value.Count = count;
      return true;
   }

   public static bool ArcTryParse_Topography(ContentNode node,
                                             LocationContext ctx,
                                             string actionName,
                                             string source,
                                             [MaybeNullWhen(false)] out Topography value,
                                             ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_Topography)}"))
         validation = false;

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         value = null;
         return false;
      }

      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           actionName,
                                           ref validation,
                                           Globals.Topography,
                                           out value);
   }

   public static bool ArcTryParse_Vegetation(ContentNode node,
                                             LocationContext ctx,
                                             string actionName,
                                             string source,
                                             [MaybeNullWhen(false)] out Vegetation value,
                                             ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_Vegetation)}"))
         validation = false;

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         value = null;
         return false;
      }

      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           actionName,
                                           ref validation,
                                           Globals.Vegetation,
                                           out value);
   }

   public static bool ArcTryParse_Climate(ContentNode node,
                                          LocationContext ctx,
                                          string actionName,
                                          string source,
                                          [MaybeNullWhen(false)] out Climate value,
                                          ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_Climate)}"))
         validation = false;

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         value = null;
         return false;
      }

      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           actionName,
                                           ref validation,
                                           Globals.Climates,
                                           out value);
   }

   public static bool ArcTryParse_RawMaterial(ContentNode node,
                                              LocationContext ctx,
                                              string actionName,
                                              string source,
                                              [MaybeNullWhen(false)] out RawMaterial value,
                                              ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_RawMaterial)}"))
         validation = false;

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         value = null;
         return false;
      }

      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           actionName,
                                           ref validation,
                                           Globals.RawMaterials,
                                           out value);
   }

   public static bool ArcTryParse_StaticModifier(ContentNode node,
                                                 LocationContext ctx,
                                                 string actionName,
                                                 string source,
                                                 [MaybeNullWhen(false)] out StaticModifier value,
                                                 ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_StaticModifier)}"))
         validation = false;

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         value = null;
         return false;
      }

      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           actionName,
                                           ref validation,
                                           Globals.StaticModifiers,
                                           out value);
   }

   public static bool ArcTryParse_Dynasty(ContentNode node,
                                          LocationContext ctx,
                                          string actionName,
                                          string source,
                                          [MaybeNullWhen(false)] out Dynasty value,
                                          ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_Dynasty)}"))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         value = null;
         return false;
      }

      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           actionName,
                                           ref validation,
                                           Globals.Dynasties,
                                           out value);
   }

   public static bool ArcTryParse_CultureGroup(KeyOnlyNode node,
                                               LocationContext ctx,
                                               string actionName,
                                               string source,
                                               [MaybeNullWhen(false)] out CultureGroup value,
                                               ref bool validation)
   {
      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           node.KeyNode,
                                           source,
                                           actionName,
                                           ref validation,
                                           Globals.CultureGroups,
                                           out value);
   }

   public static bool ArcTryParse_ArtistType(KeyOnlyNode node,
                                             LocationContext ctx,
                                             string actionName,
                                             string source,
                                             [MaybeNullWhen(false)] out ArtistType value,
                                             ref bool validation)
   {
      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           node.KeyNode,
                                           source,
                                           actionName,
                                           ref validation,
                                           Globals.ArtistTypes,
                                           out value);
   }

   public static bool ArcTryParse_ArtistType(ContentNode node,
                                             LocationContext ctx,
                                             string actionName,
                                             string source,
                                             [MaybeNullWhen(false)] out ArtistType value,
                                             ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_ArtistType)}"))
      {
         validation = false;
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvn))
      {
         validation = false;
         value = null;
         return false;
      }

      return lvn.TryParseArtistType(ctx, actionName, source, ref validation, out value);
   }

   public static bool ArcTryParse_BuildingLevel(ContentNode node,
                                                LocationContext ctx,
                                                string actionName,
                                                string source,
                                                [MaybeNullWhen(false)] out BuildingLevel value,
                                                ref bool validation)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ctx,
                                             $"{actionName}.{nameof(ArcTryParse_BuildingLevel)}") ||
          !node.Value.IsLiteralValueNode(ctx, actionName, ref validation, out var lvnValue) ||
          !lvnValue.TryParseInt(ctx, actionName, source, ref validation, out var level))
      {
         validation = false;
         value = null;
         return false;
      }

      var key = node.KeyNode.GetLexeme(source);
      if (!Globals.Buildings.TryGetValue(key, out var building))
      {
         ctx.SetPosition(node.KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.UnknownKey,
                                        actionName,
                                        key,
                                        nameof(Building));
         value = null;
         validation = false;
         return false;
      }

      value = new()
      {
         Building = building, Level = level,
      };
      return true;
   }
}