using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Jomini.Date;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class LvnHelpers
{
   public static bool TryParseLocationFromLvn(this LiteralValueNode lvn,
                                              LocationContext ctx,
                                              string actionName,
                                              string source,
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
   /// <param name="lvn"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="enumType"></param>
   /// <param name="validationResult"></param>
   /// <param name="enumValue"></param>
   /// <returns></returns>
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
   /// Sets the enum property on the target NUI if the value could be parsed from the LiteralValueNode. <br/>
   /// Logs a warning if the value could not be parsed to the enum type.
   /// </summary>
   /// <param name="lvn"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="validationResult"></param>
   /// <param name="target"></param>
   /// <param name="nxProp"></param>
   public static void SetEnumIfValid(this LiteralValueNode lvn,
                                     LocationContext ctx,
                                     string actionName,
                                     string source,
                                     ref bool validationResult,
                                     INUI target,
                                     Enum nxProp)
   {
      if (lvn.GetEnum(ctx, actionName, source, nxProp.GetType(), ref validationResult, out var enumObj))
         Nx.ForceSet(enumObj, target, nxProp);
   }

   /// <summary>
   /// Tries to parse a byte from the LiteralValueNode. <br/>
   /// Logs a warning if the value could not be parsed to a byte.
   /// </summary>
   /// <param name="lvn"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="validationResult"></param>
   /// <param name="value"></param>
   /// <param name="complainOnError"></param>
   /// <returns></returns>
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
}