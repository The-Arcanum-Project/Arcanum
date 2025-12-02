using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.Jomini.Effects;

public static class EffectManager
{
   public static Dictionary<string, EffectDefinition> DefinedTypes { get; } = [];

   public static bool TryCreateEffectInstance(ref ParsingContext pc,
                                              Token token,
                                              string value,
                                              [MaybeNullWhen(false)] out EffectInstance instance)
   {
      ModifierType type;
      var key = pc.SliceString(token);
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
            pc.SetContext(token);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.UndefinedEffectKey,
                                           key);
            return pc.Fail();
         }
      }
      else
      {
         type = existingDefinition.ModifierType;
      }

      return CreateInstance(ref pc, token, value, type, existingDefinition, out instance);
   }

   private static bool CreateInstance(ref ParsingContext pc,
                                      Token token,
                                      string value,
                                      ModifierType type,
                                      EffectDefinition existingDefinition,
                                      [MaybeNullWhen(false)] out EffectInstance instance)
   {
      if (!TryConvertValue(ref pc, token, value, type, out var convertedValue))
      {
         instance = null;
         return false;
      }

#pragma warning disable CS0618 // Type or member is obsolete
      instance = new(existingDefinition, convertedValue, type);
#pragma warning restore CS0618 // Type or member is obsolete
      return true;
   }

   internal static bool TryConvertValue(ref ParsingContext pc,
                                        Token token,
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

            return LogConversionError(ref pc, out convertedValue);
         }

         case ModifierType.Float:
         case ModifierType.Percentage:
         {
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatResult))
            {
               convertedValue = floatResult;
               return true;
            }

            return LogConversionError(ref pc, out convertedValue);
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

            return LogConversionError(ref pc, out convertedValue);
         }

         case ModifierType.ScriptedValue:
         {
            convertedValue = value;
            return true;
         }

         default:
            return LogConversionError(ref pc, out convertedValue);
      }

      bool LogConversionError(ref ParsingContext pc, out object? convertedValue)
      {
         if (defaultToScriptedValue)
         {
            convertedValue = value;
            return true;
         }

         pc.SetContext(token);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.UnableToConvertValueToModifierType,
                                        value,
                                        type);
         convertedValue = null;
         return pc.Fail();
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