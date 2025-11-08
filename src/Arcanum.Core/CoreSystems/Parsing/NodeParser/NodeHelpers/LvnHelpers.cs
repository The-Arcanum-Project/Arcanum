using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Jomini.Date;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.GameObjects.AbstractMechanics;
using Arcanum.Core.GameObjects.Court;
using Arcanum.Core.GameObjects.Court.State.SubClasses;
using Arcanum.Core.GameObjects.Cultural;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Pops;
using Culture = Arcanum.Core.GameObjects.Cultural.Culture;
using Estate = Arcanum.Core.GameObjects.Cultural.Estate;
using ParliamentType = Arcanum.Core.GameObjects.Court.ParliamentType;
using Religion = Arcanum.Core.GameObjects.Religious.Religion;
using ReligionGroup = Arcanum.Core.GameObjects.Religious.ReligionGroup;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class LvnHelpers
{
   public static bool TryParseLocationFromLvn(this LiteralValueNode lvn,
                                              LocationContext ctx,
                                              string actionName,
                                              string source,
                                              ref bool validation,
                                              out Location location)
   {
      var lexeme = lvn.Value.GetLexeme(source);
      if (!Globals.Locations.TryGetValue(lexeme, out location!))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidLocationKey,
                                        actionName,
                                        lexeme);
         location = Location.Empty;
         validation = false;
         return false;
      }

      return true;
   }

   public static bool GetLocation(this LiteralValueNode lvn,
                                  LocationContext ctx,
                                  string actionName,
                                  string source,
                                  ref bool validationResult,
                                  [MaybeNullWhen(false)] out Location location)
   {
      var lexeme = lvn.Value.GetLexeme(source);
      if (!Globals.Locations.TryGetValue(lexeme, out location))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidLocationKey,
                                        actionName,
                                        lexeme);
         location = Location.Empty;
         validationResult = false;
         return false;
      }

      return true;
   }

   /// <summary>
   /// Gets the enum value from the LiteralValueNode. <br/>
   /// Logs a warning if the value could not be parsed to the enum type.
   /// </summary>
   public static bool GetEnum(this LiteralValueNode lvn,
                              LocationContext ctx,
                              string actionName,
                              string source,
                              Type enumType,
                              ref bool validationResult,
                              [MaybeNullWhen(false)] out object enumValue)
   {
      var lexeme = lvn.Value.GetLexeme(source);
      if (!Enum.TryParse(enumType, lexeme, true, out enumValue))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.EnumParseError,
                                        actionName,
                                        lexeme,
                                        enumType.Name);
         enumValue = null;
         validationResult = false;
         return false;
      }

      return true;
   }

   /// <summary>
   /// Tries to parse a byte from the LiteralValueNode. <br/>
   /// Logs a warning if the value could not be parsed to a byte.
   /// </summary>
   public static bool TryParseByte(this LiteralValueNode lvn,
                                   LocationContext ctx,
                                   string actionName,
                                   string source,
                                   ref bool validationResult,
                                   out byte value,
                                   bool complainOnError = true)
   {
      var lexeme = lvn.Value.GetLexeme(source);
      if (!byte.TryParse(lexeme, out value) && complainOnError)
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidByteValue,
                                        actionName,
                                        lexeme);
         value = 0;
         validationResult = false;
         return false;
      }

      return true;
   }

   public static bool TryParseBool(this LiteralValueNode lvn,
                                   LocationContext ctx,
                                   string actionName,
                                   string source,
                                   ref bool validationResult,
                                   out bool value,
                                   bool complainOnError = true)
   {
      var lexeme = lvn.Value.GetLexeme(source);
      if (bool.TryParse(lexeme, out value) || !complainOnError)
         return true;

      ctx.SetPosition(lvn.Value);
      DiagnosticException.LogWarning(ctx.GetInstance(),
                                     ParsingError.Instance.BoolParsingError,
                                     actionName,
                                     lexeme);
      value = false;
      validationResult = false;
      return false;
   }

   public static bool TryParseFloat(this LiteralValueNode lvn,
                                    LocationContext ctx,
                                    string actionName,
                                    string source,
                                    ref bool validationResult,
                                    out float value,
                                    bool complainOnError = true)
   {
      var lexeme = lvn.Value.GetLexeme(source);
      if (!float.TryParse(lexeme, out value) && complainOnError)
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidFloatValue,
                                        actionName,
                                        lexeme);
         value = 0;
         validationResult = false;
         return false;
      }

      return true;
   }

   public static bool TryParseInt(this LiteralValueNode lvn,
                                  LocationContext ctx,
                                  string actionName,
                                  string source,
                                  ref bool validationResult,
                                  out int value,
                                  bool complainOnError = true)
   {
      var lexeme = lvn.Value.GetLexeme(source);
      if (!int.TryParse(lexeme, out value) && complainOnError)
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidIntegerValue,
                                        actionName,
                                        lexeme);
         value = 0;
         validationResult = false;
         return false;
      }

      return true;
   }

   public static bool TryParseJominiDate(this LiteralValueNode lvn,
                                         LocationContext ctx,
                                         string actionName,
                                         string source,
                                         ref bool validationResult,
                                         out JominiDate value)
   {
      var lexeme = lvn.Value.GetLexeme(source);
      var parts = lexeme.Split('.');
      if (parts.Length != 3 ||
          !int.TryParse(parts[0], out var year) ||
          !int.TryParse(parts[1], out var month) ||
          !int.TryParse(parts[2], out var day))
      {
         // if the year is present we interpret any missing month/day as 1
         if (parts.Length == 1 && int.TryParse(parts[0], out year))
         {
            month = 1;
            day = 1;
            ctx.SetPosition(lvn.Value);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.PartialDateValue,
                                           actionName,
                                           lexeme);
            validationResult = false;
            value = new(year, month, day);
            return true;
         }

         if (parts.Length == 2 && int.TryParse(parts[0], out year) && int.TryParse(parts[1], out month))
         {
            day = 1;
            ctx.SetPosition(lvn.Value);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.PartialDateValue,
                                           actionName,
                                           lexeme);
            validationResult = false;
            value = new(year, month, day);
            return true;
         }

         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidDateValue,
                                        actionName,
                                        lexeme);
         validationResult = false;

         value = JominiDate.MinValue;
         return false;
      }

      value = new(year, month, day);
      return true;
   }

   public static bool TryParseCountry(this LiteralValueNode lvn,
                                      LocationContext ctx,
                                      string actionName,
                                      string source,
                                      ref bool validationResult,
                                      [MaybeNullWhen(false)] out Country country)
   {
      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           actionName,
                                           ref validationResult,
                                           Globals.Countries,
                                           out country);
   }

   public static bool TryParseCharacter(this LiteralValueNode lvn,
                                        LocationContext ctx,
                                        string actionName,
                                        string source,
                                        ref bool validationResult,
                                        [MaybeNullWhen(false)] out Character character)
   {
      var lexeme = lvn.Value.GetLexeme(source);
      if (!Globals.Characters.TryGetValue(lexeme, out character))
      {
         if (lexeme == Character.RandomCharacter.UniqueId)
         {
            character = Character.RandomCharacter;
            return true;
         }

         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidObjectKey,
                                        actionName,
                                        lexeme,
                                        typeof(Character));
         character = null;
         validationResult = false;
         return false;
      }

      return true;
   }

   public static bool TryParseAge(this LiteralValueNode lvn,
                                  LocationContext ctx,
                                  string actionName,
                                  string source,
                                  ref bool validationResult,
                                  [MaybeNullWhen(false)] out Age value)
   {
      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           actionName,
                                           ref validationResult,
                                           Globals.Ages,
                                           out value);
   }

   public static bool TryParsePopType(this LiteralValueNode lvn,
                                      LocationContext ctx,
                                      string actionName,
                                      string source,
                                      ref bool validationResult,
                                      [MaybeNullWhen(false)] out PopType value)
   {
      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           actionName,
                                           ref validationResult,
                                           Globals.PopTypes,
                                           out value);
   }

   public static bool TryParseCulture(this LiteralValueNode lvn,
                                      LocationContext ctx,
                                      string actionName,
                                      string source,
                                      ref bool validationResult,
                                      [MaybeNullWhen(false)] out Culture value)
   {
      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           actionName,
                                           ref validationResult,
                                           Globals.Cultures,
                                           out value);
   }

   public static bool TryParseReligion(this LiteralValueNode lvn,
                                       LocationContext ctx,
                                       string actionName,
                                       string source,
                                       ref bool validationResult,
                                       [MaybeNullWhen(false)] out Religion value)
   {
      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           actionName,
                                           ref validationResult,
                                           Globals.Religions,
                                           out value);
   }

   public static bool TryParseReligionGroup(this LiteralValueNode lvn,
                                            LocationContext ctx,
                                            string actionName,
                                            string source,
                                            ref bool validationResult,
                                            [MaybeNullWhen(false)] out ReligionGroup value)
   {
      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           actionName,
                                           ref validationResult,
                                           Globals.ReligionGroups,
                                           out value);
   }

   public static bool TryParseDesignateHeirReason(this LiteralValueNode lvn,
                                                  LocationContext ctx,
                                                  string actionName,
                                                  string source,
                                                  ref bool validationResult,
                                                  [MaybeNullWhen(false)] out DesignateHeirReason value)
   {
      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           actionName,
                                           ref validationResult,
                                           Globals.DesignateHeirReasons,
                                           out value);
   }

   public static bool TryParseEstate(this LiteralValueNode lvn,
                                     LocationContext ctx,
                                     string actionName,
                                     string source,
                                     ref bool validationResult,
                                     [MaybeNullWhen(false)] out Estate value)
   {
      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           actionName,
                                           ref validationResult,
                                           Globals.Estates,
                                           out value);
   }

   public static bool TryParseTrait(this LiteralValueNode lvn,
                                    LocationContext ctx,
                                    string actionName,
                                    string source,
                                    ref bool validationResult,
                                    [MaybeNullWhen(false)] out Trait value)
   {
      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           actionName,
                                           ref validationResult,
                                           Globals.Traits,
                                           out value);
   }

   public static bool TryParseParliamentType(this LiteralValueNode lvn,
                                             LocationContext ctx,
                                             string actionName,
                                             string source,
                                             ref bool validationResult,
                                             [MaybeNullWhen(false)] out ParliamentType value)
   {
      return LUtil.TryGetFromGlobalsAndLog(ctx,
                                           lvn.Value,
                                           source,
                                           actionName,
                                           ref validationResult,
                                           Globals.ParliamentTypes,
                                           out value);
   }

    public static bool TryParseArtistType(this LiteralValueNode lvn,
                                 LocationContext ctx,
                                 string actionName,
                                 string source,
                                 ref bool validationResult,
                                 [MaybeNullWhen(false)] out ArtistType value)
    {
        return LUtil.TryGetFromGlobalsAndLog(ctx,
                                             lvn.Value,
                                             source,
                                             actionName,
                                             ref validationResult,
                                             Globals.ArtistTypes,
                                             out value);
    }
}