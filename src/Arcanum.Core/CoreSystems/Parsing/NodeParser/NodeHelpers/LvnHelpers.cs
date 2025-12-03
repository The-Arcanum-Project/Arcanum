using System.Diagnostics.CodeAnalysis;
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
   extension(LiteralValueNode lvn)
   {
      public bool TryParseLocationFromLvn(ref ParsingContext pc,
                                          out Location location)
      {
         using var scope = pc.PushScope();
         var lexeme = pc.SliceString(lvn);
         if (!Globals.Locations.TryGetValue(lexeme, out location!))
         {
            pc.SetContext(lvn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidLocationKey,
                                           lexeme);
            location = Location.Empty;
            return pc.Fail();
         }

         return true;
      }

      public bool GetLocation(ref ParsingContext pc,
                              [MaybeNullWhen(false)] out Location location)
      {
         using var scope = pc.PushScope();
         var lexeme = pc.SliceString(lvn);
         if (!Globals.Locations.TryGetValue(lexeme, out location))
         {
            pc.SetContext(lvn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidLocationKey,
                                           lexeme);
            location = Location.Empty;
            return pc.Fail();
         }

         return true;
      }

      /// <summary>
      /// Gets the enum value from the LiteralValueNode. <br/>
      /// Logs a warning if the value could not be parsed to the enum type.
      /// </summary>
      public bool GetEnum(ref ParsingContext pc,
                          Type enumType,
                          [MaybeNullWhen(false)] out object enumValue)
      {
         using var scope = pc.PushScope();
         var lexeme = pc.SliceString(lvn);
         if (!Enum.TryParse(enumType, lexeme, true, out enumValue))
         {
            pc.SetContext(lvn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.EnumParseError,
                                           lexeme,
                                           enumType.Name);
            enumValue = null;
            return pc.Fail();
         }

         return true;
      }

      /// <summary>
      /// Tries to parse a byte from the LiteralValueNode. <br/>
      /// Logs a warning if the value could not be parsed to a byte.
      /// </summary>
      public bool TryParseByte(ref ParsingContext pc,
                               out byte value,
                               bool complainOnError = true)
      {
         using var scope = pc.PushScope();
         var lexeme = pc.SliceString(lvn);
         if (!byte.TryParse(lexeme, out value) && complainOnError)
         {
            pc.SetContext(lvn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidByteValue,
                                           lexeme);
            value = 0;
            return pc.Fail();
         }

         return true;
      }

      public bool TryParseBool(ref ParsingContext pc,
                               out bool value,
                               bool complainOnError = true)
      {
         using var scope = pc.PushScope();
         var lexeme = pc.SliceString(lvn);

         switch (lexeme)
         {
            case "yes":
               value = true;
               return true;
            case "no":
               value = false;
               return true;
         }

         if (!complainOnError)
         {
            value = false;
            return false;
         }

         pc.SetContext(lvn);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.BoolParsingError,
                                        lexeme);
         value = false;
         return pc.Fail();
      }

      public bool TryParseFloat(ref ParsingContext pc,
                                out float value,
                                bool complainOnError = true)
      {
         using var scope = pc.PushScope();
         var lexeme = pc.SliceString(lvn);
         if (!float.TryParse(lexeme, out value) && complainOnError)
         {
            pc.SetContext(lvn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidFloatValue,
                                           lexeme);
            value = 0;
            return pc.Fail();
         }

         return true;
      }

      public bool TryParseInt(ref ParsingContext pc,
                              out int value,
                              bool complainOnError = true)
      {
         using var scope = pc.PushScope();
         var lexeme = pc.SliceString(lvn);
         if (!int.TryParse(lexeme, out value) && complainOnError)
         {
            pc.SetContext(lvn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidIntegerValue,
                                           lexeme);
            value = 0;
            return pc.Fail();
         }

         return true;
      }

      public bool TryParseJominiDate(ref ParsingContext pc,
                                     out JominiDate value)
      {
         using var scope = pc.PushScope();
         var lexeme = pc.SliceString(lvn);
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
               pc.SetContext(lvn);
               DiagnosticException.LogWarning(ref pc,
                                              ParsingError.Instance.PartialDateValue,
                                              lexeme);
               pc.Fail();
               value = new(year, month, day);
               return true;
            }

            if (parts.Length == 2 && int.TryParse(parts[0], out year) && int.TryParse(parts[1], out month))
            {
               day = 1;
               pc.SetContext(lvn);
               DiagnosticException.LogWarning(ref pc,
                                              ParsingError.Instance.PartialDateValue,
                                              lexeme);

               pc.Fail();
               value = new(year, month, day);
               return true;
            }

            pc.SetContext(lvn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidDateValue,
                                           lexeme);
            value = JominiDate.MinValue;
            return pc.Fail();
         }

         value = new(year, month, day);
         return true;
      }

      public bool TryParseCountry(ref ParsingContext pc,
                                  [MaybeNullWhen(false)] out Country country)
      {
         return LUtil.TryGetFromGlobalsAndLog(lvn.Value,
                                              ref pc,
                                              Globals.Countries,
                                              out country);
      }

      public bool TryParseCharacter(ref ParsingContext pc,
                                    [MaybeNullWhen(false)] out Character character)
      {
         using var scope = pc.PushScope();
         var lexeme = pc.SliceString(lvn);
         if (!Globals.Characters.TryGetValue(lexeme, out character))
         {
            if (lexeme == Character.RandomCharacter.UniqueId)
            {
               character = Character.RandomCharacter;
               return true;
            }

            pc.SetContext(lvn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidObjectKey,
                                           lexeme,
                                           typeof(Character));
            character = null;
            return pc.Fail();
         }

         return true;
      }

      public bool TryParseAge(ref ParsingContext pc,
                              [MaybeNullWhen(false)] out Age value)
      {
         using var scope = pc.PushScope();
         return LUtil.TryGetFromGlobalsAndLog(lvn.Value,
                                              ref pc,
                                              Globals.Ages,
                                              out value);
      }

      public bool TryParsePopType(ref ParsingContext pc,
                                  [MaybeNullWhen(false)] out PopType value)
      {
         using var scope = pc.PushScope();
         return LUtil.TryGetFromGlobalsAndLog(lvn.Value,
                                              ref pc,
                                              Globals.PopTypes,
                                              out value);
      }

      public bool TryParseCulture(ref ParsingContext pc,
                                  [MaybeNullWhen(false)] out Culture value)
      {
         using var scope = pc.PushScope();
         return LUtil.TryGetFromGlobalsAndLog(lvn.Value,
                                              ref pc,
                                              Globals.Cultures,
                                              out value);
      }

      public bool TryParseReligion(ref ParsingContext pc,
                                   [MaybeNullWhen(false)] out Religion value)
      {
         using var scope = pc.PushScope();
         return LUtil.TryGetFromGlobalsAndLog(lvn.Value,
                                              ref pc,
                                              Globals.Religions,
                                              out value);
      }

      public bool TryParseReligionGroup(ref ParsingContext pc,
                                        [MaybeNullWhen(false)] out ReligionGroup value)
      {
         using var scope = pc.PushScope();
         return LUtil.TryGetFromGlobalsAndLog(lvn.Value,
                                              ref pc,
                                              Globals.ReligionGroups,
                                              out value);
      }

      public bool TryParseDesignateHeirReason(ref ParsingContext pc,
                                              [MaybeNullWhen(false)] out DesignateHeirReason value)
      {
         using var scope = pc.PushScope();
         return LUtil.TryGetFromGlobalsAndLog(lvn.Value,
                                              ref pc,
                                              Globals.DesignateHeirReasons,
                                              out value);
      }

      public bool TryParseEstate(ref ParsingContext pc,
                                 [MaybeNullWhen(false)] out Estate value)
      {
         using var scope = pc.PushScope();
         return LUtil.TryGetFromGlobalsAndLog(lvn.Value,
                                              ref pc,
                                              Globals.Estates,
                                              out value);
      }

      public bool TryParseTrait(ref ParsingContext pc,
                                [MaybeNullWhen(false)] out Trait value)
      {
         using var scope = pc.PushScope();
         return LUtil.TryGetFromGlobalsAndLog(lvn.Value,
                                              ref pc,
                                              Globals.Traits,
                                              out value);
      }

      public bool TryParseParliamentType(ref ParsingContext pc,
                                         [MaybeNullWhen(false)] out ParliamentType value)
      {
         using var scope = pc.PushScope();
         return LUtil.TryGetFromGlobalsAndLog(lvn.Value,
                                              ref pc,
                                              Globals.ParliamentTypes,
                                              out value);
      }

      public bool TryParseArtistType(ref ParsingContext pc,
                                     [MaybeNullWhen(false)] out ArtistType value)
      {
         using var scope = pc.PushScope();
         return LUtil.TryGetFromGlobalsAndLog(lvn.Value,
                                              ref pc,
                                              Globals.ArtistTypes,
                                              out value);
      }
   }
}