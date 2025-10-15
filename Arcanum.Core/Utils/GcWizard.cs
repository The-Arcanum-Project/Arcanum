namespace Arcanum.Core.Utils;

public static class GcWizard
{
   // Call the GC to clean up memory for 20 sec 
   public static void ForceGc()
   {
      var thread = new Thread(() =>
      {
         for (var i = 0; i <= 20; i++)
         {
            GC.Collect(i, GCCollectionMode.Forced, true, true);
            GC.WaitForPendingFinalizers();
            Thread.Sleep(300);
         }

         GC.Collect();
      }) { IsBackground = true };
      thread.Start();
   }
}