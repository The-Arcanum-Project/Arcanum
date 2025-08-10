using System.Diagnostics;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.Utils.Parsing.ParsingStep;

public abstract class SingleFileLoadingBase
{
   private readonly Stopwatch _stopwatch = new();
   protected TimeSpan Duration => _stopwatch.Elapsed;
   public static SingleFileLoadingBase Dummy { get; } = new DummySingleFileLoading();

   /// <summary>
   /// Executes the file loading and measures the time taken
   /// The time can be extracted by using the <see cref="Duration"/> property.
   /// This method is intended to be used for performance measurement and logging and has to be thread-safe.
   /// Multiple instances of this will be called in parallel
   /// </summary>
   /// <param name="fileObj"></param>
   /// <returns></returns>
   public virtual bool LoadFileWithMetrics(FileObj fileObj)
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
}

public class DummySingleFileLoading : SingleFileLoadingBase
{
   public override bool LoadSingleFile(FileObj fileObj, object? lockObject = null) => true;
}