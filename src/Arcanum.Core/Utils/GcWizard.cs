namespace Arcanum.Core.Utils;

public static class GcWizard
{
   public static void SettleMemoryInBackground()
   {
      Task.Run(async () =>
      {
         var startMb = GC.GetTotalMemory(false) / 1024 / 1024;
         // Give the app 5 seconds to finish background thread work, 
         // close async state machines, and return arrays to the ArrayPool.
         await Task.Delay(TimeSpan.FromSeconds(5));

         // 3-pass GC. 
         // Pass 1: Identifies dead objects and queues finalizers.
         // Pass 2: Clears finalized objects and trims ArrayPools.
         // Pass 3: Final compaction and returning memory to the OS.
         for (var i = 0; i < 3; i++)
         {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
            GC.WaitForPendingFinalizers();

            // Give the OS and .NET pools a moment to process the changes
            await Task.Delay(TimeSpan.FromSeconds(2));
         }

         var mb = GC.GetTotalMemory(false) / 1024 / 1024;
         ArcLog.WritePure($"Memory settled at {mb} MB");
      });
   }
}