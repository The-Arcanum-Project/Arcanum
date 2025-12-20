using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Jomini.Effects;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Common.UI;
using Common.UI.MBox;
using ModifierDefinition = Arcanum.Core.GameObjects.InGame.Common.ModifierDefinition;

namespace Arcanum.Core.CoreSystems.Jomini.Modifiers;

public static class ModifierManager
{
   /// <summary>
   /// If the key exists in the global modifier definitions, the inferred type is determined and
   /// the value is validated against that type, and instance is created and returned. <br/>
   /// If any of these steps fail, a diagnostic warning is logged and false is returned.
   /// </summary>
   public static bool TryCreateModifierInstance(ref ParsingContext pc,
                                                Token nodeKeyNode,
                                                string value,
                                                [MaybeNullWhen(false)] out ModValInstance instance)
   {
      var key = pc.SliceString(nodeKeyNode);
      instance = null;
      if (!Globals.ModifierDefinitions.TryGetValue(key, out var definition))
      {
         pc.SetContext(nodeKeyNode);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.UndefinedModifierKey,
                                        key,
                                        value);
         return pc.Fail();
      }

      if (!InferModifierType(definition, out var inferredType) && inferredType is null)
      {
         pc.SetContext(nodeKeyNode);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.ImpossibleModifierTypeInferring,
                                        key,
                                        "Could not infer modifier type from definition.");
         return pc.Fail();
      }

      if (!EffectManager.TryConvertValue(ref pc,
                                         nodeKeyNode,
                                         value,
                                         inferredType!.Value,
                                         out var convertedValue))
         return false;

#pragma warning disable CS0618 // Type or member is obsolete
      instance = new(definition, convertedValue, inferredType.Value);
