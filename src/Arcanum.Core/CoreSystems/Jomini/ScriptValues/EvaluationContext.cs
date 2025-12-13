namespace Arcanum.Core.CoreSystems.Jomini.ScriptValues;

public class EvaluationContext
{
   // The Character, Player, Province, etc. that the script is currently focused on.
   public object Scope { get; }

   // A reference to the registry that holds all named IScriptValue objects.
   // ReSharper disable once NotAccessedField.Local
   private readonly IReadOnlyDictionary<string, IScriptValue> _valueRegistry;

   public EvaluationContext(object scope, IReadOnlyDictionary<string, IScriptValue> registry)
   {
      Scope = scope;
      _valueRegistry = registry;
   }

   // A helper method to resolve other values, which is the core of chaining.
   public double Resolve(object value)
   {
      // This method will contain the logic to handle if 'value' is already a number,
      // a string to be looked up in the registry, an inline formula, etc.
      // (Implementation details come later)
      throw new NotImplementedException();
   }
}