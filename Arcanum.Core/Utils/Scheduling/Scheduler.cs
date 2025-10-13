using System.Collections.Concurrent;
using System.Management;

namespace Arcanum.Core.Utils.Scheduling;

/// <summary>
/// A sophisticated job scheduler that distinguishes between physical and logical CPU cores
/// to optimize task distribution for both heavy (CPU-bound) and light (I/O-bound or short) workloads.
/// </summary>
public sealed class Scheduler : IDisposable
{
   #region Properties

   /// <summary>
   /// Gets the number of physical CPU cores on the system.
   /// </summary>
   public int PhysicalCores { get; }

   /// <summary>
   /// Gets the number of logical processors on the system.
   /// </summary>
   public int LogicalCores { get; }

   /// <summary>
   /// Gets a value indicating whether hyper-threading is active.
   /// </summary>
   public bool IsHyperThreaded { get; }

   /// <summary>
   /// Gets the number of worker threads dedicated to heavy, CPU-bound tasks.
   /// </summary>
   public int HeavyWorkerCount => _heavyWorkers.Count;

   /// <summary>
   /// Gets the number of worker threads dedicated to light, short-lived, or I/O-bound tasks.
   /// </summary>
   public int LightWorkerCount => _lightWorkers.Count;

   /// <summary>
   /// Gets the current number of heavy tasks waiting in the queue.
   /// </summary>
   public int HeavyTasksWaiting => _heavyTaskQueue.Count;

   /// <summary>
   /// Gets the current number of light tasks waiting in the queue.
   /// </summary>
   public int LightTasksWaiting => _lightTaskQueue.Count;

   #endregion

   private readonly List<Thread> _heavyWorkers;
   private readonly List<Thread> _lightWorkers;
   private readonly BlockingCollection<Action> _heavyTaskQueue;
   private readonly BlockingCollection<Action> _lightTaskQueue;
   private readonly CancellationTokenSource _cts = new();
   private bool _isDisposed;

   public Scheduler()
   {
      (PhysicalCores, LogicalCores) = DetectCpuTopology();
      IsHyperThreaded = LogicalCores > PhysicalCores;

      _heavyTaskQueue = new();
      _lightTaskQueue = new();

      // Create workers for physical cores (heavy tasks)
      _heavyWorkers = new(PhysicalCores);
      for (var i = 0; i < PhysicalCores; i++)
      {
         var worker = new Thread(() => WorkerLoop(_heavyTaskQueue, _cts.Token))
         {
            Name = $"JobScheduler-HeavyWorker-{i + 1}", IsBackground = true,
         };
         worker.Start();
         _heavyWorkers.Add(worker);
      }

      // Create workers for logical cores (light tasks) if hyper-threading is on
      var lightWorkerCount = LogicalCores - PhysicalCores;
      _lightWorkers = new(lightWorkerCount);
      if (lightWorkerCount <= 0)
         return;

      for (var i = 0; i < lightWorkerCount; i++)
      {
         var worker = new Thread(() => WorkerLoop(_lightTaskQueue, _cts.Token))
         {
            Name = $"JobScheduler-LightWorker-{i + 1}", IsBackground = true,
         };
         worker.Start();
         _lightWorkers.Add(worker);
      }
   }

   public int AvailableHeavyWorkers => HeavyWorkerCount - HeavyTasksWaiting;
   public int AvailableLightWorkers => LightWorkerCount - LightTasksWaiting;

   /// <summary>
   /// Queues a heavy, CPU-bound task to be executed on a worker thread tied to a physical core.
   /// </summary>
   public Task QueueHeavyWork(Action work)
   {
      var tcs = new TaskCompletionSource<bool>();
      QueueWorkInternal(() =>
                        {
                           try
                           {
                              work();
                              tcs.SetResult(true);
                           }
                           catch (Exception ex)
                           {
                              tcs.SetException(ex);
                           }
                        },
                        _heavyTaskQueue);
      return tcs.Task;
   }

   /// <summary>
   /// Queues a parallel for loop to be executed with a specified degree of parallelism.
   /// This is useful for dividing work across multiple threads for CPU-bound tasks.
   /// </summary>
   /// <param name="count"></param>
   /// <param name="action"></param>
   /// <param name="parallelismDegree"></param>
   /// <returns></returns>
   public Task QueueWorkInForParallel(int count, Action<int> action, int parallelismDegree = -1)
   {
      if (parallelismDegree <= 0)
         parallelismDegree = AvailableHeavyWorkers;

      var options = new ParallelOptions { MaxDegreeOfParallelism = parallelismDegree, };

      return Task.Run(() => Parallel.For(0, count, options, action));
   }

   /// <summary>
   /// Queues a heavy, CPU-bound task with a return value.
   /// </summary>
   public Task<T> QueueHeavyWork<T>(Func<T> work, CancellationToken ctsToken)
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
                        _heavyTaskQueue);
      return tcs.Task;
   }

   /// <summary>
   /// Queues a light, short, or I/O-bound task to be executed on a hyper-threaded core.
   /// If no hyper-threading is available, this will be queued as heavy work.
   /// </summary>
   public Task QueueLightWork(Action work)
   {
      var tcs = new TaskCompletionSource<bool>();
      var queue = LightWorkerCount > 0 ? _lightTaskQueue : _heavyTaskQueue;
      QueueWorkInternal(() =>
                        {
                           try
                           {
                              work();
                              tcs.SetResult(true);
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
   public Task<T> QueueLightWork<T>(Func<T> work, CancellationToken ctsToken)
   {
      var tcs = new TaskCompletionSource<T>();
      var queue = LightWorkerCount > 0 ? _lightTaskQueue : _heavyTaskQueue;
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

   private void QueueWorkInternal(Action work, BlockingCollection<Action> queue)
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
               Console.Error.WriteLine($"[ERROR] Task on thread {Thread.CurrentThread.Name} failed: {ex.Message}");
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

   public void Dispose()
   {
      if (_isDisposed)
         return;

      _isDisposed = true;

      _heavyTaskQueue.CompleteAdding();
      _lightTaskQueue.CompleteAdding();
      _cts.Cancel();

      foreach (var worker in _heavyWorkers.Concat(_lightWorkers))
         worker.Join();

      _heavyTaskQueue.Dispose();
      _lightTaskQueue.Dispose();
      _cts.Dispose();
   }
}