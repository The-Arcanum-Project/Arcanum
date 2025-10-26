using Arcanum.Core.CoreSystems.History.Dtos;

namespace Arcanum.Core.CoreSystems.History.HistoryDto;

public class HistoryNodeDto
{
   public int Id { get; set; }
   public CommandDto Command { get; set; } = null!;
   public HistoryEntryType EntryType { get; set; }
   public bool IsCompacted { get; set; }
   // Flatten the hierarchy: List of child DTOs, no Parent property.
   public List<HistoryNodeDto> Children { get; set; } = [];
}