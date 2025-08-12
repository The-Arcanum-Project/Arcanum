using System.Collections.ObjectModel;
using System.Windows;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.StyleClasses;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class RunLoadingStep
{
   public ObservableCollection<FileDescriptor> Steps { get; set; }

   public RunLoadingStep()
   {
      InitializeComponent();
      Steps = new(DescriptorDefinitions.FileDescriptors);
   }

   private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
   {
      // Execute the selected LoadingStep
      if (sender is BaseButton { Tag: FileDescriptor descriptor })
      {
         var step = descriptor.LoadingService.GetParsingStep(descriptor);
         step.UnloadAllFiles();
         if (!step.Execute())
            AppData.WindowLinker.OpenMainMenuScreen();
         StepResultsTextBox.Text =
            $"{"Step:",-25} '{step.Name}'\n" +
            $"{"Success:",-25} {step.IsSuccessful}\n" +
            $"{"Duration:",-25} {step.Duration.TotalMilliseconds:#####.0} ms\n" +
            $"{"Debug Infos:",-25} {descriptor.LoadingService.GetFileDataDebugInfo()}\n" +
            $"{"Num of Diagnostics:",-25} {step.Diagnostics.Count}\n" +
            $"{"Diagnostics:",-25} {string.Join(", ", step.Diagnostics.Select(d => d.ToString()))}";
      }
   }

   private void DoublePlay_OnClick(object sender, RoutedEventArgs e)
   {
      const int numOfExecutions = 10;
      if (sender is BaseButton { Tag: FileDescriptor descriptor })
      {
         var durations = new TimeSpan[numOfExecutions];
         var step = descriptor.LoadingService.GetParsingStep(descriptor);
         for (var i = 0; i < numOfExecutions; i++)
         {
            step = descriptor.LoadingService.GetParsingStep(descriptor);
            step.UnloadAllFiles();
            if (!step.Execute())
               AppData.WindowLinker.OpenMainMenuScreen();
            durations[i] = step.Duration;
         }

         StepResultsTextBox.Text =
            $"{"Step:",-30} '{step.Name}'\n" +
            $"{"Executed:",-30} {numOfExecutions}x\n" +
            $"{"Average time:",-30} {durations.Select(x => x.TotalMilliseconds).Average():#####.0} ms\n" +
            $"-----------------------------------------------------------------\n" +
            $"{"Last step invocation info",-30}\n" +
            $"{"Success:",-30} {step.IsSuccessful}\n" +
            $"{"Duration:",-30} {step.Duration.TotalMilliseconds:#####.0} ms\n" +
            $"{"Debug Infos:",-30} {descriptor.LoadingService.GetFileDataDebugInfo()}\n" +
            $"{"Num of Diagnostics:",-30} {step.Diagnostics.Count}\n" +
            $"{"Diagnostics:",-30} {string.Join(", ", step.Diagnostics.Select(d => d.ToString()))}";
      }
   }
}