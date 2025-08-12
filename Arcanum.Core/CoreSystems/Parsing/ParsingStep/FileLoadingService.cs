using System.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingStep;

public abstract class FileLoadingService
{
   public string Name { get; }
   private readonly Stopwatch _stopwatch = new();
   protected TimeSpan Duration => _stopwatch.Elapsed;

   protected FileLoadingService()
   {
      Name = GetType().Name;
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
   /// <returns></returns>
   public virtual bool LoadSingleFileWithMetrics(FileObj fileObj)
   {
      _stopwatch.Restart();

      var result = LoadSingleFile(fileObj);
      _stopwatch.Stop();

      return result;
   }

   /// <summary>
   /// Loads a single file.
   /// Has to be thread-safe and should not be called directly but always by a manager that handles the performance measurement.
   /// </summary>
   /// <param name="fileObj"></param>
   /// <param name="lockObject"></param>
   /// <returns></returns>
   public abstract bool LoadSingleFile(FileObj fileObj, object? lockObject = null);

   public abstract bool UnloadSingleFileContent(FileObj fileObj);

   public virtual bool ReloadFile(FileObj fileObj)
   {
      return UnloadSingleFileContent(fileObj) && LoadSingleFileWithMetrics(fileObj);
   }
}
