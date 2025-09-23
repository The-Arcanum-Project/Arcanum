using Microsoft.CodeAnalysis;

namespace ParserGenerator.SubClasses;

public class SaveAsMetadata
{
   private const string SAVING_COMMENT_PROVIDER = "Arcanum.Core.CoreSystems.SavingSystem.AGS.SavingCommentProvider";
   private const string CUSTOM_SAVING_PROVIDER = "Arcanum.Core.CoreSystems.SavingSystem.AGS.SavingActionProvider";
   private const string CUSTOM_ITEM_KEY_PROVIDER = "Arcanum.Core.CoreSystems.SavingSystem.AGS.CustomItemKeyProvider";
   private const string DEFAULT_VALUE_ATTRIBUTE = "System.ComponentModel.DefaultValueAttribute";

   public SaveAsMetadata(IPropertySymbol property, AttributeData saveAs)
   {
      Prop = property;
      var args = saveAs.ConstructorArguments;

      // CHANGE 2: Each argument is now accessed safely, with a check on the array length.
      // This prevents the IndexOutOfRangeException that was crashing your generator.

      // Argument 0: ValueType (enum)
      ValueType = args.Length > 0
                     ? (Helpers.GetEnumMemberName(args[0]) ?? "SavingValueType.Auto")
                     : "SavingValueType.Auto";

      // Argument 1: Separator (enum)
      Separator = args.Length > 1 ? (Helpers.GetEnumMemberName(args[1]) ?? "TokenType.Equals") : "TokenType.Equals";

      // Argument 2: SavingMethod (string)
      SavingMethod = args.Length > 2 ? GetProviderString(args[2], CUSTOM_SAVING_PROVIDER) : null;

      // Argument 3: CommentMethod (string)
      CommentMethod = args.Length > 3 ? GetProviderString(args[3], SAVING_COMMENT_PROVIDER) : null;

      // Argument 4: CollectionKeyMethod (string)
      CollectionKeyMethod = args.Length > 4 ? GetProviderString(args[4], CUSTOM_ITEM_KEY_PROVIDER) : null;

      // Argument 5: IsCollection (bool)
      // Default is determined by inspecting the property type.
      var defaultIsCollection = IsPropertySymbolACollection(Prop);
      IsCollection = defaultIsCollection;

      // Argument 6: CollectionSeparator (string)
      CollectionSeparator = args.Length > 6 ? $"\"{args[6].Value}\"" as string : "\" \"";

      // Argument 7: SaveEmbeddedAsIdentifier (bool)
      SaveEmbeddedAsIdentifier = args.Length <= 7 || ((bool?)args[7].Value ?? true);
      CollectionAsPureIdentifierList = args.Length <= 8 || ((bool?)args[8].Value ?? false);
      IsEmbeddedObject = args.Length > 9 && ((bool?)args[9].Value ?? false);

      // This part was correct, as it operates on the now-reliable Prop symbol.
      DefaultValueAttribute = Prop.GetAttributes()
                                  .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                        DEFAULT_VALUE_ATTRIBUTE);
   }

   public IPropertySymbol Prop { get; }
   public string ValueType { get; }
   public string Separator { get; }
   public string? SavingMethod { get; } = null;
   public string? CommentMethod { get; } = null;
   public string? CollectionKeyMethod { get; } = null;
   public bool IsCollection { get; } = false;
   public string? CollectionSeparator { get; }
   public bool SaveEmbeddedAsIdentifier { get; }
   public bool CollectionAsPureIdentifierList { get; }
   public bool IsEmbeddedObject { get; }
   public AttributeData? DefaultValueAttribute { get; set; } = null;

   private static string GetProviderString(TypedConstant value, string providerName)
   {
      if (value.Value is string stringValue && !string.IsNullOrEmpty(stringValue))
         return $"{providerName}.{stringValue}";

      return "null";
   }

   public static bool IsPropertySymbolACollection(IPropertySymbol? propertySymbol)
   {
      if (propertySymbol == null)
         return false;

      if (propertySymbol.Type.SpecialType == SpecialType.System_String)
         return false;

      var type = propertySymbol.Type;

      // Check if the type is an array
      if (type.TypeKind == TypeKind.Array)
         return true;

      // Check if the type implements IEnumerable<T> or IEnumerable
      foreach (var @interface in type.AllInterfaces)
         if (@interface.SpecialType == SpecialType.System_Collections_IEnumerable ||
             @interface.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>")
            return true;

      return false;
   }
}