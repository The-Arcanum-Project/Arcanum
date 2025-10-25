// # define DEBUG_PARSING_STEP_TIMES

#define PRINT_PARSING_ORDER

using System.Diagnostics;
using System.Windows;
#if DEBUG_PARSING_STEP_TIMES
using System.Globalization;
#endif
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Jomini.Effects;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common.SubClasses;
using Arcanum.Core.Registry;
using Arcanum.Core.Utils.Scheduling;
using Arcanum.Core.Utils.Sorting;
using Common.Logger;
using Common.UI;
using Common.UI.MBox;
using JetBrains.Annotations;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public class ParsingMaster
{
   [UsedImplicitly]
   public EventHandler<FileLoadingService>? ParsingStepsChanged;

   public EventHandler<double>? TotalProgressChanged;

   private static HashSet<FileLoadingService> _sortedLoadingSteps = [];

   private static readonly Lazy<ParsingMaster> LazyInstance = new(() => new());
   public static ParsingMaster Instance => LazyInstance.Value;
   public int ParsingSteps => _sortedLoadingSteps.Count;
   public int ParsingStepsDone { get; private set; }
   public List<TimeSpan> StepDurations { get; } = [];

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
         if (_sortedLoadingSteps.TryGetValue(descr, out var parsingStep) && parsingStep.SuccessfullyLoaded)
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
      //_sortedLoadingSteps =
      //   new(TopologicalSort.Sort<string, FileLoadingService>(DescriptorDefinitions.LoadingStepsList));

      var (s1, s2) = PartitionStepsByPriority(DescriptorDefinitions.LoadingStepsList);

      var sortedPrioritySteps = TopologicalSort.Sort<string, FileLoadingService>(s1);
      var sortedRemainingSteps =
         TopologicalSort.Sort<string, FileLoadingService>(DescriptorDefinitions.LoadingStepsList);
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
   }

   /// <summary>
   /// Partitions a given list of steps into two lists: one for priority tasks (including their dependencies)
   /// and one for all remaining tasks.
   /// </summary>
   /// <param name="allAvailableSteps">The complete, unsorted list of all loading steps.</param>
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

   private const bool NO_PARALLEL_EXECUTION = true;

   public async Task<bool> ExecuteAllParsingSteps()
   {
      return true;

      var sw = Stopwatch.StartNew();
      EffectParser.ParseEffectDefinitions();

      InitializeSteps();

      ParsingStepsDone = 0;
      var steps = _sortedLoadingSteps.ToList();
      var lockObj = new object();
      var cts = new CancellationTokenSource();

      while (ParsingStepsDone < ParsingSteps)
      {
         var hasPrioSteps = steps.Any(s => s.HasPriority);
         var readySteps = hasPrioSteps ? [steps[0]] : LookAheadSteps(steps);
         foreach (var step in readySteps)
            steps.Remove(step);

         if (readySteps.Count == 0)
         {
            // Deadlock check: No steps can run, but we are not finished.
            if (ParsingStepsDone < ParsingSteps)
               throw new
                  InvalidOperationException($"Deadlock detected. No steps can be executed, but {ParsingSteps - ParsingStepsDone} steps remain.");

            break;
         }

         List<Task<bool>> currentBatchTasks = [];
         foreach (var step in readySteps)
         {
            if (NO_PARALLEL_EXECUTION)
            {
               try
               {
                  var sw3 = Stopwatch.StartNew();
                  var wrapper = step.GetParsingStep();
                  var result = wrapper.Execute();
                  if (result)
                  {
                     ParsingStepsDone++;
                     StepDurations.Add(wrapper.Duration);
                     ParsingStepsChanged?.Invoke(this, step);
                     TotalProgressChanged?.Invoke(this,
                                                  ParsingStepsDone /
                                                  (double)ParsingSteps *
                                                  100.0);
                  }

                  step.LastTotalLoadingDuration = sw3.Elapsed;
               }
               catch (Exception e)
               {
                  UIHandle.Instance.PopUpHandle
                          .ShowMBox("An Exception occured during parsing step: " +
                                    $"{step.Name}\n\n" +
                                    $"Exception Message: {e.Message}\n\n" +
                                    "Please check the log for more details.",
                                    "Parsing Error",
                                    MBoxButton.OK,
                                    MessageBoxImage.Error);
                  throw
                     new($"Exception occurred while executing parsing step '{step.Name}': {e.Message}",
                         e);
               }
            }
            else
            {
               if (step.IsHeavyStep)
                  currentBatchTasks.Add(Scheduler.QueueHeavyWork(() =>
                                                                 {
                                                                    try
                                                                    {
                                                                       var sw = Stopwatch.StartNew();
                                                                       var wrapper = step.GetParsingStep();
                                                                       var result = wrapper.Execute();
                                                                       lock (lockObj)
                                                                          if (result)
                                                                          {
                                                                             ParsingStepsDone++;
                                                                             StepDurations.Add(wrapper.Duration);
                                                                             ParsingStepsChanged?.Invoke(this, step);
                                                                             TotalProgressChanged?.Invoke(this,
                                                                                 ParsingStepsDone /
                                                                                 (double)ParsingSteps *
                                                                                 100.0);
                                                                          }

                                                                       step.LastTotalLoadingDuration = sw.Elapsed;
                                                                       return result;
                                                                    }
                                                                    catch (Exception e)
                                                                    {
                                                                       UIHandle.Instance.PopUpHandle
                                                                         .ShowMBox("An Exception occured during heavy parsing step: " +
                                                                                 $"{step.Name}\n\n" +
                                                                                 $"Exception Message: {e.Message}\n\n" +
                                                                                 "Please check the log for more details.",
                                                                              "Parsing Error",
                                                                              MBoxButton.OK,
                                                                              MessageBoxImage.Error);
                                                                       throw
                                                                          new($"Exception occurred while executing heavy parsing step '{step.Name}': {e.Message}",
                                                                              e);
                                                                    }
                                                                 },
                                                                 cts.Token));
               else
                  currentBatchTasks.Add(Scheduler.QueueWorkAsHeavyIfAvailable(() =>
                                                                              {
                                                                                 try
                                                                                 {
                                                                                    var sw2 = Stopwatch.StartNew();
                                                                                    var wrapper = step.GetParsingStep();
                                                                                    var result = wrapper.Execute();
                                                                                    lock (lockObj)
                                                                                       if (result)
                                                                                       {
                                                                                          ParsingStepsDone++;
                                                                                          StepDurations
                                                                                            .Add(wrapper.Duration);
                                                                                          ParsingStepsChanged
                                                                                           ?.Invoke(this,
                                                                                                 step);
                                                                                          TotalProgressChanged
                                                                                           ?.Invoke(this,
                                                                                                 ParsingStepsDone /
                                                                                                 (double)ParsingSteps *
                                                                                                 100.0);
                                                                                       }

                                                                                    step.LastTotalLoadingDuration =
                                                                                       sw2.Elapsed;
                                                                                    return result;
                                                                                 }
                                                                                 catch (Exception e)
                                                                                 {
                                                                                    UIHandle.Instance.PopUpHandle
                                                                                      .ShowMBox("An Exception occured during parsing step: " +
                                                                                              $"{step.Name}\n\n" +
                                                                                              $"Exception Message: {e.Message}\n\n" +
                                                                                              "Please check the log for more details.",
                                                                                           "Parsing Error",
                                                                                           MBoxButton.OK,
                                                                                           MessageBoxImage.Error);
                                                                                    throw
                                                                                       new($"Exception occurred while executing parsing step '{step.Name}': {e.Message}",
                                                                                           e);
                                                                                 }
                                                                              },
                                                                              cts.Token));
            }
         }

         if (!NO_PARALLEL_EXECUTION)
         {
            while (currentBatchTasks.Count > 0)
            {
               var finishedTask = await Task.WhenAny(currentBatchTasks);
               currentBatchTasks.Remove(finishedTask);

               if (await finishedTask)
                  continue;

               // A task failed. Cancel all other running tasks and stop.
               await cts.CancelAsync();
               return false;
            }
         }
      }

      Queastor.Queastor.AddIEu5ObjectsToQueastor(Queastor.Queastor.GlobalInstance, Eu5ObjectsRegistry.Eu5Objects);
      Queastor.Queastor.GlobalInstance.RebuildBkTree();

      sw.Stop();

      ArcLog.WriteLine("PMS", LogLevel.INF, $"All parsing steps completed in {sw.Elapsed.TotalSeconds:F2} seconds.");

#if DEBUG_PARSING_STEP_TIMES
      var sortedByDuration = StepDurationsByName.OrderByDescending(t => t.Item2).ToList();

      foreach (var (name, duration) in sortedByDuration)
         ArcLog.WriteLine("PMS",
                          LogLevel.DBG,
                          $"{duration.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture)} s for '{name}'");
#endif

      return await Task.FromResult(true);
   }

   /// Looks ahead in the parsing steps queue to check if the next steps dependencies are already loaded to load them in parallel
   public List<FileLoadingService> LookAheadSteps(List<FileLoadingService> steps)
   {
      if (!Scheduler.IsHyperThreaded)
         return [steps[0]];

      List<FileLoadingService> lookAheadSteps = [];

      for (var i = steps.Count - 1; i >= 0; i--)
      {
         var step = steps[i];
         if (AreDependenciesLoaded(step) && !step.IsHeavyStep ||
             step.IsHeavyStep && lookAheadSteps.Count == 0 && AreDependenciesLoaded(step))
         {
            lookAheadSteps.Add(step);
            steps.RemoveAt(i);
         }
         else if (step.IsHeavyStep && lookAheadSteps.Count > 0)
            break;
      }

      return lookAheadSteps;
   }

   public static bool RemoveAllGroupingNodes(RootNode rn,
                                             LocationContext ctx,
                                             string actionStack,
                                             string source,
                                             ref bool validation,
                                             string[] groupingNodeNames,
                                             out List<StatementNode> sns)
   {
      if (groupingNodeNames.Length == 0)
      {
         sns = rn.Statements;
         return true;
      }

      if (!SimpleObjectParser.StripGroupingNodes(rn,
                                                 ctx,
                                                 actionStack,
                                                 source,
                                                 ref validation,
                                                 groupingNodeNames[0],
                                                 out sns))
         return false;

      for (var i = 1; i < groupingNodeNames.Length; i++)
      {
         if (sns.Count != 1 || !sns[0].IsBlockNode(ctx, source, actionStack, ref validation, out var bn))
            continue;

         if (!SimpleObjectParser.StripGroupingNodes(bn!,
                                                    ctx,
                                                    actionStack,
                                                    source,
                                                    ref validation,
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