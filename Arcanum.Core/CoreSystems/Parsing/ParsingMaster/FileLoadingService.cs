using System.Diagnostics;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingStep;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

/// <summary>
/// Defines how a file is loaded and parsed. <br/>
/// How to add it to the loading / parsing system:<br/>
/// 1. Create a new instance of <see cref="FileLoadingService"/> and a <see cref="Arcanum.Core.CoreSystems.SavingSystem.Services.ISavingService"/> <br/>
/// 2. Create a new <see cref="FileDescriptor"/> with the instance of these in <see cref="DescriptorDefinitions"/><br/>
/// 3. Make sure to define all dependencies in the <see cref="FileDescriptor.Dependencies"/> <br/>
/// 4. Add it to the <see cref="DescriptorDefinitions.FileDescriptors"/> list<br/>
/// </summary>
public abstract class FileLoadingService
{
   public string Name { get; }
   private readonly Stopwatch _stopwatch = new();
   protected TimeSpan Duration => _stopwatch.Elapsed;

   protected FileLoadingService()
   {
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
      return new(descriptor, descriptor.IsMultithreadable);
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
   public virtual void LoadSingleFileWithMetrics(FileObj fileObj, FileDescriptor descriptor)
   {
      _stopwatch.Restart();

      LoadWithErrorHandling(fileObj, descriptor);

      _stopwatch.Stop();
   }

   public ReloadFileException? LoadWithErrorHandling(FileObj fileObj,
                                                     FileDescriptor descriptor,
                                                     object? lockObject = null)
   {
      var repeatLoading = true;
      while (repeatLoading)
      {
         try
         {
            LoadSingleFile(fileObj, descriptor, lockObject);
            repeatLoading = false;
         }
         catch (ReloadFileException ex)
         {
            if (ex.IsCritical)
               return ex;

            UnloadSingleFileContent(fileObj, descriptor);
         }
      }

      return null;
   }

   /// <summary>
   /// Loads a single file.
   /// Has to be thread-safe and should not be called directly but always by a manager that handles the performance measurement.
   /// </summary>
   /// <param name="fileObj"></param>
   /// <param name="descriptor"></param>
   /// <param name="lockObject"></param>
   /// <returns></returns>
   public abstract bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null);

   public abstract bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor);

   public virtual void ReloadFile(FileObj fileObj, FileDescriptor descriptor)
   {
      UnloadSingleFileContent(fileObj, descriptor);
      LoadSingleFileWithMetrics(fileObj, descriptor);
   }
}