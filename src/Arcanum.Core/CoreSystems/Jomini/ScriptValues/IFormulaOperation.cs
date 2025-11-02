namespace Arcanum.Core.CoreSystems.Jomini.ScriptValues;

/// <summary>
/// Represents a single operation within a formula, like 'add = 5' or 'max = 10'.
/// </summary>
public interface IFormulaOperation
{
   /// <summary>
   /// Executes the operation.
   /// </summary>
   /// <param name="currentValue">The value from the previous step of the formula.</param>
   /// <param name="context">The current evaluation context.</param>
   /// <returns>The new value after this operation is applied.</returns>
   double Execute(double currentValue, EvaluationContext context);
}