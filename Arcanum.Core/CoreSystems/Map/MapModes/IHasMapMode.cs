namespace Arcanum.Core.CoreSystems.Map.MapModes;

public interface IHasMapMode
{
   public static abstract IMapMode GetMapMode { get; }
}