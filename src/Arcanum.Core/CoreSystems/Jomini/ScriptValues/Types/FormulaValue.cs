namespace Arcanum.Core.CoreSystems.Jomini.ScriptValues.Types;

/// <summary>
/// Represents a complex formula with a sequence of operations.
/// Example: my_formula = { value = age; multiply = 0.5; }
/// </summary>
public class FormulaValue : IScriptValue
{
   private readonly IReadOnlyList<IFormulaOperation> _operations;

   public FormulaValue(IReadOnlyList<IFormulaOperation> operations)
   {
      _operations = operations;
   }

   public double Evaluate(EvaluationContext context)
   {
      double currentValue = 0.0;

      // Execute each operation in the order they were defined.
      foreach (var operation in _operations)
      {
         currentValue = operation.Execute(currentValue, context);
      }

      return currentValue;
   }
}