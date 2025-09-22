using System.Diagnostics;
using System.IO;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Jomini.Effects;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Utils.Sorting;
using JetBrains.Annotations;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public class ParsingMaster
{
   [UsedImplicitly]
   public EventHandler<FileDescriptor>? ParsingStepsChanged;

   public EventHandler<(double percentage, int doneSteps)>? StepProcessChanged;
   public EventHandler<TimeSpan>? StepDurationEstimationChanged;
   public EventHandler<double>? TotalProgressChanged;

   private static HashSet<FileDescriptor> _sortedFileDescriptors = [];

   private static readonly Lazy<ParsingMaster> LazyInstance = new(() => new());
   public static ParsingMaster Instance => LazyInstance.Value;
   public int ParsingSteps => _sortedFileDescriptors.Count;
   public int ParsingStepsDone { get; private set; }
   public List<TimeSpan> StepDurations { get; } = [];
   public static List<(string, TimeSpan)> StepDurationsByName => _sortedFileDescriptors
                                                                .Select(descriptor
                                                                           => (descriptor.LoadingService.Name,
                                                                               descriptor.LastTotalLoadingDuration))
                                                                .ToList();

   public static bool AreDependenciesLoaded(FileDescriptor descriptor)
   {
      ArgumentNullException.ThrowIfNull(descriptor);
      if (_sortedFileDescriptors.Count == 0)
         throw new InvalidOperationException("Check is only available after calling ExecuteAllParsingSteps() first.");

      var dependentDescriptors =
         TopologicalSort.GetAllDependencies<string, FileDescriptor>(descriptor, StaticData.FileDescriptors);

      if (dependentDescriptors.Count == 0)
         return true;

      foreach (var descr in dependentDescriptors)
      {
         if (_sortedFileDescriptors.TryGetValue(descr, out var parsingStep) && parsingStep.SuccessfullyLoaded)
            continue;

         return false;
      }

      return true;
   }

   public static List<double> GetStepWeightsByFileSize(FileDescriptor descriptor)
   {
      ArgumentNullException.ThrowIfNull(descriptor);
      if (_sortedFileDescriptors.Count == 0)
         throw new InvalidOperationException("Check is only available after calling ExecuteAllParsingSteps() first.");

      List<long> fileSizes = [];
      fileSizes.AddRange(descriptor.Files.Select(fileObj => fileObj.Path.FullPath)
                                   .Select(filePath => new FileInfo(filePath))
                                   .Select(fileInfo => fileInfo.Exists ? fileInfo.Length : 0));

      if (fileSizes.Count == 0)
         return [];

      var totalSize = fileSizes.Sum();
      if (totalSize == 0)
         return [];

      return fileSizes.Select(size => size / (double)totalSize).ToList();
   }

   /// <summary>
   /// Sorts all file descriptors based on their dependencies and initializes the parsing steps.
   /// This method should be called before executing any parsing steps to ensure the correct order of execution
   /// </summary>
   private static void InitializeSteps()
   {
      _sortedFileDescriptors = new(TopologicalSort.Sort<string, FileDescriptor>(DescriptorDefinitions.FileDescriptors));
   }

   public Task<bool> ExecuteAllParsingSteps()
   {
      EffectParser.ParseEffectDefinitions();

      InitializeSteps();

      ParsingStepsDone = 0;
      foreach (var descriptor in _sortedFileDescriptors)
      {
         TotalProgressChanged?.Invoke(this, ParsingStepsDone / (double)ParsingSteps * 100.0);
         ParsingStepsChanged?.Invoke(this, descriptor);

         var stepWrapper = descriptor.LoadingService.GetParsingStep(descriptor);

         stepWrapper.SubStepCompleted += (_, _) =>
         {
            StepProcessChanged?.Invoke(this, (stepWrapper.SubPercentageCompleted, stepWrapper.SubStepsDone));
            StepDurationEstimationChanged?.Invoke(this, stepWrapper.EstimatedRemaining ?? TimeSpan.Zero);
         };

         if (!stepWrapper.Execute())
            break;

         StepDurations.Add(stepWrapper.Duration);
         ParsingStepsDone++;
      }

      if (ParsingSteps != ParsingStepsDone)
         return Task.FromResult(false);

      var sw = Stopwatch.StartNew();
      var items = Queastor.Queastor.GlobalInstance.BkTreeTerms.Count;
      Queastor.Queastor.GlobalInstance.RebuildBkTree();
      sw.Stop();
      Debug.WriteLine($"[Queastor] Rebuilt BK-Tree in {sw.ElapsedMilliseconds} ms for {items} words.");

      return Task.FromResult(true);
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
         if (sns.Count != 1 || !sns[0].IsBlockNode(ctx, source, actionStack, out var bn))
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
}