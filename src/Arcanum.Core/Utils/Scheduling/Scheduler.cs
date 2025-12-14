using System.Collections.Concurrent;
using System.Management;

namespace Arcanum.Core.Utils.Scheduling;

/// <summary>
/// A sophisticated job scheduler that distinguishes between physical and logical CPU cores
/// to optimize task distribution for both heavy (CPU-bound) and light (I/O-bound or short) workloads.
/// </summary>
public static class Scheduler
{
   #region Properties

   /// <summary>
   /// Gets the number of physical CPU cores on the system.
   /// </summary>
   public static int PhysicalCores { get; }

   /// <summary>
   /// Gets the number of logical processors on the system.
   /// </summary>
   public static int LogicalCores { get; }

   /// <summary>
   /// Gets a value indicating whether hyper-threading is active.
   /// </summary>
   public static bool IsHyperThreaded { get; }

   /// <summary>
   /// Gets the number of worker threads dedicated to heavy, CPU-bound tasks.
   /// </summary>
   public static int HeavyWorkerCount => HeavyWorkers.Count;

   /// <summary>
   /// Gets the number of worker threads dedicated to light, short-lived, or I/O-bound tasks.
   /// </summary>
   public static int LightWorkerCount => LightWorkers.Count;

   /// <summary>
   /// Gets the current number of heavy tasks waiting in the queue.
   /// </summary>
   public static int HeavyTasksWaiting => HeavyTaskQueue.Count;

   /// <summary>
   /// Gets the current number of light tasks waiting in the queue.
   /// </summary>
   public static int LightTasksWaiting => LightTaskQueue.Count;

   #endregion

   private static readonly List<Thread> HeavyWorkers;
   private static readonly List<Thread> LightWorkers;
   private static readonly BlockingCollection<Action> HeavyTaskQueue;
   private static readonly BlockingCollection<Action> LightTaskQueue;
   private static readonly CancellationTokenSource Cts = new();
   private static bool _isDisposed;

   static Scheduler()
   {
      (PhysicalCores, LogicalCores) = DetectCpuTopology();
      IsHyperThreaded = LogicalCores > PhysicalCores;

      HeavyTaskQueue = new();
      LightTaskQueue = new();

      // Create workers for physical cores (heavy tasks)
      HeavyWorkers = new(PhysicalCores);
      for (var i = 0; i < PhysicalCores; i++)
      {
         var worker = new Thread(() => WorkerLoop(HeavyTaskQueue, Cts.Token))
         {
            Name = $"JobScheduler-HeavyWorker-{i + 1}", IsBackground = true,
         };
         worker.Start();
         HeavyWorkers.Add(worker);
      }

      // Create workers for logical cores (light tasks) if hyper-threading is on
      var lightWorkerCount = LogicalCores - PhysicalCores;
      LightWorkers = new(lightWorkerCount);
      if (lightWorkerCount <= 0)
         return;

      for (var i = 0; i < lightWorkerCount; i++)
      {
         var worker = new Thread(() => WorkerLoop(LightTaskQueue, Cts.Token))
         {
            Name = $"JobScheduler-LightWorker-{i + 1}", IsBackground = true,
         };
         worker.Start();
         LightWorkers.Add(worker);
      }
   }

   public static int AvailableHeavyWorkers => HeavyWorkerCount - HeavyTasksWaiting;
   public static int AvailableLightWorkers => LightWorkerCount - LightTasksWaiting;

   /// <summary>
   /// Queues a heavy, CPU-bound task to be executed on a worker thread tied to a physical core.
   /// </summary>
   public static Task QueueHeavyWork(Action work, CancellationToken ctsToken)
   {
      var tcs = new TaskCompletionSource<bool>();
      QueueWorkInternal(() =>
                        {
                           try
                           {
                              if (ctsToken.IsCancellationRequested)
                              {
                                 tcs.SetCanceled(ctsToken);
                                 return;
                              }

                              work();
                              tcs.SetResult(true);
                           }
                           catch (OperationCanceledException)
                           {
                              tcs.SetCanceled(ctsToken);
                           }
                           catch (Exception ex)
                           {
                              tcs.SetException(ex);
                           }
                        },
                        HeavyTaskQueue);
      return tcs.Task;
   }

   /// <summary>
   /// Queues multiple heavy, CPU-bound tasks to be executed in parallel on worker threads tied to physical cores.
   /// The tasks are distributed evenly across the available heavy workers. <br/>
   /// If <code>parallelismDegree is -1</code>, it uses all available heavy workers.  <br/>
   /// If <code>parallelismDegree is -2</code>, it uses as many workers as there are tasks. <br/>
   /// If <code>parallelismDegree is positive</code>, it uses that many workers, capped at the number of tasks. <br/>
   /// </summary>
   public static Task QueueWorkInForParallel(int count, Action<int> action, int parallelismDegree = -1)
   {
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

      var degree = parallelismDegree switch
      {
         -1 => Math.Clamp(AvailableHeavyWorkers, 1, count),
         -2 => count,
         < 1 => throw new ArgumentOutOfRangeException(nameof(parallelismDegree)),
         _ => Math.Min(parallelismDegree, count),
      };

      degree = Math.Min(degree, PhysicalCores * 4);

      return Task.Run(() =>
      {
         Parallel.For(0,
                      degree,
                      new() { MaxDegreeOfParallelism = degree },
                      pi => { CalculatePartitionRangeAndExecute(count, action, degree, pi); });
      });
   }

