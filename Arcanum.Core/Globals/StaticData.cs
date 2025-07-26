using Arcanum.Core.CoreSystems.Parsing.DocsParsing;

namespace Arcanum.Core.Globals;

public static class StaticData
{
   public static List<DocsObj> DocsTriggers { get; set; } = [];
   public static List<DocsObj> DocsEffects { get; set; } = [];
}