#pragma warning restore CS0618 // Type or member is obsolete

      return true;
   }

   public static bool TryConvertValueToType(string value,
                                            ModifierType type,
                                            [MaybeNullWhen(false)] out object convertedValue,
                                            bool defaultToScripted = true)
   {
      convertedValue = null;
      try
      {
         switch (type)
         {
            case ModifierType.Boolean:
               convertedValue = value.Equals("yes") || value.Equals("no");
               break;
            case ModifierType.ScriptedValue:
               convertedValue = Convert.ToString(value, CultureInfo.InvariantCulture);
               break;
            case ModifierType.Percentage:
               convertedValue = Convert.ToDouble(value, CultureInfo.InvariantCulture);
               break;
            case ModifierType.Float:
            case ModifierType.Integer:
               convertedValue = Convert.ToSingle(value, CultureInfo.InvariantCulture);
               break;
            default:
               convertedValue = null;
               break;
         }

         return convertedValue is not null;
      }
      catch
      {
         if (!defaultToScripted)
            return false;

         convertedValue = Convert.ToString(value, CultureInfo.InvariantCulture) ??
                          throw new InvalidOperationException("Identifier modifier value cannot be null");
         return true;
      }
   }

   public static bool ValidateModifierValue(object value, ModifierType type)
   {
      return type switch
      {
         ModifierType.Boolean => value is bool,
         ModifierType.ScriptedValue => value is string,
         ModifierType.Percentage => value is float or double or int,
         ModifierType.Float => value is float or double,
         ModifierType.Integer => value is int,
         _ => false,
      };
   }

   /// <summary>
   /// Infers the type of a modifier based on its definition. <br/>
   /// The inference is based on the properties of the definition, such as whether it is boolean,
   /// a percentage, the number of decimals, etc. <br/>
   /// If the definition is inconclusive or conflicting, a diagnostic warning is logged and an
   /// exception is thrown as this is considered a critical error.
   /// </summary>
   /// <param name="definition"></param>
   /// <param name="type"></param>
   /// <returns></returns>
   /// <exception cref="InvalidOperationException"></exception>
   public static bool InferModifierType(ModifierDefinition definition, out ModifierType? type)
   {
      if (IsBooleanModifier(definition))
      {
         type = ModifierType.Boolean;
         return true;
      }

      if (IsPercentageModifier(definition))
      {
         type = ModifierType.Percentage;
         return true;
      }

      if (IsFloatingModifier(definition))
      {
         type = ModifierType.Float;
         return true;
      }

      if (IsIdentifierModifier(definition))
      {
         type = ModifierType.ScriptedValue;
         return true;
      }

      type = null;
      return false;
   }

   /// <summary>
   /// Formats a modifier instance's value based on its type.
   /// </summary>
   /// <param name="instance"></param>
   /// <returns></returns>
   /// <exception cref="InvalidOperationException"></exception>
   /// <exception cref="ArgumentOutOfRangeException"></exception>
   public static string FormatModifierPattern(this IModifierPattern instance)
   {
      return instance.Type switch
      {
         ModifierType.Boolean => (bool)instance.Value ? "true" : "false",
         ModifierType.ScriptedValue => instance.Value.ToString() ??
                                       throw new InvalidOperationException("Identifier modifier value cannot be null"),
         ModifierType.Percentage =>
            $"{Convert.ToDouble(instance.Value, CultureInfo.InvariantCulture).ToString("0.##", CultureInfo.InvariantCulture)}%",
         ModifierType.Float =>
            $"{Convert.ToSingle(instance.Value, CultureInfo.InvariantCulture).ToString("0.##", CultureInfo.InvariantCulture)}",
         ModifierType.Integer => Convert.ToInt32(instance.Value).ToString(),
         _ => throw new
                 ArgumentOutOfRangeException($"Unknown modifier type {instance.Type} for modifier {instance.UniqueId}"),
      };
   }

   /// <summary>
   /// Formats a modifier instance's value based on its type.
   /// </summary>
   /// <param name="instance"></param>
   /// <returns></returns>
   /// <exception cref="InvalidOperationException"></exception>
   /// <exception cref="ArgumentOutOfRangeException"></exception>
   public static string FormatModifierPatternToCode(this IModifierPattern instance)
   {
      switch (instance.Type)
      {
         case ModifierType.Boolean:
            return (bool)instance.Value ? "yes" : "no";
         case ModifierType.ScriptedValue:
            return instance.Value.ToString() ??
                   throw new InvalidOperationException("Identifier modifier value cannot be null");
         case ModifierType.Percentage:
         case ModifierType.Float:
            if (instance.Value is string strValue)
               return strValue;

            return
               $"{Convert.ToDouble(instance.Value, CultureInfo.InvariantCulture).ToString("0.##", CultureInfo.InvariantCulture)}";
         case ModifierType.Integer:
            if (instance.Value is string strValue2)
               return strValue2;

            return Convert.ToInt32(instance.Value).ToString();
         default:
            throw new
               ArgumentOutOfRangeException($"Unknown modifier type {instance.Type} for modifier {instance.UniqueId}");
      }
   }

   #region Modifier Type Inference Helpers

   private static bool IsBooleanModifier(ModifierDefinition definition)
   {
      if (!definition.IsBoolean)
         return false;

      // Ensure no other conflicting properties are set
      if (definition is { IsPercentage: false, IsAlreadyPercent: false })
         return true;

      DiagnosticException.LogWarning(LocationContext.Empty,
                                     ParsingError.Instance.InconclusiveModifierTypeDefinition,
                                     $"{nameof(InferModifierType)}.{nameof(IsBooleanModifier)}",
                                     definition.UniqueId,
                                     "A boolean modifier cannot be a percentage or already percent.");
      return false;
   }

   private static bool IsFloatingModifier(ModifierDefinition definition)
   {
      if (definition is { IsPercentage: false, IsAlreadyPercent: false, IsBoolean: false })
         return true;

      DiagnosticException.LogWarning(LocationContext.Empty,
                                     ParsingError.Instance.InconclusiveModifierTypeDefinition,
                                     $"{nameof(InferModifierType)}.{nameof(IsFloatingModifier)}",
                                     definition.UniqueId,
                                     "A floating modifier cannot be a percentage, already_percent, or boolean.");
      return false;
   }

   private static bool IsPercentageModifier(ModifierDefinition definition)
   {
      if (definition is { IsPercentage: false, IsAlreadyPercent: false })
         return false;

      if (definition.IsAlreadyPercent || definition.IsPercentage)
         return true;

      DiagnosticException.LogWarning(LocationContext.Empty,
                                     ParsingError.Instance.InconclusiveModifierTypeDefinition,
                                     $"{nameof(InferModifierType)}.{nameof(IsPercentageModifier)}",
                                     definition.UniqueId,
                                     "A percentage modifier cannot be boolean.");
      return false;
   }

   private static bool IsIdentifierModifier(ModifierDefinition definition)
   {
      if (definition is { IsBoolean: false, IsPercentage: false, IsAlreadyPercent: false })
         return true;

      DiagnosticException.LogWarning(LocationContext.Empty,
                                     ParsingError.Instance.InconclusiveModifierTypeDefinition,
                                     $"{nameof(InferModifierType)}.{nameof(IsIdentifierModifier)}",
                                     definition.UniqueId,
                                     "An identifier modifier cannot be boolean, percentage, or already_percent, and must have 0 decimals.");
      return false;
   }

   #endregion

   public static bool IsModifierValueInStandardRange(object value, ModifierType type)
   {
      if (type == ModifierType.Boolean && value is bool)
         return true;

      switch (type)
      {
         case ModifierType.Integer:
            // For now we assume all integer values are valid
            break;
         case ModifierType.Float:
            // We consider any value bigger than 0.4 or smaller than -0.4 as non-standard
            if (value is float and (> 0.4f or < -0.4f))
               return WarnForNonStandardModifierValue(value, type, "-0.4 to 0.4");

            break;
         case ModifierType.Percentage:
            // We consider any value bigger than 40% or smaller than -40% as non-standard
            if (value is float and (> 40f or < -40f))
               return WarnForNonStandardModifierValue(value, type, "-40 to 40");

            break;
         case ModifierType.ScriptedValue:
            // For now we assume all scripted values are valid
            // Later we want to resolve the the value to a number and check its range for the inferred type if not string.
            break;
         default:
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
      }

      return true;
   }

   private static bool WarnForNonStandardModifierValue(object value, ModifierType type, string range)
   {
      var result =
         UIHandle.Instance.PopUpHandle
                 .ShowMBox($"The modifier value '{value}' for type '{type}' is outside the standard range of '{range}'. Do you want to proceed?",
                           "Warning",
                           MBoxButton.OKCancel,
                           MessageBoxImage.Warning);

      return result == MBoxResult.OK;
   }

   public static List<string> GetDefaultValuesForModifier(ModifierDefinition definition)
   {
      var type = InferModifierType(definition, out var inferredType) ? inferredType : null;
      if (type is null)
         return [];

      return type switch
      {
         ModifierType.Boolean => ["yes", "no"],
         ModifierType.Integer => ["0", "1", "-1", "2", "-2", "5", "-5"],
         ModifierType.Float => ["0.0", "0.1", "-0.1", "0.5", "-0.5", "1.0", "-1.0"],
         ModifierType.Percentage => ["0%", "10%", "-10%", "15%", "-15%", "20%", "-20%"],
         ModifierType.ScriptedValue => ["some_value", "another_value"],
         _ => [],
      };
   }
}