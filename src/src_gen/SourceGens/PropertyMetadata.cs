using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using ParserGenerator.HelperClasses;

namespace ParserGenerator;

public record PropertyMetadata
{
   public IPropertySymbol Symbol { get; }
   public string PropertyName => Symbol.Name;
   public ITypeSymbol PropertyType => Symbol.Type;
   public ITypeSymbol ItemType => PropertyType is INamedTypeSymbol { IsGenericType: true } namedType
                                     ? namedType.TypeArguments.FirstOrDefault() ?? Symbol.Type
                                     : Symbol.Type;
   public string Keyword { get; }
   public string KeywordConstantName => SanitizeToIdentifier(Keyword).ToUpper();
   public ParserSourceGenerator.NodeType AstNodeType { get; }
   public string? CustomParserMethodName { get; }
   public INamedTypeSymbol? IEu5KeyType { get; }
   public bool IsEmbedded { get; }
   public bool IsShatteredList { get; }
   public bool IsCollection { get; }
   public bool IsHashSet { get; }
   public bool Ignore { get; }
   public INamedTypeSymbol? CustomGlobalsSource { get; }

   public ParserSourceGenerator.NodeType ItemNodeType { get; }

   public PropertyMetadata(IPropertySymbol symbol, AttributeData attribute)
   {
      Symbol = symbol;

      IsShatteredList = AttributeHelper.SimpleGetAttrArgValue<bool>(attribute, 3, "isShatteredList");
      CustomParserMethodName =
         AttributeHelper.SimpleGetAttrArgValue<string?>(attribute, 2, "customParser");
      ItemNodeType =
         AttributeHelper.SimpleGetAttrArgValue(attribute, 4, "itemNodeType", ParserSourceGenerator.NodeType.KeyOnlyNode);
      IsEmbedded = AttributeHelper.SimpleGetAttrArgValue<bool>(attribute, 5, "isEmbedded");
      IEu5KeyType =
         AttributeHelper.SimpleGetAttrArgValue<INamedTypeSymbol?>(attribute, 6, "iEu5KeyType");
      CustomGlobalsSource =
         AttributeHelper.SimpleGetAttrArgValue<INamedTypeSymbol?>(attribute, 7, "customGlobalsSource");
      Ignore = AttributeHelper.SimpleGetAttrArgValue<bool>(attribute, 8, "ignore");

      IsCollection = PropertyType.AllInterfaces.Any(i => i.ToDisplayString() == "System.Collections.ICollection");
      IsHashSet = PropertyType.OriginalDefinition.ToDisplayString() ==
                  "Arcanum.Core.CoreSystems.NUI.ObservableHashSet<T>";
      if (IsHashSet)
         IsCollection = true;

      if (IsEmbedded || ((IsHashSet || IsCollection) && !IsShatteredList))
      {
         AstNodeType = ParserSourceGenerator.NodeType.BlockNode;
      }
      else
      {
         // For non-embedded types, we need to parse the enum from the attribute.
         var constructor = attribute.AttributeConstructor;
         var nodeTypeParameter = constructor?.Parameters.FirstOrDefault(p => p.Name == "nodeType");

         var astNodeTypeEnumValue = attribute.ConstructorArguments.Length > 1
                                       ? attribute.ConstructorArguments[1].Value
                                       : nodeTypeParameter?.ExplicitDefaultValue;

         if (astNodeTypeEnumValue != null)
         {
            var enumTypeSymbol = nodeTypeParameter?.Type;
            var enumMemberSymbol = enumTypeSymbol?.GetMembers()
                                                  .OfType<IFieldSymbol>()
                                                  .FirstOrDefault(f => f.ConstantValue != null &&
                                                                       f.ConstantValue.Equals(astNodeTypeEnumValue));

            AstNodeType = Enum.TryParse(enumMemberSymbol?.Name, out ParserSourceGenerator.NodeType nt) ? nt : ParserSourceGenerator.NodeType.ContentNode;
         }
      }

      Keyword = attribute.ConstructorArguments[0].Value as string ?? ToSnakeCase(PropertyName);
   }

   private static string ToSnakeCase(string text)
   {
      if (string.IsNullOrEmpty(text))
         return string.Empty;

      return string.Concat(text.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString()))
                   .ToLower();
   }

   // Simple sanitizer to create a valid C# identifier from a keyword string.
   // For example, "my-key" -> "MY_KEY"
   private static string SanitizeToIdentifier(string text)
   {
      if (string.IsNullOrEmpty(text))
         return "_";

      var sanitized = Regex.Replace(text, "[^a-zA-Z0-9_]", "_");

      if (char.IsDigit(sanitized[0]))
         sanitized = "_" + sanitized;

      return sanitized;
   }
}