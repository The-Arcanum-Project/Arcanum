namespace Arcanum.Core.CoreSystems.History.Dtos;

public class ClearCollectionDto
{
   public Eu5Dto[] Targets { get; set; } = [];
   public object[][] OldValues { get; set; } = [];
}