# define DEBUG_PARSING_STEP_TIMES
// #define PRINT_PARSING_ORDER

using System.Diagnostics;
using System.Text;
using System.Threading.Channels;
using System.Windows;
using Arcanum.Core.CoreSystems.ErrorSystem;
using Arcanum.Core.CoreSystems.ErrorSystem.Exceptions;
using Arcanum.Core.CoreSystems.Jomini.Effects;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.CoreSystems.Validators;
using Arcanum.Core.Registry;
using Arcanum.Core.Utils.Scheduling;
using Arcanum.Core.Utils.Sorting;
using Common.UI;
using Common.UI.MBox;
using JetBrains.Annotations;
#if DEBUG_PARSING_STEP_TIMES
using System.Globalization;
#endif

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public partial class ParsingMaster
{
   [UsedImplicitly]
   public EventHandler<FileLoadingService>? ParsingStepsChanged;

   public EventHandler<double>? TotalProgressChanged;

   private static List<FileLoadingService> _sortedLoadingSteps = [];

   private static readonly Lazy<ParsingMaster> LazyInstance = new(() => new());
   public static ParsingMaster Instance => LazyInstance.Value;
   public int ParsingSteps => _sortedLoadingSteps.Count;
   public int ParsingStepsDone { get; private set; }
   // ReSharper disable once CollectionNeverQueried.Global
   public List<TimeSpan> StepDurations { get; } = [];
   public static IValidator[] Validators = [new LocationValidator()];

   public static List<(string, TimeSpan)> StepDurationsByName => _sortedLoadingSteps
                                                                .Select(loading
                                                                           => (loading.Name,
                                                                               loading.LastTotalLoadingDuration))
                                                                .ToList();

   public static bool AreDependenciesLoaded(FileLoadingService descriptor)
   {
      ArgumentNullException.ThrowIfNull(descriptor);
      if (_sortedLoadingSteps.Count == 0)
         throw new InvalidOperationException("Check is only available after calling ExecuteAllParsingSteps() first.");

      var dependentDescriptors =
         TopologicalSort.GetAllDependencies<string, FileLoadingService>(descriptor, _sortedLoadingSteps);

      if (dependentDescriptors.Count == 0)
         return true;

      foreach (var descr in dependentDescriptors)
      {
         var parsingStep = _sortedLoadingSteps.FirstOrDefault(descr);
         if (parsingStep.SuccessfullyLoaded)
            continue;

         return false;
      }

      return true;
   }

   /// <summary>
   /// Sorts all file descriptors based on their dependencies and initializes the parsing steps.
   /// This method should be called before executing any parsing steps to ensure the correct order of execution
   /// </summary>
   private static void InitializeSteps()
   {
      var (s1, _) = PartitionStepsByPriority(DescriptorDefinitions.LoadingStepsList);

      var sortedPrioritySteps = TopologicalSort.SortGeneral<string, FileLoadingService>(s1);
      var sortedRemainingSteps =
         TopologicalSort.SortGeneral<string, FileLoadingService>(DescriptorDefinitions.LoadingStepsList);
      sortedRemainingSteps = sortedRemainingSteps.Except(sortedPrioritySteps).ToList();

      var finalList = sortedPrioritySteps.Concat(sortedRemainingSteps);
      _sortedLoadingSteps = new(finalList);

#if PRINT_PARSING_ORDER
      ArcLog.WriteLine("PMS", LogLevel.INF, "Parsing order of loading steps:");
      var index = 1;
      foreach (var step in _sortedLoadingSteps)
      {
         ArcLog.WriteLine("PMS", LogLevel.INF, $"{index}. {step.Name} (Priority: {step.HasPriority})");
         index++;
      }
#endif
      ArcLog.WriteLine("PMS", LogLevel.INF, "Initialized parsing steps.");
   }

   /// <summary>
   /// Partitions a given list of steps into two lists: one for priority tasks (including their dependencies)
   /// and one for all remaining tasks.
   /// </summary>
   /// <param name="allAvailableSteps">The complete, unsorted list of all loading steps.</param>
   // ReSharper disable once UnusedTupleComponentInReturnValue
   private static (List<FileLoadingService> priority, List<FileLoadingService> remaining) PartitionStepsByPriority(
      IEnumerable<FileLoadingService> allAvailableSteps)
   {
      var allStepsList = allAvailableSteps.ToList();
      var prioritySet = new HashSet<FileLoadingService>();

      foreach (var step in allStepsList)
         if (step.HasPriority)
         {
            prioritySet.Add(step);
            var dependencies = TopologicalSort.GetAllDependencies<string, FileLoadingService>(step, allStepsList);
            foreach (var dep in dependencies)
               prioritySet.Add(dep);
         }

      var prioritySteps = allStepsList.Where(s => prioritySet.Contains(s)).ToList();
      var remainingSteps = allStepsList.Where(s => !prioritySet.Contains(s)).ToList();

      return (prioritySteps, remainingSteps);
   }

   public async Task<bool> ExecuteAllParsingSteps()
   {
      if (ArcLog.IsLevelEnabled(LogLevel.INF))
      {
         ArcLog.WriteLine(CommonLogSource.PMT, LogLevel.INF, string.Concat("ModPath: ", FileManager.ModDataSpace.FullPath));
         foreach (var dpend in FileManager.DependentDataSpaces)
            ArcLog.WriteLine(CommonLogSource.PMT, LogLevel.INF, string.Concat("Dependent DataSpace: ", dpend.FullPath));
      }

      var sw = Stopwatch.StartNew();
      if (!EffectParser.ParseEffectDefinitions())
         return false;

      InitializeSteps();

      var allSteps = _sortedLoadingSteps.ToArray();
      var totalSteps = allSteps.Length;
      ParsingStepsDone = 0;

      ArcLog.WriteLine("PMS", LogLevel.INF, $"Starting parsing steps (Graph Mode). Count: {totalSteps}");

      var idToIndex = new Dictionary<string, int>(totalSteps);
      for (var i = 0; i < totalSteps; i++)
         idToIndex[allSteps[i].Id] = i;

      var dependents = new List<int>[totalSteps];
      var remainingDependencies = new int[totalSteps];

      for (var i = 0; i < totalSteps; i++)
         dependents[i] = [];

      for (var i = 0; i < totalSteps; i++)
      {
         var step = allSteps[i];
         var activeDependencyCount = 0;

         if (step.Dependencies.Length > 0)
         {
            foreach (var parentNode in step.Dependencies)
            {
               if (idToIndex.TryGetValue(parentNode.Id, out var parentId))
               {
                  dependents[parentId].Add(i);
                  activeDependencyCount++;
               }
               else
                  ArcLog.WriteLine(CommonLogSource.PMT,
                                   LogLevel.CRT,
                                   $"Parsing Step '{step.Name}' depends on missing '{parentNode.Id}'. Ignoring.");
            }
         }

         remainingDependencies[i] = activeDependencyCount;
      }

      var readyChannel = Channel.CreateUnbounded<int>(new() { SingleReader = true, SingleWriter = false });
      using var cts = new CancellationTokenSource();
      var completionTcs = new TaskCompletionSource<bool>();

      var context = new GraphContext
      {
         Dependents = dependents,
         RemainingDependencies = remainingDependencies,
         Writer = readyChannel.Writer,
         Tcs = completionTcs,
         Cts = cts,
         TotalSteps = totalSteps,
         CompletedCount = 0,
         CtxInstance = this
      };

      // Seed initial tasks
      for (var i = 0; i < totalSteps; i++)
         if (remainingDependencies[i] == 0)
            readyChannel.Writer.TryWrite(i);

      // Consumer Loop
      try
      {
         while (await readyChannel.Reader.WaitToReadAsync(cts.Token))
         {
            while (readyChannel.Reader.TryRead(out var stepId))
            {
               var step = allSteps[stepId];

               var workItem = () => ExecuteSingleStepLogic(step);

               var task = step.IsHeavyStep
                             ? Scheduler.QueueHeavyWork(workItem, cts.Token)
                             : Scheduler.QueueWorkAsHeavyIfAvailable(workItem, cts.Token);

               _ = task.ContinueWith(OnTaskCompleteStatic,
                                     Tuple.Create(context, stepId, step),
                                     CancellationToken.None,
                                     TaskContinuationOptions.ExecuteSynchronously,
                                     TaskScheduler.Default);
            }
         }
      }
      catch (OperationCanceledException)
      {
         ArcLog.WriteLine(CommonLogSource.PMT, LogLevel.ERR, "Parsing steps execution was canceled.");
         completionTcs.TrySetResult(false);
         SaveLog();

         throw new ParsingCanceledException("Parsing steps execution was canceled.");
      }

      if (!await completionTcs.Task)
         return false;

      Queastor.Queastor.AddIEu5ObjectsToQueastor(Queastor.Queastor.GlobalInstance, Eu5ObjectsRegistry.Eu5Objects);
      Queastor.Queastor.GlobalInstance.RebuildBkTree();

      sw.Stop();
      ArcLog.WriteLine("PMS", LogLevel.INF, $"All parsing steps completed in {sw.Elapsed.TotalSeconds:F2} seconds.");

      foreach (var validator in Validators)
         validator.Validate();

#if DEBUG_PARSING_STEP_TIMES
      {
         ArcLog.WriteLine("PMS", LogLevel.INF, "Parsing Step Durations:");
         foreach (var (name, duration) in StepDurationsByName)
            ArcLog.WriteLine("PMS", LogLevel.INF, $"\t{name}: {duration.TotalMilliseconds.ToString("F2", CultureInfo.InvariantCulture)} ms");
      }
#endif

      SaveLog();
      return true;
   }

   // Static callback to prevent implicit closures
   private static void OnTaskCompleteStatic(Task<bool> t, object? state)
   {
      var (context, stepId, step) = (Tuple<GraphContext, int, FileLoadingService>)state!;

      if (t.IsFaulted || t.IsCanceled || !t.Result)
      {
         ArcLog.WriteLine(CommonLogSource.PMT, LogLevel.ERR, $"Step failed: {step.Name}");
         context.Cts.Cancel();
         context.Tcs.TrySetResult(false);
         return;
      }

      var finishedCount = Interlocked.Increment(ref context.CompletedCount);
      context.CtxInstance.TotalProgressChanged?.Invoke(context.CtxInstance, (double)finishedCount / context.TotalSteps * 100.0);

      var children = context.Dependents[stepId];

      var count = children.Count;
      for (var c = 0; c < count; c++)
      {
         var childId = children[c];
         if (Interlocked.Decrement(ref context.RemainingDependencies[childId]) == 0)
            context.Writer.TryWrite(childId);
      }

      if (finishedCount == context.TotalSteps)
      {
         context.Tcs.TrySetResult(true);
         context.Writer.Complete();
      }
   }

   private bool ExecuteSingleStepLogic(FileLoadingService step)
   {
      var stepSw = Stopwatch.StartNew();
      var wrapper = step.GetParsingStep();

      bool result;
      try
      {
         result = wrapper.Execute();
      }
      catch (Exception e)
      {
         HandleStepException(step, e);
         throw new OperationCanceledException();
      }

      if (result)
      {
         step.LastTotalLoadingDuration = stepSw.Elapsed;
         lock (StepDurations)
            StepDurations.Add(wrapper.Duration);
         ParsingStepsChanged?.Invoke(this, step);
         ParsingStepsDone++;
      }

      return result;
   }

   private static void HandleStepException(FileLoadingService step, Exception e)
   {
      var sb = new StringBuilder();
      sb.AppendLine($"An Exception occurred during parsing step: {step.Name}");
      sb.AppendLine();
      sb.AppendLine($"Exception Message: {e.Message}");
      sb.AppendLine();
      sb.AppendLine("Stack Trace:");
      sb.AppendLine(e.StackTrace);

      var fullMessage = sb.ToString();

      ArcLog.WriteLine(CommonLogSource.PMT, LogLevel.ERR, fullMessage);

      if (!AppData.IsHeadless)
      {
         var uiMessage = $"An Exception occurred during parsing step: {step.Name}\n\n" +
                         $"Exception Message: {e.Message}\n\n" +
                         "Please check the log for more details.";

         UIHandle.Instance.PopUpHandle.ShowMBox(uiMessage,
                                                "Parsing Error",
                                                MBoxButton.OK,
                                                MessageBoxImage.Error);
      }
   }

   private static void SaveLog()
   {
      if (!Config.Settings.ErrorLogOptions.AlwaysExportLogToFile)
         return;

      var sw = new Stopwatch();
      sw.Start();
      try
      {
         ArcLog.WriteLine("PSM", LogLevel.INF, $"Exporting error log to file: \"{Config.Settings.ErrorLogOptions.ErrorLogFileName}\"");
         var sb = new StringBuilder();
         ErrorManager.ExportHumanReadableLog(sb);
         IO.IO.WriteAllTextUtf8(IO.IO.GetErrorLogsFilePath, sb.ToString());
      }
      catch (Exception e)
      {
         ArcLog.WriteLine("PMS",
                          LogLevel.ERR,
                          $"Failed to export error log to '{Config.Settings.ErrorLogOptions.ExportFilePath}'. Exception: {e.Message}");
      }

      sw.Stop();
      ArcLog.WriteLine("PMS", LogLevel.INF, $"Exporting error log completed in {sw.Elapsed.TotalSeconds:F2} seconds.");
   }

   public static bool RemoveAllGroupingNodes(RootNode rn,
                                             ref ParsingContext pc,
                                             string[] groupingNodeNames,
                                             out List<StatementNode> sns)
   {
      if (groupingNodeNames.Length == 0)
      {
         sns = rn.Statements;
         return true;
      }

      if (!SimpleObjectParser.StripGroupingNodes(rn,
                                                 ref pc,
                                                 groupingNodeNames[0],
                                                 out sns))
         return false;

      for (var i = 1; i < groupingNodeNames.Length; i++)
      {
         if (sns.Count != 1 || !sns[0].IsBlockNode(ref pc, out var bn))
            continue;

         if (!SimpleObjectParser.StripGroupingNodes(bn,
                                                    ref pc,
                                                    groupingNodeNames[i],
                                                    out sns))
            return false;
      }

      return true;
   }

   public static void UnloadAll()
   {
      foreach (var descriptor in DescriptorDefinitions.FileDescriptors)
      {
         foreach (var file in descriptor.Files)
            foreach (var ls in descriptor.LoadingService)
               ls.UnloadSingleFileContent(file, descriptor, null);
      }
   }
}