   private static void CalculatePartitionRangeAndExecute(int count,
                                                         Action<int> action,
                                                         int parallelismDegree,
                                                         int partitionIndex)
   {
      var (start, end) = (partitionIndex * count / parallelismDegree,
                          (partitionIndex + 1) * count / parallelismDegree);
      for (var i = start; i < end; i++)
         action(i);
   }

   public static Task<T> QueueWorkAsHeavyIfAvailable<T>(Func<T> work, CancellationToken ctsToken)
   {
      return AvailableHeavyWorkers > 0 ? QueueHeavyWork(work, ctsToken) : QueueLightWork(work, ctsToken);
   }

   public static Task QueueWorkAsHeavyIfAvailable(Action work, CancellationToken ctsToken)
   {
      return AvailableHeavyWorkers > 0 ? QueueHeavyWork(work, ctsToken) : QueueLightWork(work, ctsToken);
   }

   /// <summary>
   /// Queues a heavy, CPU-bound task with a return value.
   /// </summary>
   public static Task<T> QueueHeavyWork<T>(Func<T> work, CancellationToken ctsToken)
   {
      var tcs = new TaskCompletionSource<T>();
      QueueWorkInternal(() =>
                        {
                           try
                           {
                              if (ctsToken.IsCancellationRequested)
                              {
                                 tcs.SetCanceled(ctsToken);
                                 return;
                              }

                              tcs.SetResult(work());
                           }
                           catch (OperationCanceledException)
                           {
                              tcs.SetCanceled(ctsToken);
                           }
                           catch (Exception ex)
                           {
                              tcs.SetException(ex);
                           }
                        },
                        HeavyTaskQueue);
      return tcs.Task;
   }

   /// <summary>
   /// Queues a light, short, or I/O-bound task to be executed on a hyper-threaded core.
   /// If no hyper-threading is available, this will be queued as heavy work.
   /// </summary>
   public static Task QueueLightWork(Action work, CancellationToken ctsToken)
   {
      var tcs = new TaskCompletionSource<bool>();
      var queue = LightWorkerCount > 0 ? LightTaskQueue : HeavyTaskQueue;
      QueueWorkInternal(() =>
                        {
                           try
                           {
                              if (ctsToken.IsCancellationRequested)
                              {
                                 tcs.SetCanceled(ctsToken);
                                 return;
                              }

                              work();
                              tcs.SetResult(true);
                           }
                           catch (OperationCanceledException)
                           {
                              tcs.SetCanceled(ctsToken);
                           }
                           catch (Exception ex)
                           {
                              tcs.SetException(ex);
                           }
                        },
                        queue);
      return tcs.Task;
   }

   /// <summary>
   /// Queues a light, short, or I/O-bound task with a return value.
   /// </summary>
   public static Task<T> QueueLightWork<T>(Func<T> work, CancellationToken ctsToken)
   {
      var tcs = new TaskCompletionSource<T>();
      var queue = LightWorkerCount > 0 ? LightTaskQueue : HeavyTaskQueue;
      QueueWorkInternal(() =>
                        {
                           try
                           {
                              if (ctsToken.IsCancellationRequested)
                              {
                                 tcs.SetCanceled(ctsToken);
                                 return;
                              }

                              tcs.SetResult(work());
                           }
                           catch (OperationCanceledException)
                           {
                              tcs.SetCanceled(ctsToken);
                           }
                           catch (Exception ex)
                           {
                              tcs.SetException(ex);
                           }
                        },
                        queue);
      return tcs.Task;
   }

   private static void QueueWorkInternal(Action work, BlockingCollection<Action> queue)
   {
      if (_isDisposed)
         throw new ObjectDisposedException(nameof(Scheduler));

      queue.Add(work);
   }

   private static void WorkerLoop(BlockingCollection<Action> queue, CancellationToken token)
   {
      try
      {
         foreach (var work in queue.GetConsumingEnumerable(token))
            try
            {
               work();
            }
            catch (Exception ex)
            {
               ArcLog.WriteLine("SCH", LogLevel.CRT, $"Task on thread {Thread.CurrentThread.Name} failed: {ex.Message}");
            }
      }
      catch (OperationCanceledException)
      {
         /* Expected on shutdown */
      }
   }

   /// <summary>
   /// This is the method that uses the System.Management library.
   /// </summary>
   private static (int physicalCores, int logicalCores) DetectCpuTopology()
   {
      // This implementation is Windows-specific due to WMI.
#if WINDOWS
      try
      {
         var physicalCoreCount = 0;
         using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
         foreach (var item in searcher.Get())
            physicalCoreCount += Convert.ToInt32(item["NumberOfCores"]);

         return (physicalCoreCount > 0 ? physicalCoreCount : Environment.ProcessorCount, Environment.ProcessorCount);
      }
      catch
      {
         // Fallback if WMI fails for any reason
         return (Environment.ProcessorCount, Environment.ProcessorCount);
      }
#else
        // Fallback for non-Windows platforms
        Console.WriteLine("WARNING: Non-Windows platform. Cannot detect physical cores. Treating all cores as physical.");
        return (Environment.ProcessorCount, Environment.ProcessorCount);
#endif
   }

   public static void CleanUp()
   {
      if (_isDisposed)
         return;

      _isDisposed = true;

      HeavyTaskQueue.CompleteAdding();
      LightTaskQueue.CompleteAdding();
      Cts.Cancel();

      foreach (var worker in HeavyWorkers.Concat(LightWorkers))
         worker.Join();

      HeavyTaskQueue.Dispose();
      LightTaskQueue.Dispose();
      Cts.Dispose();
   }
}