using System.Diagnostics;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Scheduling;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster.ParsingStep;

public class DefaultParsingStep
{
   private readonly Stopwatch _stopwatch = new();
   private readonly object _lock = new();

   private bool _isSuccessful;
   public bool IsMultithreadable { get; }
   public FileDescriptor Descriptor => LoadingService.Descriptor;

   public FileLoadingService LoadingService { get; }
   public List<Diagnostic> Diagnostics { get; } = [];
   public TimeSpan Duration { get; private set; }
   public bool IsSuccessful
   {
      get => _isSuccessful;
      private set => _isSuccessful = value;
   }
   public string Name { get; }

   public DefaultParsingStep(FileLoadingService loadingService, bool isMultithreadable)
   {
      LoadingService = loadingService;
      IsMultithreadable = isMultithreadable;
      Name = GetType().Name;
   }

   /// <summary>
   /// This is the main method which will be executed to perform the parsing step.
   /// To have a proper estimation of the remaining time, it is recommended to set the step weights when using multiple files or steps.
   /// </summary>
   /// <returns></returns>
   public virtual bool Execute()
   {
      SetupExecutionContext();

      if (IsMultithreadable)
      {
         var files = Descriptor.Files;

         try
         {
            CancellationTokenSource cts = new();
            var cancellationToken = cts.Token;

            Scheduler.QueueWorkInForParallel(files.Count,
                                             i =>
                                             {
                                                ParallelProcessFileStep(files,
                                                                        i,
                                                                        ParallelLoadWithErrorHandling,
                                                                        cts,
                                                                        cancellationToken);
                                             },
                                             files.Count);

            Scheduler.QueueWorkInForParallel(files.Count,
                                             i =>
                                             {
                                                ParallelProcessFileStep(files,
                                                                        i,
                                                                        ParallelAfterLoadingStepErrorHandling,
                                                                        cts,
                                                                        cancellationToken);
                                             },
                                             files.Count);
         }
         catch (OperationCanceledException)
         {
            Volatile.Write(ref _isSuccessful, false);
            Diagnostics.Add(new(ParsingError.Instance.ParsingBaseStepFailure,
                                LocationContext.Empty,
                                DiagnosticSeverity.Error,
                                $"{GetType().Name} failed to load one or more files.",
                                "One or more files could not be loaded successfully during the parsing step.",
                                $"The parsing step {Name} encountered errors while loading files. Please check the diagnostics for more details."));
         }
      }
      else
      {
         var swInner = Stopwatch.StartNew();
         var files = Descriptor.Files;
         files = files.OrderBy(x => x.Path.FullPath).ToList();
         foreach (var file in files)
         {
            FileStateManager.RegisterPath(file.Path);
            if (LoadingService.LoadWithErrorHandling(file, Descriptor, null) is { IsCritical: true })
               return false;
         }

         foreach (var file in Descriptor.Files)
            if (LoadingService.LoadAfterStepWithErrorHandling(file, Descriptor, null) is { IsCritical: true })
               return false;

         swInner.Stop();
         Debug.WriteLine($"Single-threaded parsing step {Name} took {swInner.Elapsed.TotalMilliseconds:#####.0} ms to complete.");
      }

      if (!LoadingService.AfterLoadingStep(Descriptor))
         IsSuccessful = false;

      FinalizeExecutionContext(IsSuccessful);
      return IsSuccessful;
   }

   private void ParallelProcessFileStep(List<Eu5FileObj> files,
                                        int i,
                                        Func<Eu5FileObj, ReloadFileException?> handle,
                                        CancellationTokenSource cts,
                                        CancellationToken cancellationToken)
   {
      var file = files[i];
      FileStateManager.RegisterPath(file.Path);
      if (cancellationToken.IsCancellationRequested)
      {
         Volatile.Write(ref _isSuccessful, false);
         return;
      }

      var ex = handle(file);

      if (ex is { IsCritical: true })
         cts.Cancel();
   }

   private ReloadFileException? ParallelLoadWithErrorHandling(Eu5FileObj file)
   {
      return LoadingService.LoadWithErrorHandling(file, Descriptor, lockObject: _lock);
   }

   private ReloadFileException? ParallelAfterLoadingStepErrorHandling(Eu5FileObj file)
   {
      return LoadingService.LoadAfterStepWithErrorHandling(file, Descriptor, lockObject: _lock);
   }

   protected void FinalizeExecutionContext(bool isSuccessful)
   {
      _stopwatch.Stop();
      Duration = _stopwatch.Elapsed;
      LoadingService.SuccessfullyLoaded = isSuccessful;
   }

   protected void SetupExecutionContext()
   {
      IsSuccessful = true;
      if (!ParsingMaster.AreDependenciesLoaded(LoadingService))
         throw
            new InvalidOperationException($"Cannot execute parsing step {Name} because dependencies are not loaded.");

      _stopwatch.Restart();
      Duration = TimeSpan.Zero;
      SubPercentageCompleted = 0.0;
      SubStepsDone = 0;
   }

   public bool UnloadAllFiles()
   {
      foreach (var file in Descriptor.Files)
         if (!LoadingService.UnloadSingleFileContent(file, Descriptor, null))
            return false;

      return true;
   }

   public double SubPercentageCompleted { get; private set; }
   public int SubStepsDone { get; private set; }
}