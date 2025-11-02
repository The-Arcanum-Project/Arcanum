namespace Arcanum.Core.CoreSystems.Jomini.ScriptValues.Types;

/// <summary>
/// Represents a simple, unchanging numerical value.
/// Example: minor_stress_gain = 10
/// </summary>
public class StaticValue : IScriptValue
{
   private readonly double _value;

   public StaticValue(double value)
   {
      _value = value;
   }

   public double Evaluate(EvaluationContext context)
   {
      // The evaluation is trivial; it just returns its stored value.
      // The context is not needed here.
      return _value;
   }
}