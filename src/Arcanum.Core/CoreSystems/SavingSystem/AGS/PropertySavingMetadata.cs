using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

// ReSharper disable PossibleMultipleEnumeration

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public class PropertySavingMetadata
{
   /// <summary>
   /// The keyword to use when saving this property.
   /// </summary>
   public required string Keyword { get; init; }
   /// <summary>
   /// The separator to use between the keyword and the value.
   /// </summary>
   public TokenType Separator { get; init; } = TokenType.Equals;
   /// <summary>
   /// The Nexus property this metadata is for.
   /// </summary>
   public required Enum NxProp { get; init; }
   /// <summary>
   /// The type of value this property represents. <br/>
   /// If set to Auto, the type will be determined at runtime based on the actual value.
   /// </summary>
   public SavingValueType ValueType { get; set; }
   /// <summary>
   /// The default value for this property. <br/>
   /// Used to determine if the property should be saved or omitted (if it matches the default value).
   /// </summary>
   public required object? DefaultValue { get; set; }
   /// <summary>
   /// A delegate to provide a comment for this property when saving. <br/>
   /// If null, no comment will be added.
   /// </summary>
   public required AgsDelegates.AgsCommentProvider? CommentProvider { get; set; }
   /// <summary>
   /// A delegate to provide a custom saving method for this property. <br/>
   /// If null, the property will be saved using the default method based on its type.
   /// </summary>
   public required AgsDelegates.AgsSavingAction? SavingMethod { get; set; }
   /// <summary>
   /// A delegate to provide a key for each item in a collection when saving. <br/>
   /// If null, the items will be saved using their value representation.
   /// </summary>
   public required AgsDelegates.GetCollectionItemKey? CollectionItemKeyProvider { get; set; }
   /// <summary>
   /// Indicates whether this property is a collection (e.g., List, Array). <br/>
   /// If true, the property will be handled as a collection during saving.
   /// </summary>
   public required bool IsCollection { get; init; }
   public required bool CollectionAsPureIdentifierList { get; init; }
   public required bool IsEmbeddedObject { get; init; }

   /// <summary>
   /// The separator to use between items in a collection when saving. <br/>
   /// Only relevant if IsCollection is true. Default is "".
   /// </summary>
   public required string CollectionSeparator { get; init; } = "";
   public required bool SaveEmbeddedAsIdentifier { get; init; } = true;
   /// <summary>
   /// If this list is shattered into multiple parts when saved. <br/>
   /// Only relevant if IsCollection is true. Default is false.
   /// </summary>
   public required bool IsShattered { get; init; }
   /// <summary>
   /// Number of decimal places to use when saving float or double values. <br/>
   /// Default is 2.
   /// </summary>
   public required int NumOfDecimalPlaces { get; init; }

   /// <summary>
   /// The property is always serialized
   /// </summary>
   public required bool AlwaysWrite { get; init; }
   public required bool IsArray { get; init; }

   public required Func<object, bool>? MustNotBeWritten { get; init; }

   #region Equality operations

   public override string ToString() => $"{NxProp} as {Keyword} ({ValueType})";

   public override int GetHashCode()
   {
      return NxProp.GetHashCode();
   }

   protected bool Equals(PropertySavingMetadata other) => NxProp.Equals(other.NxProp);

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;
      if (obj.GetType() != GetType())
         return false;

      return Equals((PropertySavingMetadata)obj);
   }

   #endregion
}