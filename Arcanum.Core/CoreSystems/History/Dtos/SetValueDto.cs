namespace Arcanum.Core.CoreSystems.History.Dtos;

public class SetValueDto
{
   public object Value { get; set; } = null!;
   public Eu5Dto[] Targets { get; set; } = [];
   public object[] OldValues { get; set; } = [];
}