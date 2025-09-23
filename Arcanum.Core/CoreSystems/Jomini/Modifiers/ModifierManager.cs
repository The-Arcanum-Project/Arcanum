using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Jomini.Effects;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.GameObjects.Common;

namespace Arcanum.Core.CoreSystems.Jomini.Modifiers;

public static class ModifierManager
{
   /// <summary>
   /// If the key exists in the global modifier definitions, the inferred type is determined and
   /// the value is validated against that type, and instance is created and returned. <br/>
   /// If any of these steps fail, a diagnostic warning is logged and false is returned.
   /// </summary>
   /// <param name="ctx"></param>
   /// <param name="nodeKeyNode"></param>
   /// <param name="source"></param>
   /// <param name="value"></param>
   /// <param name="validation"></param>
   /// <param name="instance"></param>
   /// <returns></returns>
   public static bool TryCreateModifierInstance(LocationContext ctx,
                                                Token nodeKeyNode,
                                                string source,
                                                string value,
                                                ref bool validation,
                                                [MaybeNullWhen(false)] out ModValInstance instance)
   {
      var key = nodeKeyNode.GetLexeme(source);
      instance = null;
      if (!Globals.ModifierDefinitions.TryGetValue(key, out var definition))
      {
         ctx.SetPosition(nodeKeyNode);
         DiagnosticException.LogWarning(ctx,
                                        ParsingError.Instance.UndefinedModifierKey,
                                        nameof(ModifierManager),
                                        key,
                                        value);
         validation = false;
         return false;
      }

      if (!InferModifierType(definition, out var inferredType) && inferredType is not null)
      {
         ctx.SetPosition(nodeKeyNode);
         DiagnosticException.LogWarning(ctx,
                                        ParsingError.Instance.ImpossibleModifierTypeInferring,
                                        nameof(ModifierManager),
                                        key,
                                        "Could not infer modifier type from definition.");
         validation = false;
         return false;
      }

      if (!EffectManager.TryConvertValue(ctx,
                                         nodeKeyNode,
                                         ref validation,
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

   private static bool IsIntegerModifier(ModifierDefinition definition)
   {
      if (definition.NumDecimals != 0)
         return false;

      if (definition is { IsPercentage: false, IsAlreadyPercent: false, IsBoolean: false })
         return true;

      DiagnosticException.LogWarning(LocationContext.Empty,
                                     ParsingError.Instance.InconclusiveModifierTypeDefinition,
                                     $"{nameof(InferModifierType)}.{nameof(IsIntegerModifier)}",
                                     definition.UniqueId,
                                     "An integer modifier cannot be a percentage, already_percent, or boolean.");
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
      if (!definition.IsPercentage)
         return false;

      if (definition is { IsBoolean: false })
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
}