using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.Jomini.Effects;

public static class EffectManager
{
   public static Dictionary<string, EffectDefinition> DefinedTypes { get; } = [];

   public static bool TryCreateEffectInstance(LocationContext ctx,
                                              Token token,
                                              string source,
                                              string value,
                                              ref bool validationResult,
                                              [MaybeNullWhen(false)] out EffectInstance instance)
   {
      ModifierType type;
      var key = token.GetLexeme(source);
      if (!DefinedTypes.TryGetValue(key, out var existingDefinition))
      {
         type = InferEffectTypeFromValue(value);
         if (EffectRegistry.Effects.TryGetValue(key, out existingDefinition))
         {
            existingDefinition.ModifierType = type;
            DefinedTypes[key] = existingDefinition;
         }
         else
         {
            instance = null;
            ctx.SetPosition(token);
            DiagnosticException.LogWarning(ctx,
                                           ParsingError.Instance.UndefinedEffectKey,
                                           "Creating EffectInstance",
                                           key);
            validationResult = false;
            return false;
         }
      }
      else
      {
         type = existingDefinition.ModifierType;
      }

      return CreateInstance(ctx, token, value, ref validationResult, type, existingDefinition, out instance);
   }

   private static bool CreateInstance(LocationContext ctx,
                                      Token token,
                                      string value,
                                      ref bool validationResult,
                                      ModifierType type,
                                      EffectDefinition existingDefinition,
                                      [MaybeNullWhen(false)] out EffectInstance instance)
   {
      if (!TryConvertValue(ctx, token, ref validationResult, value, type, out var convertedValue))
      {
         instance = null;
         return false;
      }

#pragma warning disable CS0618 // Type or member is obsolete
      instance = new(existingDefinition, convertedValue, type);
#pragma warning restore CS0618 // Type or member is obsolete
      return true;
   }

   internal static bool TryConvertValue(LocationContext ctx,
                                        Token token,
                                        ref bool validationResult,
                                        string value,
                                        ModifierType type,
                                        [MaybeNullWhen(false)] out object convertedValue,
                                        bool defaultToScriptedValue = true)
   {
      switch (type)
      {
         case ModifierType.Integer:
         {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intResult))
            {
               convertedValue = intResult;
               return true;
            }

            return LogConversionError(out validationResult, out convertedValue);
         }

         case ModifierType.Float:
         case ModifierType.Percentage:
         {
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatResult))
            {
               convertedValue = floatResult;
               return true;
            }

            return LogConversionError(out validationResult, out convertedValue);
         }

         case ModifierType.Boolean:
         {
            if (value.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
               convertedValue = true;
               return true;
            }

            if (value.Equals("no", StringComparison.OrdinalIgnoreCase))
            {
               convertedValue = false;
               return true;
            }

            return LogConversionError(out validationResult, out convertedValue);
         }

         case ModifierType.ScriptedValue:
         {
            convertedValue = value;
            return true;
         }

         default:
            return LogConversionError(out validationResult, out convertedValue);
      }

      bool LogConversionError(out bool validationResult, out object? convertedValue)
      {
         if (defaultToScriptedValue)
         {
            convertedValue = value;
            validationResult = true;
            return true;
         }

         ctx.SetPosition(token);
         DiagnosticException.LogWarning(ctx,
                                        ParsingError.Instance.UnableToConvertValueToModifierType,
                                        "Converting EffectInstance value",
                                        value,
                                        type);
         validationResult = false;
         convertedValue = null;
         return false;
      }
   }

   private static ModifierType InferEffectTypeFromValue(object value)
   {
      if (value is int)
         return ModifierType.Integer;
      if (value is float or double or decimal)
         return ModifierType.Float;
      if (value.Equals("yes") || value.Equals("no"))
         return ModifierType.Boolean;

      return ModifierType.ScriptedValue;
   }
}