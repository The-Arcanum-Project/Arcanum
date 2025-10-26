using Arcanum.Core.CoreSystems.History.Dtos;

namespace Arcanum.Core.CoreSystems.History.HistoryDto;

public static class HistoryDtoManager
{
   public static HistoryNodeDto CreateHistoryNodeDto(HistoryNode node)
   {
      return new()
      {
         Id = node.Id,
         Children = CreateHistoryNodeDtoList(node.Children),
         EntryType = node.EntryType,
         Command = CommandDtoManager.CreateCommandDto(node.Command),
      };
   }

   public static List<HistoryNodeDto> CreateHistoryNodeDtoList(IList<HistoryNode> nodes)
   {
      var dtoList = new List<HistoryNodeDto>();
      foreach (var node in nodes)
         dtoList.Add(CreateHistoryNodeDto(node));
      return dtoList;
   }

   public static HistoryNode CreateHistoryNodeFromDto(HistoryNodeDto dto)
   {
      var node = new HistoryNode(dto.Id, CommandDtoManager.CreateCommand(dto.Command), dto.EntryType)
      {
         IsCompacted = false,
      };
      foreach (var childDto in dto.Children)
      {
         var childNode = CreateHistoryNodeFromDto(childDto);
         childNode.Parent = node;
         node.Children.Add(childNode);
      }

      return node;
   }

   public static HistoryNodeDto GetHistoryAsDto()
   {
      AppData.HistoryManager.UncompactTree();
      return CreateHistoryNodeDto(AppData.HistoryManager.Root);
   }

   public static void LoadHistoryFromDto(HistoryNodeDto rootDto)
   {
      // TODO proper handling of all edge cases.
      //AppData.HistoryManager.SetRoot(CreateHistoryNodeFromDto(rootDto), FindMaxNodeId(rootDto));
   }

   private static int FindMaxNodeId(HistoryNodeDto nodeDto)
   {
      var maxId = nodeDto.Id;
      foreach (var childDto in nodeDto.Children)
      {
         var childMaxId = FindMaxNodeId(childDto);
         if (childMaxId > maxId)
            maxId = childMaxId;
      }

      return maxId;
   }
}