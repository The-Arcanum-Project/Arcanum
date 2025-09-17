namespace Arcanum.Core.CoreSystems.Jomini.ScriptValues;

/// <summary>
/// Represents any value that can be defined in script,
/// capable of being evaluated to a final numerical result.
/// </summary>
public interface IScriptValue
{
   /// <summary>
   /// Calculates the final numerical value based on a given context.
   /// </summary>
   /// <param name="context">The current state (scope, other values) needed for evaluation.</param>
   /// <returns>A double-precision floating point number.</returns>
   double Evaluate(EvaluationContext context);
}