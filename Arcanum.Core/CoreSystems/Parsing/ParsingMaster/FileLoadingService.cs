using System.Diagnostics;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster.ParsingStep;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

/// <summary>
/// Defines how a file is loaded and parsed. <br/>
/// How to add it to the loading / parsing system:<br/>
/// 1. Create a new instance of <see cref="FileLoadingService"/> and a <see cref="Arcanum.Core.CoreSystems.SavingSystem.Services.ISavingService"/> <br/>
/// 2. Create a new <see cref="FileDescriptor"/> with the instance of these in <see cref="DescriptorDefinitions"/><br/>
/// 3. Make sure to define all dependencies in the <see cref="FileDescriptor.Dependencies"/> <br/>
/// 4. Add it to the <see cref="DescriptorDefinitions.FileDescriptors"/> list<br/>
/// </summary>
public abstract class FileLoadingService : IDependencyNode<string>
{
   
   public IEnumerable<IDependencyNode<string>> Dependencies { get; }
   public string Name { get; }
   private readonly Stopwatch _stopwatch = new();
   protected TimeSpan Duration => _stopwatch.Elapsed;
   
   public TimeSpan LastTotalLoadingDuration { get; set; } = TimeSpan.Zero;
   public bool SuccessfullyLoaded { get; set; } = false;
   

   //TODO @MelCo make dependencies optional for automatic dependency resolution
   protected FileLoadingService(IEnumerable<IDependencyNode<string>> dependencies)
   {
      dependencies ??= [];
      Dependencies = dependencies;
      Name = GetType().Name;
   }

   /// <summary>
   /// Returns a list of types that this file loading service creates / parses from the file
   /// </summary>
   public abstract List<Type> ParsedObjects { get; }

   protected string GetActionName([System.Runtime.CompilerServices.CallerMemberName] string caller = "")
   {
      var declaringType = GetType();
      return $"{declaringType.FullName}.{caller}";
   }

   public virtual DefaultParsingStep GetParsingStep(FileDescriptor descriptor)
   {
      return new(this, descriptor.IsMultithreadable);
   }

   /// <summary>
   /// Returns debug information about any data contained in this filetype.
   /// </summary>
   /// <returns></returns>
   public abstract string GetFileDataDebugInfo();

   /// <summary>
   /// Executes the file loading and measures the time taken
   /// The time can be extracted by using the <see cref="Duration"/> property.
   /// This method is intended to be used for performance measurement and logging and has to be thread-safe.
   /// Multiple instances of this will be called in parallel
   /// </summary>
   /// <param name="fileObj"></param>
   /// <param name="descriptor"></param>
   /// <returns></returns>
   public virtual void LoadSingleFileWithMetrics(Eu5FileObj fileObj, FileDescriptor descriptor)
   {
      _stopwatch.Restart();

      LoadWithErrorHandling(fileObj, descriptor, null);

      _stopwatch.Stop();
   }

   public ReloadFileException? LoadWithErrorHandling(Eu5FileObj fileObj,
                                                     FileDescriptor descriptor,
                                                     object? lockObject)
   {
      var repeatLoading = true;
      while (repeatLoading)
         try
         {
            LoadSingleFile(fileObj, descriptor, lockObject);
            repeatLoading = false;
         }
         catch (ReloadFileException ex)
         {
            if (ex.IsCritical)
               return ex;

            UnloadSingleFileContent(fileObj, descriptor, lockObject);
         }

      return null;
   }

   public ReloadFileException? LoadAfterStepWithErrorHandling(Eu5FileObj fileObj,
                                                              FileDescriptor descriptor,
                                                              object? lockObject)
   {
      var isReloading = false;
      var repeatLoading = true;
      while (repeatLoading)
         try
         {
            if (isReloading)
               LoadSingleFile(fileObj, descriptor, lockObject);
            SingleFileAfterLoadingStep(fileObj, descriptor, lockObject);
            repeatLoading = false;
         }
         catch (ReloadFileException ex)
         {
            if (ex.IsCritical)
               return ex;

            isReloading = true;
            UnloadSingleFileContent(fileObj, descriptor, lockObject);
         }

      return null;
   }

   /// <summary>
   /// This step is called once all files of this step have been loaded.
   /// </summary>
   public virtual bool SingleFileAfterLoadingStep(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      return true;
   }

   /// <summary>
   /// This step is executed after the <see cref="SingleFileAfterLoadingStep"/> and is only executed once per file descriptor.
   /// This is useful for example to resolve references between objects that are parsed in different files.
   /// </summary>
   public virtual bool AfterLoadingStep(FileDescriptor descriptor)
   {
      return true;
   }

   /// <summary>
   /// Loads a single file.
   /// Has to be thread-safe and should not be called directly but always by a manager that handles the performance measurement.
   /// </summary>
   /// <returns></returns>
   public abstract bool LoadSingleFile(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject);

   public abstract bool UnloadSingleFileContent(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject);

   public virtual void ReloadFile(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      UnloadSingleFileContent(fileObj, descriptor, lockObject);
      LoadSingleFileWithMetrics(fileObj, descriptor);
   }

   public virtual bool IsFullyParsed => true;
   public string Id => Name;
}