using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.GameObjects.Common;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.ModifierSystem;

public static class ModifierManager
{
   /// <summary>
   /// If the key exists in the global modifier definitions, the inferred type is determined and
   /// the value is validated against that type, and instance is created and returned. <br/>
   /// If any of these steps fail, a diagnostic warning is logged and false is returned.
   /// </summary>
   /// <param name="key"></param>
   /// <param name="value"></param>
   /// <param name="instance"></param>
   /// <returns></returns>
   public static bool TryCreateModifierInstance(string key,
                                                object value,
                                                [MaybeNullWhen(false)] out ModValInstance instance)
   {
      instance = null;
      if (!Globals.ModifierDefinitions.TryGetValue(key, out var definition))
      {
         DiagnosticException.LogWarning(LocationContext.Empty,
                                        ParsingError.Instance.UndefinedModifierKey,
                                        nameof(ModifierManager),
                                        key);
         return false;
      }

      if (!InferModifierType(definition, out var inferredType) && inferredType is not null)
      {
         DiagnosticException.LogWarning(LocationContext.Empty,
                                        ParsingError.Instance.ImpossibleModifierTypeInferring,
                                        nameof(ModifierManager),
                                        key,
                                        "Could not infer modifier type from definition.");
         return false;
      }

      if (!ValidateModifierValue(value, inferredType!.Value))
      {
         DiagnosticException.LogWarning(LocationContext.Empty,
                                        ParsingError.Instance.InvalidModifierValueType,
                                        nameof(ModifierManager),
                                        key,
                                        value.GetType().Name);
         return false;
      }

#pragma warning disable CS0618 // Type or member is obsolete
      instance = new(definition, value, inferredType.Value);
#pragma warning restore CS0618 // Type or member is obsolete

      return true;
   }

   public static bool ValidateModifierValue(object value, ModifierType type)
   {
      return type switch
      {
         ModifierType.Boolean => value is bool,
         ModifierType.ScriptedValue => value is string,
         ModifierType.Percentage => value is float or double or int,
         ModifierType.Floating => value is float or double,
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

      if (IsIntegerModifier(definition))
      {
         type = ModifierType.Integer;
         return true;
      }

      if (IsPercentageModifier(definition))
      {
         type = ModifierType.Percentage;
         return true;
      }

      if (IsFloatingModifier(definition))
      {
         type = ModifierType.Floating;
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
   public static string FormatModifierInstance(this ModValInstance instance)
   {
      return instance.Type switch
      {
         ModifierType.Boolean => (bool)instance.Value ? "true" : "false",
         ModifierType.ScriptedValue => instance.Value.ToString() ??
                                       throw new InvalidOperationException("Identifier modifier value cannot be null"),
         ModifierType.Percentage =>
            $"{Convert.ToDouble(instance.Value, CultureInfo.InvariantCulture).ToString("0.##", CultureInfo.InvariantCulture)}%",
         ModifierType.Floating =>
            $"{Convert.ToSingle(instance.Value, CultureInfo.InvariantCulture).ToString("0.##", CultureInfo.InvariantCulture)}",
         ModifierType.Integer => Convert.ToInt32(instance.Value).ToString(),
         _ => throw new
                 ArgumentOutOfRangeException($"Unknown modifier type {instance.Type} for modifier {instance.Definition.Name}"),
      };
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
                                     definition.Name,
                                     "A boolean modifier cannot be a percentage or already percent.");
      return false;
   }

   private static bool IsIntegerModifier(ModifierDefinition definition)
   {
      if (definition.NumDecimals != 0)
         return false;

      if (definition is { IsPercentage: false, IsAlreadyPercent: false, IsBoolean: false })
         return true;

      DiagnosticException.LogWarning(LocationContext.Empty,
                                     ParsingError.Instance.InconclusiveModifierTypeDefinition,
                                     $"{nameof(InferModifierType)}.{nameof(IsIntegerModifier)}",
                                     definition.Name,
                                     "An integer modifier cannot be a percentage, already_percent, or boolean.");
      return false;
   }

   private static bool IsFloatingModifier(ModifierDefinition definition)
   {
      if (definition.NumDecimals <= 0)
         return false;

      if (definition is { IsPercentage: false, IsAlreadyPercent: false, IsBoolean: false })
         return true;

      DiagnosticException.LogWarning(LocationContext.Empty,
                                     ParsingError.Instance.InconclusiveModifierTypeDefinition,
                                     $"{nameof(InferModifierType)}.{nameof(IsFloatingModifier)}",
                                     definition.Name,
                                     "A floating modifier cannot be a percentage, already_percent, or boolean.");
      return false;
   }

   private static bool IsPercentageModifier(ModifierDefinition definition)
   {
      if (!definition.IsPercentage)
         return false;

      if (definition is { IsBoolean: false })
         return true;

      DiagnosticException.LogWarning(LocationContext.Empty,
                                     ParsingError.Instance.InconclusiveModifierTypeDefinition,
                                     $"{nameof(InferModifierType)}.{nameof(IsPercentageModifier)}",
                                     definition.Name,
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
                                     definition.Name,
                                     "An identifier modifier cannot be boolean, percentage, or already_percent, and must have 0 decimals.");
      return false;
   }

   #endregion
}