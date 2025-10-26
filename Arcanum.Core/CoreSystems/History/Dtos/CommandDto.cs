namespace Arcanum.Core.CoreSystems.History.Dtos;

public class CommandDto
{
   public string CommandType { get; set; } = null!;
   public object CommandData { get; set; } = null!;
}