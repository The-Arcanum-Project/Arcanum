using Arcanum.Core.CoreSystems.Jomini.Scopes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.GlobalStates;

public static class StaticData
{
   public static List<ETDefinition> DocsTriggers { get; set; } = [];
   public static List<ETDefinition> DocsEffects { get; set; } = [];

   public static List<FileDescriptor> FileDescriptors { get; set; } = [];
}