using Arcanum.Core.CoreSystems.Parsing.DocsParsing;

namespace Arcanum.Core.GlobalStates;

public static class StaticData
{
   public static List<DocsObj> DocsTriggers { get; set; } = [];
   public static List<DocsObj> DocsEffects { get; set; } = [];
}