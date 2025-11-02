namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

/// <summary>
/// This enum overrides other properties so use with caution.
/// </summary>
public enum SavingFormat
{
   /// <summary>
   /// No empty lines between properties
   /// </summary>
   Compact,

   /// <summary>
   /// One empty line between properties 
   /// </summary>
   Default,

   /// <summary>
   /// Two empty lines between properties and one after the opening brace and before the closing brace.
   /// </summary>
   Spacious,
}