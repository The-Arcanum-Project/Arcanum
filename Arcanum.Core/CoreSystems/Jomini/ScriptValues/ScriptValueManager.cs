namespace Arcanum.Core.CoreSystems.Jomini.ScriptValues;

public static class ScriptValueManager
{
   public static readonly Dictionary<string, IScriptValue> ValueRegistry = new();
}