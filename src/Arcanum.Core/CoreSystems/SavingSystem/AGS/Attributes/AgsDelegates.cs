using Arcanum.Core.CoreSystems.Common;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

public static class AgsDelegates
{
   /// <summary>
   /// Represents a method that provides a comment string for a given property.
   /// </summary>
   /// <param name="target">The IAgs object instance being saved.</param>
   /// <param name="commentChar">The character(s) used to denote comments in the AGS format (e.g., "#").</param>
   /// <param name="sb">An <see cref="IndentedStringBuilder"/></param>
   /// <returns>The comment string to be written, or null/empty if no comment should be added.</returns>
   public delegate string? AgsCommentProvider(IAgs target, string commentChar, IndentedStringBuilder sb);

   /// <summary>
   /// Represents a method that performs the complete saving logic for a property.
   /// </summary>
   /// <param name="target">The IAgs object instance being saved.</param>
   /// <param name="metadata">The pre-compiled metadata for the property being saved.</param>
   /// <param name="sb">An <see cref="IndentedStringBuilder"/></param>
   public delegate void AgsSavingAction(IAgs target, PropertySavingMetadata metadata, IndentedStringBuilder sb);

   /// <summary>
   /// Represents a method that generates a unique key for an item in a collection.
   /// </summary>
   public delegate string GetCollectionItemKey(object item);
}