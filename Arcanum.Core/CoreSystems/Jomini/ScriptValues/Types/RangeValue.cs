namespace Arcanum.Core.CoreSystems.Jomini.ScriptValues.Types;

/// <summary>
/// Represents a random value within a defined range.
/// Example: add_gold = { 1 5 } or add_gold = { min_gold max_gold }
/// </summary>
public class RangeValue : IScriptValue
{
   // The operands can be numbers, or strings that name other script values.
   private readonly object _minOperand;
   private readonly object _maxOperand;

   public RangeValue(object minOperand, object maxOperand)
   {
      _minOperand = minOperand;
      _maxOperand = maxOperand;
   }

   public double Evaluate(EvaluationContext context)
   {
      // 1. Resolve the min and max values using the context.
      double min = context.Resolve(_minOperand);
      double max = context.Resolve(_maxOperand);

      // 2. Return a random number in that range.
      return min + (Random.Shared.NextDouble() * (max - min));
   }
}