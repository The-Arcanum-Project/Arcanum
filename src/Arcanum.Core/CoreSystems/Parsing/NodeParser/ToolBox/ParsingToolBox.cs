using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Arcanum.Core.AgsRegistry;
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
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;
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
using Arcanum.Core.GameObjects.LocationCollections.SubObjects;
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
/// - string pc.BuildStackTrace(): The name of the action being performed, used for logging.
/// - string source: The original source code being parsed.
/// - out {Type} value: The parsed value, if successful.
/// </summary>
public static class ParsingToolBox
{
   private static readonly string[] Args = ["hsv", "rgb", "hsv360", "any color key"];

   public static bool ArcTryParse_String(ContentNode node,
                                         ref ParsingContext pc,
                                         [MaybeNullWhen(false)] out string value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
         pc.Fail();

      if (node.Value is not LiteralValueNode lvn)
      {
         pc.SetContext(node);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidNodeType,
                                        node.Value.GetType().Name,
                                        nameof(LiteralValueNode),
                                        pc.SliceString(node));
         value = null;
         return pc.Fail();
      }

      value = pc.SliceString(lvn);
      return true;
   }

   public static bool ArcTryParse_JominiColor(ContentNode node,
                                              ref ParsingContext pc,
                                              [MaybeNullWhen(false)] out JominiColor value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
         pc.Fail();

      if (node.Value is LiteralValueNode lvn)
      {
         value = new JominiColor.ColorKey(pc.SliceString(lvn));
         return true;
      }

      if (node.Value is FunctionCallNode fcn)
         if (!fcn.GetColorDefinition(ref pc,
                                     out value))
            return pc.Fail();
         else
            return true;

      pc.SetContext(node);
      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.InvalidColorMarkUp,
                                     pc.SliceString(node),
                                     Args);
      value = null;
      pc.Fail();
      return false;
   }

   /// <summary>
   /// Parses a ContentNode containing a literal integer value into an int.
   /// Validates that the ContentNode's separator is one of the supported types for integer values.
   /// Logs warnings to the provided LocationContext if any issues are encountered during parsing.
   /// </summary>
   public static bool ArcTryParse_Int32(ContentNode node,
                                        ref ParsingContext pc,
                                        out int value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsAnySupportedSeparator(node.Separator,
                                                   ref pc,
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
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidIntegerValue,
                                           pc.BuildStackTrace(),
                                           lexeme);
            value = 0;
            return pc.Fail();
         }
      }
      else if (node.Value is UnaryNode un)
      {
         if (un.Operator.Type != TokenType.Minus)
         {
            ctx.SetPosition(un.Operator);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidFloatOperator,
                                           pc.BuildStackTrace(),
                                           un.Operator.GetLexeme(source),
                                           nameof(TokenType.Minus));
            value = 0;
            return false;
         }

         if (un.Value is not LiteralValueNode lvn2)
         {
            ctx.SetPosition(un.Value);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidNodeType,
                                           pc.BuildStackTrace(),
                                           un.Value.GetType().Name,
                                           nameof(LiteralValueNode),
                                           node.KeyNode.GetLexeme(source));
            value = 0;
            return pc.Fail();
         }

         var lexeme = lvn2.Value.GetLexeme(source);
         if (!int.TryParse(lexeme, out value))
         {
            ctx.SetPosition(lvn2.Value);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidIntegerValue,
                                           pc.BuildStackTrace(),
                                           lexeme);
            value = 0;
            return false;
         }

         value = -value;
      }
      else
      {
         pc.SetContext(node);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidNodeType,
                                        pc.BuildStackTrace(),
                                        node.Value.GetType().Name,
                                        $"{nameof(LiteralValueNode)} or {nameof(UnaryNode)}",
                                        node.KeyNode.GetLexeme(source));
         value = 0;
         pc.Fail();
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
   /// <param name="pc.BuildStackTrace()"></param>
   /// <param name="source"></param>
   /// <param name="value"></param>
   /// <param name="validation"></param>
   /// <returns></returns>
   public static bool ArcTryParse_Boolean(ContentNode node,
                                          ref ParsingContext pc,
                                          out bool value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
         pc.Fail();

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         value = false;
         return false;
      }

      var lexeme = lvn.Value.GetLexeme(source);
      if (!NumberParsing.TryParseBool(lexeme, ctx, out value))
      {
         value = false;
         pc.Fail();
         return false;
      }

      return true;
   }

   public static bool ArcTryParse_Double(ContentNode node,
                                         ref ParsingContext pc,
                                         out double value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsAnySupportedSeparator(node.Separator,
                                                   ctx,
                                                   pc.BuildStackTrace(),
                                                   TokenType.Equals,
                                                   TokenType.GreaterOrEqual,
                                                   TokenType.LessOrEqual,
                                                   TokenType.Greater,
                                                   TokenType.Less))
      {
         value = 0;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         value = 0;
         return false;
      }

      var lexeme = lvn.Value.GetLexeme(source);
      if (!double.TryParse(lexeme.Replace(',', '.'),
                           NumberStyles.Float,
                           CultureInfo.InvariantCulture,
                           out value))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidDoubleValue,
                                        pc.BuildStackTrace(),
                                        lexeme);
         value = 0;
         return false;
      }

      return true;
   }

   public static bool ArcTryParse_Single(ContentNode node,
                                         ref ParsingContext pc,
                                         out float value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsAnySupportedSeparator(node.Separator,
                                                   ctx,
                                                   pc.BuildStackTrace(),
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
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidFloatValue,
                                           pc.BuildStackTrace(),
                                           lexeme);
            value = 0;
            return pc.Fail();
         }
      }
      else if (node.Value is UnaryNode un)
      {
         if (un.Operator.Type != TokenType.Minus)
         {
            ctx.SetPosition(un.Operator);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidFloatOperator,
                                           pc.BuildStackTrace(),
                                           un.Operator.GetLexeme(source),
                                           nameof(TokenType.Minus));
            value = 0;
            return false;
         }

         if (un.Value is not LiteralValueNode lvn2)
         {
            ctx.SetPosition(un.Value);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidNodeType,
                                           pc.BuildStackTrace(),
                                           un.Value.GetType().Name,
                                           nameof(LiteralValueNode),
                                           node.KeyNode.GetLexeme(source));
            value = 0;
            return pc.Fail();
         }

         var lexeme = lvn2.Value.GetLexeme(source);
         if (!NumberParsing.TryParseFloat(lexeme, ctx, out value))
         {
            ctx.SetPosition(lvn2.Value);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidFloatValue,
                                           pc.BuildStackTrace(),
                                           lexeme);
            value = 0;
            return false;
         }

         value = -value;
      }
      else
      {
         pc.SetContext(node);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidNodeType,
                                        pc.BuildStackTrace(),
                                        node.Value.GetType().Name,
                                        $"{nameof(LiteralValueNode)} or {nameof(UnaryNode)}",
                                        node.KeyNode.GetLexeme(source));
         value = 0;
         pc.Fail();
         return false;
      }

      return true;
   }

   public static bool ArcTryParse_Enum<TEnum>(ContentNode node,
                                              ref ParsingContext pc,
                                              out TEnum value) where TEnum : struct, Enum
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
         pc.Fail();

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         value = default;
         return false;
      }

      var lexeme = lvn.Value.GetLexeme(source);

      if (!EnumAgsRegistry.TryParse(lexeme, out value))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidEnumValue,
                                        pc.BuildStackTrace(),
                                        lexeme,
                                        typeof(TEnum).Name,
                                        Enum.GetNames(typeof(TEnum)));
         value = default;
         pc.Fail();
         return false;
      }

      return true;
   }

   public static bool ArcTryParse_FlagsEnum<TEnum>(ContentNode node,
                                                   ref ParsingContext pc,
                                                   out TEnum value) where TEnum : struct, Enum
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
         pc.Fail();

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         value = default;
         return false;
      }

      var lexeme = lvn.Value.GetLexeme(source);
      if (!Enum.TryParse(lexeme, true, out value))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidEnumValue,
                                        pc.BuildStackTrace(),
                                        lexeme,
                                        typeof(TEnum).Name,
                                        Enum.GetNames(typeof(TEnum)));
         value = default;
         pc.Fail();
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
   /// <param name="pc.BuildStackTrace()"></param>
   /// <param name="source"></param>
   /// <param name="value"></param>
   /// <param name="validation"></param>
   /// <returns></returns>
   public static bool ArcTryParse_Location(ContentNode node,
                                           ref ParsingContext pc,
                                           [MaybeNullWhen(false)] out Location value)
   {
      using var scope = pc.PushScope();
      return node.TryGetLocation(ref pc, out value);
   }

   /// <summary>
   /// Parses a ContentNode containing a location definition into a Location object.
   /// Utilizes the LocationContext to resolve the location.
   /// Logs warnings to the provided LocationContext if any issues are encountered during parsing.
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="pc.BuildStackTrace()"></param>
   /// <param name="source"></param>
   /// <param name="value"></param>
   /// <param name="validation"></param>
   /// <returns></returns>
   public static bool ArcTryParse_Location(KeyOnlyNode node,
                                           ref ParsingContext pc,
                                           [MaybeNullWhen(false)] out Location value)
   {
      using var scope = pc.PushScope();
      return node.TryGetLocation(ref pc, out value);
   }

   public static bool ArcTryParse_ModValInstance(ContentNode node,
                                                 ref ParsingContext pc,
                                                 [MaybeNullWhen(false)] out ModValInstance value)
   {
      using var scope = pc.PushScope();
      return node.TryParseModValInstance(ref pc, out value);
   }

   public static bool ArcTryParse_AudioTag(ContentNode node,
                                           ref ParsingContext pc,
                                           [MaybeNullWhen(false)] out AudioTag value)
   {
      using var scope = pc.PushScope();
      return node.TryParseAudioTagInstance(ref pc, out value);
   }

   public static bool ArcTryParse_ReligiousFaction(KeyOnlyNode node,
                                                   ref ParsingContext pc,
                                                   [MaybeNullWhen(false)] out ReligiousFaction value)
   {
      using var scope = pc.PushScope();
      return node.TryGetReligiousFaction(ref pc, out value);
   }

   public static bool ArcTryParse_CurrencyData(ContentNode node,
                                               ref ParsingContext pc,
                                               [MaybeNullWhen(false)] out CurrencyData value)
   {
      using var scope = pc.PushScope();
      value = null;
      return false;
   }

   public static bool ArcTryParse_Age(ContentNode node,
                                      ref ParsingContext pc,
                                      [MaybeNullWhen(false)] out Age value)
   {
      using var scope = pc.PushScope();
      return node.TryParseAge(ref pc, out value);
   }

   public static bool ArcTryParse_Province(KeyOnlyNode node,
                                           ref ParsingContext pc,
                                           [MaybeNullWhen(false)] out Province value)
   {
      using var scope = pc.PushScope();
      return node.TryGetProvince(ref pc, out value);
   }

   public static bool ArcTryParse_Area(KeyOnlyNode node,
                                       ref ParsingContext pc,
                                       [MaybeNullWhen(false)] out Area value)
   {
      using var scope = pc.PushScope();
      return node.TryGetArea(ref pc, out value);
   }

   public static bool ArcTryParse_Region(KeyOnlyNode node,
                                         ref ParsingContext pc,
                                         [MaybeNullWhen(false)] out Region value)
   {
      using var scope = pc.PushScope();
      return node.TryGetRegion(ref pc, out value);
   }

   public static bool ArcTryParse_AiTag(ContentNode node,
                                        ref ParsingContext pc,
                                        [MaybeNullWhen(false)] out AiTag value)
   {
      using var scope = pc.PushScope();
      return node.TryParseAiTagInstance(ref pc, out value);
   }

   public static bool ArcTryParse_String(KeyOnlyNode node,
                                         ref ParsingContext pc,
                                         [MaybeNullWhen(false)] out string value)
   {
      using var scope = pc.PushScope();
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
   /// <param name="pc.BuildStackTrace()"></param>
   /// <param name="source"></param>
   /// <param name="value"></param>
   /// <param name="validation"></param>
   /// <returns></returns>
   public static bool ArcTryParse_ReligiousSchool(ContentNode node,
                                                  ref ParsingContext pc,
                                                  [MaybeNullWhen(false)] out ReligiousSchool value)
   {
      using var scope = pc.PushScope();
      if (node.TryGetIdentifierNode(ctx, pc.BuildStackTrace(), source, out var rsName))
      {
         if (!Globals.ReligiousSchools.TryGetValue(rsName, out var rs))
         {
            pc.SetContext(node);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.UnknownObjectKey,
                                           pc.BuildStackTrace(),
                                           rsName,
                                           nameof(ReligiousSchool));
            value = null;
            return pc.Fail();
         }

         value = rs;
         return true;
      }

      value = null;
      pc.Fail();
      return false;
   }

   /// <summary>
   /// Parses a ContentNode containing a country rank identifier into a CountryRank object.
   /// Utilizes the global CountryRanks list to resolve the identifier.
   /// Logs warnings to the provided LocationContext if any issues are encountered during parsing.
   /// </summary>
   /// <param name="cn"></param>
   /// <param name="ctx"></param>
   /// <param name="pc.BuildStackTrace()"></param>
   /// <param name="source"></param>
   /// <param name="value"></param>
   /// <param name="validation"></param>
   /// <returns></returns>
   public static bool ArcTryParse_CountryRank(ContentNode cn,
                                              ref ParsingContext pc,
                                              [MaybeNullWhen(false)] out CountryRank value)
   {
      using var scope = pc.PushScope();
      if (!cn.TryGetIdentifierNode(ctx, pc.BuildStackTrace(), source, out var crlName))
      {
         value = null;
         pc.Fail();
         return false;
      }

      Globals.CountryRanks.TryGetValue(crlName, out value);
      if (value != null)
         return true;

      {
         ctx.SetPosition(cn.Value);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidCountryRankKey,
                                        pc.BuildStackTrace(),
                                        crlName,
                                        Globals.CountryRanks.Keys);
         value = null;
         pc.Fail();
         return false;
      }
   }

   public static bool ArcTryParse_JominiDate(ContentNode node,
                                             ref ParsingContext pc,
                                             [MaybeNullWhen(false)] out JominiDate value)
   {
      using var scope = pc.PushScope();
      value = JominiDate.Empty;
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
         return false;
      }

      if (!lvn.TryParseJominiDate(ref pc, out value))
         return false;

      return true;
   }

   public static bool ArcTryParse_EnactedLaw(ContentNode node,
                                             ref ParsingContext pc,
                                             [MaybeNullWhen(false)] out EnactedLaw value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
         value = null;
         return false;
      }

      value = new() { Key = node.KeyNode.GetLexeme(source), Value = lvn.Value.GetLexeme(source) };
      return true;
   }

   public static bool ArcTryParse_RegnalNumber(ContentNode node,
                                               ref ParsingContext pc,
                                               [MaybeNullWhen(false)] out RegnalNumber value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
         value = null;
         return false;
      }

      value = new() { Key = node.KeyNode.GetLexeme(source), Value = lvn.Value.GetLexeme(source) };
      return true;
   }

   public static bool ArcTryParse_Country(ContentNode node,
                                          ref ParsingContext pc,
                                          [MaybeNullWhen(false)] out Country value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
         value = null;
         return false;
      }

      return lvn.TryParseCountry(ref pc, out value);
   }

   public static bool ArcTryParse_Language(ContentNode node,
                                           ref ParsingContext pc,
                                           [MaybeNullWhen(false)] out Language value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
         pc.Fail();

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         value = null;
         return false;
      }

      var lexeme = lvn.Value.GetLexeme(source);
      if (!Globals.Languages.TryGetValue(lexeme, out value) && !Globals.Dialects.TryGetValue(lexeme, out value))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.UnknownObjectKey,
                                        pc.BuildStackTrace(),
                                        lexeme,
                                        nameof(Language));
         value = null;
         pc.Fail();
         return false;
      }

      return true;
   }

   public static bool ArcTryParse_CultureOpinionValue(ContentNode node,
                                                      ref ParsingContext pc,
                                                      [MaybeNullWhen(false)] out CultureOpinionValue value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!Globals.Cultures.TryGetValue(node.KeyNode.GetLexeme(source), out var culture))
      {
         ctx.SetPosition(node.KeyNode);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.UnknownObjectKey,
                                        pc.BuildStackTrace(),
                                        node.KeyNode.GetLexeme(source),
                                        nameof(Culture));
         value = null;
         pc.Fail();
         return false;
      }

      var lexeme = lvn.Value.GetLexeme(source);
      if (!EnumAgsRegistry.TryParse<Opinion>(lexeme, out var opinion))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidEnumValue,
                                        pc.BuildStackTrace(),
                                        lexeme,
                                        nameof(Opinion),
                                        Enum.GetNames(typeof(Opinion)));
         value = null;
         pc.Fail();
         return false;
      }

      value = new() { Key = culture, Value = opinion };
      return true;
   }

   public static bool ArcTryParse_ReligionOpinionValue(ContentNode node,
                                                       ref ParsingContext pc,
                                                       [MaybeNullWhen(false)] out ReligionOpinionValue value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!Globals.Religions.TryGetValue(node.KeyNode.GetLexeme(source), out var religion))
      {
         ctx.SetPosition(node.KeyNode);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.UnknownObjectKey,
                                        pc.BuildStackTrace(),
                                        node.KeyNode.GetLexeme(source),
                                        nameof(Religion));
         value = null;
         pc.Fail();
         return false;
      }

      var lexeme = lvn.Value.GetLexeme(source);
      if (!EnumAgsRegistry.TryParse<Opinion>(lexeme, out var opinion))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidEnumValue,
                                        pc.BuildStackTrace(),
                                        lexeme,
                                        nameof(Opinion),
                                        Enum.GetNames(typeof(Opinion)));
         value = null;
         pc.Fail();
         return false;
      }

      value = new() { Key = religion, Value = opinion };
      return true;
   }

   public static bool ArcTryParse_ReligiousSchoolOpinionValue(ContentNode node,
                                                              ref ParsingContext pc,
                                                              [MaybeNullWhen(false)] out ReligiousSchoolOpinionValue value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!Globals.ReligiousSchools.TryGetValue(node.KeyNode.GetLexeme(source), out var rs))
      {
         ctx.SetPosition(node.KeyNode);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.UnknownObjectKey,
                                        pc.BuildStackTrace(),
                                        node.KeyNode.GetLexeme(source),
                                        nameof(ReligiousSchoolOpinionValue));
         value = null;
         pc.Fail();
         return false;
      }

      var lexeme = lvn.Value.GetLexeme(source);
      if (!EnumAgsRegistry.TryParse<Opinion>(lexeme, out var opinion))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidEnumValue,
                                        pc.BuildStackTrace(),
                                        lexeme,
                                        nameof(Opinion),
                                        Enum.GetNames(typeof(Opinion)));
         value = null;
         pc.Fail();
         return false;
      }

      value = new() { Key = rs, Value = opinion };
      return true;
   }

   public static bool ArcTryParse_Character(ContentNode node,
                                            ref ParsingContext pc,
                                            [MaybeNullWhen(false)] out Character value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
         value = null;
         return false;
      }

      return lvn.TryParseCharacter(ref pc, out value);
   }

   public static bool ArcTryParse_CharacterNameDeclaration(ContentNode node,
                                                           ref ParsingContext pc,
                                                           [MaybeNullWhen(false)] out CharacterNameDeclaration value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
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
                                                           ref ParsingContext pc,
                                                           [MaybeNullWhen(false)] out CharacterNameDeclaration value)
   {
      using var scope = pc.PushScope();
      var key = node.KeyNode.GetLexeme(source);

      if (node.Children.Count != 1)
      {
         ctx.SetPosition(node.KeyNode);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidNodeType,
                                        pc.BuildStackTrace(),
                                        $"Expected exactly one child node in block for CharacterNameDeclaration with key '{key}', found {node.Children.Count}.",
                                        nameof(ContentNode),
                                        key);
         value = null;
         pc.Fail();
         return false;
      }

      if (node.Children[0] is not ContentNode cn)
      {
         ctx.SetPosition(node.Children[0].KeyNode);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidNodeType,
                                        pc.BuildStackTrace(),
                                        $"Expected child node in block for CharacterNameDeclaration with key '{key}' to be a {nameof(ContentNode)}, found {node.Children[0].GetType().Name}.",
                                        nameof(ContentNode),
                                        key);
         value = null;
         pc.Fail();
         return false;
      }

      if (!SeparatorHelper.IsSeparatorOfType(cn.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!cn.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
         value = null;
         return false;
      }

      value = new() { SavingKey = key, Name = lvn.Value.GetLexeme(source) };
      return true;
   }

   public static bool ArcTryParse_CharacterNameDeclaration(StatementNode node,
                                                           ref ParsingContext pc,
                                                           [MaybeNullWhen(false)] out CharacterNameDeclaration value)
   {
      using var scope = pc.PushScope();
      if (node is BlockNode bn)
         return ArcTryParse_CharacterNameDeclaration(bn, ctx, pc.BuildStackTrace(), source, out value, ref validation);
      if (node.IsContentNode(ref pc, out var cn))
         return ArcTryParse_CharacterNameDeclaration(cn, ctx, pc.BuildStackTrace(), source, out value, ref validation);

      value = null;
      pc.Fail();
      return false;
   }

   public static bool ArcTryParse_PopType(ContentNode node,
                                          ref ParsingContext pc,
                                          [MaybeNullWhen(false)] out PopType value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
         value = null;
         return false;
      }

      return lvn.TryParsePopType(ref pc, out value);
   }

   public static bool ArcTryParse_Culture(ContentNode node,
                                          ref ParsingContext pc,
                                          [MaybeNullWhen(false)] out Culture value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
         pc.Fail();

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         value = null;
         return false;
      }

      return lvn.TryParseCulture(ref pc, out value);
   }

   public static bool ArcTryParse_ReligionGroup(ContentNode node,
                                                ref ParsingContext pc,
                                                [MaybeNullWhen(false)] out ReligionGroup value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
         pc.Fail();

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         value = null;
         return false;
      }

      return lvn.TryParseReligionGroup(ref pc, out value);
   }

   public static bool ArcTryParse_Religion(ContentNode node,
                                           ref ParsingContext pc,
                                           [MaybeNullWhen(false)] out Religion value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
         pc.Fail();

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         value = null;
         return false;
      }

      return lvn.TryParseReligion(ref pc, out value);
   }

   public static bool ArcTryParse_SoundToll(ContentNode node,
                                            ref ParsingContext pc,
                                            [MaybeNullWhen(false)] out SoundToll value)
   {
      using var scope = pc.PushScope();
      value = null;
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc,
                                             ref validation))
         return false;

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
         return false;

      if (!lvn.TryParseLocationFromLvn(ref pc, out var loc))
         return false;

      if (!node.KeyNode.IsSimpleKeyNode(ctx, source, pc.BuildStackTrace(), out var skn))
         return false;

      if (!skn.KeyToken.TryGetLocationFromToken(ref pc, out var from))
         return false;

      value = new() { StraitLocationOne = from, StraitLocationTwo = loc };
      return true;
   }

   public static bool ArcTryParse_ReligiousFocus(KeyOnlyNode node,
                                                 ref ParsingContext pc,
                                                 [MaybeNullWhen(false)] out ReligiousFocus value)
   {
      using var scope = pc.PushScope();
      return node.TryGetReligiousFocus(ref pc, out value);
   }

   public static bool ArcTryParse_DesignateHeirReason(ContentNode node,
                                                      ref ParsingContext pc,
                                                      [MaybeNullWhen(false)] out DesignateHeirReason value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
         value = null;
         return false;
      }

      return lvn.TryParseDesignateHeirReason(ref pc, out value);
   }

   public static bool ArcTryParse_Estate(ContentNode node,
                                         ref ParsingContext pc,
                                         [MaybeNullWhen(false)] out Estate value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
         value = null;
         return false;
      }

      return lvn.TryParseEstate(ref pc, out value);
   }

   public static bool ArcTryParse_Trait(KeyOnlyNode node,
                                        ref ParsingContext pc,
                                        [MaybeNullWhen(false)] out Trait value)
   {
      using var scope = pc.PushScope();
      return node.TryParseTrait(ref pc, out value);
   }

   public static bool ArcTryParse_Trait(ContentNode node,
                                        ref ParsingContext pc,
                                        [MaybeNullWhen(false)] out Trait value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
         value = null;
         return false;
      }

      return lvn.TryParseTrait(ref pc, out value);
   }

   public static bool ArcTryParse_ParliamentType(ContentNode node,
                                                 ref ParsingContext pc,
                                                 [MaybeNullWhen(false)] out ParliamentType value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
         value = null;
         return false;
      }

      return lvn.TryParseParliamentType(ref pc, out value);
   }

   public static bool ArcTryParse_DemandData(ContentNode node,
                                             ref ParsingContext pc,
                                             [MaybeNullWhen(false)] out DemandData value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
         value = null;
         return false;
      }

      var key = node.KeyNode.GetLexeme(source);
      value = Eu5Activator.CreateEmbeddedInstance<DemandData>(null, node);

      switch (key)
      {
         case "all":
            if (!lvn.TryParseFloat(ref pc, out var all))
            {
               value = null;
               return false;
            }

            value.TargetAll = all;
            break;
         case "upper":
            if (!lvn.TryParseFloat(ref pc, out var upper))
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
               DiagnosticException.LogWarning(ref pc,
                                              ParsingError.Instance.UnknownObjectKey,
                                              pc.BuildStackTrace(),
                                              key,
                                              nameof(PopType));
               value = null;
               pc.Fail();
               return false;
            }

            value.PopType = popType;
            break;
      }

      return true;
   }

   public static bool ArcTryParse_EstateCountDefinition(ContentNode node,
                                                        ref ParsingContext pc,
                                                        [MaybeNullWhen(false)] out EstateCountDefinition value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
         value = null;
         return false;
      }

      var key = node.KeyNode.GetLexeme(source);
      if (!Globals.Estates.TryGetValue(key, out var estate))
      {
         ctx.SetPosition(node.KeyNode);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.UnknownKey,
                                        pc.BuildStackTrace(),
                                        key,
                                        nameof(Estate));
         value = null;
         pc.Fail();
         return false;
      }

      value = Eu5Activator.CreateEmbeddedInstance<EstateCountDefinition>(null, node);
      value.Estate = estate;

      if (!lvn.TryParseInt(ref pc, out var count))
         return false;

      value.Count = count;
      return true;
   }

   public static bool ArcTryParse_Topography(ContentNode node,
                                             ref ParsingContext pc,
                                             [MaybeNullWhen(false)] out Topography value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
         pc.Fail();

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         value = null;
         return false;
      }

      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           pc.BuildStackTrace(),
                                           Globals.Topography,
                                           out value);
   }

   public static bool ArcTryParse_Vegetation(ContentNode node,
                                             ref ParsingContext pc,
                                             [MaybeNullWhen(false)] out Vegetation value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
         pc.Fail();

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         value = null;
         return false;
      }

      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           pc.BuildStackTrace(),
                                           Globals.Vegetation,
                                           out value);
   }

   public static bool ArcTryParse_Climate(ContentNode node,
                                          ref ParsingContext pc,
                                          [MaybeNullWhen(false)] out Climate value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
         pc.Fail();

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         value = null;
         return false;
      }

      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           pc.BuildStackTrace(),
                                           Globals.Climates,
                                           out value);
   }

   public static bool ArcTryParse_RawMaterial(ContentNode node,
                                              ref ParsingContext pc,
                                              [MaybeNullWhen(false)] out RawMaterial value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
         pc.Fail();

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         value = null;
         return false;
      }

      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           pc.BuildStackTrace(),
                                           Globals.RawMaterials,
                                           out value);
   }

   public static bool ArcTryParse_StaticModifier(ContentNode node,
                                                 [MaybeNullWhen(false)] out StaticModifier value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
         pc.Fail();

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         value = null;
         return false;
      }

      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           pc.BuildStackTrace(),
                                           Globals.StaticModifiers,
                                           out value);
   }

   public static bool ArcTryParse_Dynasty(ContentNode node,
                                          ref ParsingContext pc,
                                          [MaybeNullWhen(false)] out Dynasty value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
         value = null;
         return false;
      }

      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           pc.BuildStackTrace(),
                                           Globals.Dynasties,
                                           out value);
   }

   public static bool ArcTryParse_CultureGroup(KeyOnlyNode node,
                                               ref ParsingContext pc,
                                               [MaybeNullWhen(false)] out CultureGroup value)
   {
      using var scope = pc.PushScope();
      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           node.KeyNode,
                                           source,
                                           pc.BuildStackTrace(),
                                           Globals.CultureGroups,
                                           out value);
   }

   public static bool ArcTryParse_ArtistType(KeyOnlyNode node,
                                             ref ParsingContext pc,
                                             [MaybeNullWhen(false)] out ArtistType value)
   {
      using var scope = pc.PushScope();
      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           node.KeyNode,
                                           source,
                                           pc.BuildStackTrace(),
                                           Globals.ArtistTypes,
                                           out value);
   }

   public static bool ArcTryParse_TownSetup(ContentNode node,
                                            ref ParsingContext pc,
                                            [MaybeNullWhen(false)] out TownSetup value)
   {
      using var scope = pc.PushScope();
      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           node,
                                           source,
                                           pc.BuildStackTrace(),
                                           Globals.TownSetups,
                                           out value);
   }

   public static bool ArcTryParse_LocationRank(ContentNode node,
                                               ref ParsingContext pc,
                                               [MaybeNullWhen(false)] out LocationRank value)
   {
      using var scope = pc.PushScope();
      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           node,
                                           source,
                                           pc.BuildStackTrace(),
                                           Globals.LocationRanks,
                                           out value);
   }

   public static bool ArcTryParse_InstitutionPresence(ContentNode node,
                                                      ref ParsingContext pc,
                                                      [MaybeNullWhen(false)] out InstitutionPresence value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc) ||
          !node.Value.IsLiteralValueNode(ref pc, out var lvn) ||
          !lvn.TryParseBool(ref pc, out var isPresent))
      {
         pc.Fail();
         value = null;
         return false;
      }

      var key = node.KeyNode.GetLexeme(source);
      if (!Globals.Institutions.TryGetValue(key, out var institution))
      {
         ctx.SetPosition(node.KeyNode);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.UnknownKey,
                                        pc.BuildStackTrace(),
                                        key,
                                        nameof(Institution));
         value = null;
         pc.Fail();
         return false;
      }

      value = new()
      {
         Institution = institution, IsPresent = isPresent,
      };
      return true;
   }

   public static bool ArcTryParse_ArtistType(ContentNode node,
                                             ref ParsingContext pc,
                                             [MaybeNullWhen(false)] out ArtistType value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc))
      {
         pc.Fail();
         value = null;
         return false;
      }

      if (!node.Value.IsLiteralValueNode(ref pc, out var lvn))
      {
         pc.Fail();
         value = null;
         return false;
      }

      return lvn.TryParseArtistType(ref pc, out value);
   }

   public static bool ArcTryParse_BuildingLevel(ContentNode node,
                                                ref ParsingContext pc,
                                                [MaybeNullWhen(false)] out BuildingLevel value)
   {
      using var scope = pc.PushScope();
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator,
                                             TokenType.Equals,
                                             ref pc) ||
          !node.Value.IsLiteralValueNode(ref pc, out var lvnValue) ||
          !lvnValue.TryParseInt(ref pc, out var level))
      {
         pc.Fail();
         value = null;
         return false;
      }

      var key = node.KeyNode.GetLexeme(source);
      if (!Globals.Buildings.TryGetValue(key, out var building))
      {
         ctx.SetPosition(node.KeyNode);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.UnknownKey,
                                        pc.BuildStackTrace(),
                                        key,
                                        nameof(Building));
         value = null;
         pc.Fail();
         return false;
      }

      value = new()
      {
         Building = building, Level = level,
      };
      return true;
   }
}