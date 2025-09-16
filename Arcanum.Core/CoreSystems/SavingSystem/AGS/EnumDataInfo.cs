namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

/// <summary>
/// Contains all necessary metadata for serializing an enum type to AGS format.
/// </summary>
public class EnumDataInfo
{
   /// <summary>
   /// The enum type this metadata corresponds to.
   /// </summary>
   public required Type EnumType { get; init; }
   /// <summary>
   /// Whether the enum is treated as a flags enum (i.e., can represent a combination of values).
   /// </summary>
   public required bool IsFlags { get; init; }
   /// <summary>
   /// A mapping of enum value names to their corresponding AGS string representations.
   /// </summary>
   public required Dictionary<string, string> Mapping { get; init; }
}