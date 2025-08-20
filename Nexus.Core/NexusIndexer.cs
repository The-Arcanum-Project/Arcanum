namespace Nexus.Core;

public abstract class NexusIndexer<TField>(INexus owner)
   where TField : Enum
{
   public object this[TField key]
   {
      get => owner._getValue(key);
      set => owner._setValue(key, value);
   }
}