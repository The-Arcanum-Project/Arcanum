using System.DirectoryServices.ActiveDirectory;
using System.IO;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Utils.Parsing.ParsingStep;
using Arcanum.Core.Utils.Parsing.Steps;
using Arcanum.Core.Utils.Sorting;
using JetBrains.Annotations;

namespace Arcanum.Core.Utils.Parsing.ParsingMaster;

public class ParsingMaster
{
   private readonly List<ParsingStepBase> _parsingSteps = [];

   [UsedImplicitly]
   public EventHandler<ParsingStepBase>? ParsingStepsChanged;

   public EventHandler<(double percentage, int doneSteps)>? StepProcessChanged;
   public EventHandler<TimeSpan>? StepDurationEstimationChanged;
   public EventHandler<double>? TotalProgressChanged;

   private static Dictionary<FileDescriptor, ParsingStepBase> _stepByDescriptor = [];

   /// <summary>
   /// Here ALL loading steps should be added.
   /// </summary>
   private ParsingMaster()
   {
      _parsingSteps.Add(new DebugStep());
      _parsingSteps.Add(new LocationLoading());
   }

   private static readonly Lazy<ParsingMaster> LazyInstance = new(() => new());
   public static ParsingMaster Instance => LazyInstance.Value;
   public int ParsingSteps => _parsingSteps.Count;
   public int ParsingStepsDone { get; private set; }

   public static bool AreDependenciesLoaded(ParsingStepBase step)
   {
      ArgumentNullException.ThrowIfNull(step);
      if (_stepByDescriptor.Count == 0)
         throw new InvalidOperationException("Check is only available after calling ExecuteAllParsingSteps() first.");

      var dependentDescriptors =
         TopologicalSort.GetAllDependencies<string, FileDescriptor>(step.Descriptor, StaticData.FileDescriptors);

      if (dependentDescriptors.Count == 0)
         return true;

      foreach (var descriptor in dependentDescriptors)
      {
         if (_stepByDescriptor.TryGetValue(descriptor, out var parsingStep) && parsingStep.IsSuccessful)
            continue;

         return false;
      }

      return true;
   }

   public static List<double> GetStepWeightsByFileSize(FileDescriptor descriptor)
   {
      ArgumentNullException.ThrowIfNull(descriptor);
      if (_stepByDescriptor.Count == 0)
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

   private void InitializeSteps()
   {
      _stepByDescriptor = _parsingSteps.ToDictionary(step => step.Descriptor, step => step);
   }

   public Task ExecuteAllParsingSteps()
   {
      InitializeSteps();
      List<TimeSpan> durations = [];

      var cts = new CancellationTokenSource();

      ParsingStepsDone = 0;
      foreach (var step in _parsingSteps)
      {
         TotalProgressChanged?.Invoke(this, ParsingStepsDone / (double)ParsingSteps * 100.0);
         ParsingStepsChanged?.Invoke(this, step);
         step.SubStepCompleted += (_, _) =>
         {
            StepProcessChanged?.Invoke(this, (step.SubPercentageCompleted, step.SubStepsDone));
            StepDurationEstimationChanged?.Invoke(this, step.EstimatedRemaining ?? TimeSpan.Zero);
         };

         step.Execute(cts.Token);

         if (cts.IsCancellationRequested)
            break;

         durations.Add(step.Duration);
         ParsingStepsDone++;
      }

      Thread.Sleep(2000);

      return Task.CompletedTask;
   }
}