using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.Core.Utils.Parsing.ParsingStep;

public interface IParsingStep
{
   /// <summary>
   /// Contains a list of diagnostics that occurred during the parsing step.
   /// Will automatically be merged into the global diagnostics list.
   /// </summary>
   public List<Diagnostic> Diagnostics { get; }
   
   /// <summary>
   /// Returns the duration of the parsing step.
   /// </summary>
   public TimeSpan Duration { get; }
   
   /// <summary>
   /// An estimated remaining time for the parsing step, if applicable.
   /// This is calculated based on the progress history and may return null if not enough data is available.
   /// </summary>
   public TimeSpan? EstimatedRemaining { get; }
   
   /// <summary>
   /// Indicates whether the parsing step was successful.
   /// </summary>
   public bool IsSuccessful { get; }
   
   /// <summary>
   /// The name of the parsing step.
   /// </summary>
   public string Name { get; }
   
   /// <summary>
   /// Progress of the parsing step, if applicable.
   /// Returns the percentage completed and the number of steps done.
   /// </summary>
   public EventHandler<ParsingStepBase>? SubStepCompleted { get; set; }
   
   /// <summary>
   /// Executes the parsing step.
   /// </summary>
   /// <param name="cancellationToken"></param>
   /// <returns></returns>
   public bool Execute(CancellationToken cancellationToken = default);
   
}