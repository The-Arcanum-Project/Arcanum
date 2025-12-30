using System.Threading.Channels;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public partial class ParsingMaster
{
   private sealed class GraphContext
   {
      public List<int>[] Dependents = null!;
      public int[] RemainingDependencies = null!;
      public ChannelWriter<int> Writer = null!;
      public TaskCompletionSource<bool> Tcs = null!;
      public CancellationTokenSource Cts = null!;
      public int TotalSteps;
      public int CompletedCount;
      public ParsingMaster CtxInstance = null!;
   }
}