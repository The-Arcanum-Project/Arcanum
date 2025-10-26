using Arcanum.Core.CoreSystems.History.Commands;
using Arcanum.Core.CoreSystems.History.Commands.Collections;

namespace Arcanum.Core.CoreSystems.History.Dtos;

public static class CommandDtoManager
{
   public static CommandDto CreateCommandDto(ICommand command)
   {
      return new()
      {
         CommandType = command.GetType().Name, CommandData = command.SerializeToDto(),
      };
   }

   public static ICommand CreateCommand(CommandDto commandDto)
   {
      ICommand command;

      switch (commandDto.CommandType)
      {
         case nameof(SetValueCommand):
            command = new SetValueCommand();
            break;
         case nameof(ClearCollectionCommand):
            command = new ClearCollectionCommand();
            break;
         case nameof(RemoveFromCollectionCommand):
            command = new RemoveFromCollectionCommand();
            break;
         case nameof(AddToCollectionCommand):
            command = new AddToCollectionCommand();
            break;
         case nameof(CInitial):
            command = new CInitial();
            break;
         default:
            throw new NotSupportedException($"Command type '{commandDto.CommandType}' is not supported.");
      }

      command.DeserializeFromDto(commandDto.CommandData);
      return command;
   }
}