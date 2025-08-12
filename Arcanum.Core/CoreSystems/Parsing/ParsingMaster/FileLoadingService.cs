using System.Diagnostics;
using System.Reflection;
using Arcanum.Core.CoreSystems.Parsing.ParsingStep;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public abstract class FileLoadingService
{
   public string Name { get; }
   private readonly Stopwatch _stopwatch = new();
   protected TimeSpan Duration => _stopwatch.Elapsed;

   protected FileLoadingService()
   {
      Name = GetType().Name;
   }

   protected string GetActionName([System.Runtime.CompilerServices.CallerMemberName] string caller = "")
   {
      var declaringType = GetType();
      return $"{declaringType.FullName}.{caller}";
   }

   
   public virtual DefaultParsingStep GetParsingStep(FileDescriptor descriptor)
   {
      return new (descriptor, descriptor.IsMultithreadable);
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
   public virtual bool LoadSingleFileWithMetrics(FileObj fileObj, FileDescriptor descriptor)
   {
      _stopwatch.Restart();

      var result = LoadSingleFile(fileObj, descriptor);
      _stopwatch.Stop();

      return result;
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

   public virtual bool ReloadFile(FileObj fileObj, FileDescriptor descriptor)
   {
      return UnloadSingleFileContent(fileObj, descriptor) && LoadSingleFileWithMetrics(fileObj, descriptor);
   }
}
