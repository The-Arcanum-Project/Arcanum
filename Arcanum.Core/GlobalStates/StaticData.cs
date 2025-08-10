using Arcanum.Core.CoreSystems.Parsing.DocsParsing;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.GlobalStates;

public static class StaticData
{
   public static List<DocsObj> DocsTriggers { get; set; } = [];
   public static List<DocsObj> DocsEffects { get; set; } = [];
   
   public static List<FileDescriptor> FileDescriptors { get; set; } = [];
}