using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ParserGenerator;

[Generator]
public class ParserSourceGenerator : IIncrementalGenerator
{
   private const string PARSER_FOR_ATTRIBUTE = "Arcanum.Core.CoreSystems.Parsing.ToolBox.ParserForAttribute";
   private const string PARSE_AS_ATTRIBUTE = "Arcanum.Core.CoreSystems.Parsing.ToolBox.ParseAsAttribute";
   private const string PARSING_TOOLBOX_CLASS = "Arcanum.Core.CoreSystems.Parsing.ToolBox.ParsingToolBox";

   private const string PARSE_AS_EMBEDDED_ATTRIBUTE =
      "Arcanum.Core.CoreSystems.Parsing.ToolBox.ParseAsEmbeddedAttribute";

   public void Initialize(IncrementalGeneratorInitializationContext context)
   {
      var provider = context.SyntaxProvider
                            .CreateSyntaxProvider(predicate: (node, _)
                                                     => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                                                  transform: GetParserClassSymbol)
                            .Where(s => s is not null); // Filter out classes that don't match our criteria

      context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
                                   (spc, source) => { Generate(source.Left, source.Right, spc); });
   }

   /// <summary>
   /// This "transform" step is a semantic filter. It takes the candidate classes
   /// from the predicate and checks if they *actually* have our [ParserFor] attribute.
   /// </summary>
   private static INamedTypeSymbol GetParserClassSymbol(GeneratorSyntaxContext context, CancellationToken token)
   {
      var classDeclaration = (ClassDeclarationSyntax)context.Node;
      var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration, token);

      if (classSymbol == null)
         return null!;

      foreach (var attribute in classSymbol.GetAttributes())
         if (string.Equals(attribute.AttributeClass?.ToDisplayString(),
                           PARSER_FOR_ATTRIBUTE,
                           StringComparison.Ordinal))
            return (INamedTypeSymbol)classSymbol;

      return null!;
   }

   /// <summary>
   /// This is the main method where we will eventually generate all our code.
   /// For now, it will just prove that we've found the right classes.
   /// </summary>
   private static void Generate(Compilation compilation,
                                ImmutableArray<INamedTypeSymbol> parsers,
                                SourceProductionContext context)
   {
      if (parsers.IsDefaultOrEmpty)
         return;

      var toolboxSymbol = compilation.GetTypeByMetadataName(PARSING_TOOLBOX_CLASS);
      if (toolboxSymbol == null)
      {
         ReportMissingDependency(context);
         return;
      }

      foreach (var parserSymbol in parsers.Distinct(SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>())
      {
         var attr = parserSymbol.GetAttributes()
                                .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == PARSER_FOR_ATTRIBUTE);

         if (attr?.ConstructorArguments.FirstOrDefault().Value is not INamedTypeSymbol targetTypeSymbol)
            continue;

         // --- Collect Metadata from Target Type's Properties ---
         var propertiesToParse = new List<PropertyMetadata>();
         ExtractMetadata(targetTypeSymbol, propertiesToParse);

         // If no properties are marked for parsing, there's nothing to generate
         if (propertiesToParse.Count == 0)
            continue;

         // Generate the Keywords class
         var (keywordsHintName, keywordsSource) =
            GenerateKeywordsClass(parserSymbol, targetTypeSymbol, propertiesToParse);
         context.AddSource(keywordsHintName, keywordsSource);

         // Generate the Parser class
         var (parserHintName, parserSource) = GenerateParserClass(parserSymbol,
                                                                  targetTypeSymbol,
                                                                  propertiesToParse,
                                                                  toolboxSymbol,
                                                                  $"{parserSymbol.ContainingNamespace}.{targetTypeSymbol.Name}Keywords",
                                                                  parsers,
                                                                  context);
         context.AddSource(parserHintName, parserSource);
      }
   }

   private static (string HintName, string Source) GenerateParserClass(INamedTypeSymbol parserSymbol,
                                                                       INamedTypeSymbol targetTypeSymbol,
                                                                       List<PropertyMetadata> properties,
                                                                       INamedTypeSymbol toolboxSymbol,
                                                                       string fullyQualifiedKeywordClassName,
                                                                       ImmutableArray<INamedTypeSymbol> parsers,
                                                                       SourceProductionContext context)
   {
      var hintName = $"{parserSymbol.ContainingNamespace}.{parserSymbol.Name}.g.cs";
      var targetTypeName = targetTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

      var handwrittenMethods = parserSymbol.GetMembers()
                                           .OfType<IMethodSymbol>()
                                           .Select(m => m.Name)
                                           .ToImmutableHashSet();

      // Group properties by the AST node they parse from
      var contentNodeProps = properties.Where(p => p.AstNodeType == "ContentNode").ToList();
      var embeddedBlockProps = properties.Where(p => p.IsEmbedded).ToList();
      var standardBlockProps = properties.Where(p => !p.IsEmbedded && p.AstNodeType == "BlockNode").ToList();

      var sb = new StringBuilder();
      sb.AppendLine("// <auto-generated/>");
      sb.AppendLine($"namespace {parserSymbol.ContainingNamespace};");
      sb.AppendLine();
      // --- Usings ---
      GenerateUsings(targetTypeSymbol, sb, toolboxSymbol);

      sb.AppendLine($"public partial class {parserSymbol.Name}");
      sb.AppendLine("{");

      // --- Dictionaries ---
      GenerateParserDictionaries(fullyQualifiedKeywordClassName,
                                 sb,
                                 targetTypeName,
                                 contentNodeProps,
                                 standardBlockProps,
                                 embeddedBlockProps);

      // --- ParseProperties Method ---
      ParsePropertiesFromBlockNode(targetTypeSymbol, sb, targetTypeName);

      // --- Wrapper Methods (partial signatures) ---
      GenerateParserMethodSignatures(properties, sb, targetTypeName);

      // --- Generated Wrapper Methods ---
      GenerateAutoImplementedParsers(parserSymbol,
                                     properties,
                                     toolboxSymbol,
                                     sb,
                                     handwrittenMethods,
                                     targetTypeName,
                                     parsers,
                                     context);

      return (hintName, sb.ToString());
   }

   #region Generator Helpers

   private static void GenerateParserDictionaries(string fullyQualifiedKeywordClassName,
                                                  StringBuilder sb,
                                                  string targetTypeName,
                                                  List<PropertyMetadata> contentNodeProps,
                                                  List<PropertyMetadata> blockNodeProps,
                                                  List<PropertyMetadata> embeddedBlockNodeProps)
   {
      sb.AppendLine($"    private static readonly Dictionary<string, Pdh.ContentParser<{targetTypeName}>> _contentParsers = new()");
      sb.AppendLine("    {");

      foreach (var prop in contentNodeProps)
         AppendParserMethodMapping(fullyQualifiedKeywordClassName, sb, prop);

      sb.AppendLine("    };");
      sb.AppendLine();
      sb.AppendLine($"    private static readonly Dictionary<string, Pdh.BlockParser<{targetTypeName}>> _blockParsers = new()");
      sb.AppendLine("    {");

      foreach (var prop in blockNodeProps)
         AppendParserMethodMapping(fullyQualifiedKeywordClassName, sb, prop);
      foreach (var prop in embeddedBlockNodeProps)
         sb.AppendLine($"        {{ {fullyQualifiedKeywordClassName}.{prop.KeywordConstantName}, {PropCustomParserMethodName(prop)} }},");

      sb.AppendLine("    };");
      sb.AppendLine();
   }

   private static void AppendParserMethodMapping(string fullyQualifiedKeywordClassName,
                                                 StringBuilder sb,
                                                 PropertyMetadata prop)
   {
      sb.AppendLine($"        {{ {fullyQualifiedKeywordClassName}.{prop.KeywordConstantName}, {PropCustomParserMethodName(prop)} }},");
   }

   private static string PropCustomParserMethodName(PropertyMetadata prop)
      => prop.CustomParserMethodName ?? $"{ARC_PARSE_PREFIX}{FlagsPrefix(prop)}{prop.PropertyName}";

   private static string FlagsPrefix(PropertyMetadata prop) => IsFlagsEnum(prop) ? "Flags" : string.Empty;

   private static bool IsFlagsEnum(PropertyMetadata prop)
   {
      var isFlagsEnum = prop.PropertyType.GetAttributes()
                            .Any(attr => attr.AttributeClass?.ToDisplayString() == "System.FlagsAttribute");
      return isFlagsEnum;
   }

   private static void GenerateUsings(INamedTypeSymbol targetTypeSymbol,
                                      StringBuilder sb,
                                      INamedTypeSymbol toolboxSymbol)
   {
      sb.AppendLine("using System.Collections.Generic;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.Common;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.Parsing.ToolBox;");
      sb.AppendLine("using System.Collections.ObjectModel;");
      sb.AppendLine($"using {targetTypeSymbol.ContainingNamespace.ToDisplayString()};");
      sb.AppendLine($"using static {toolboxSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)};");
      sb.AppendLine();
   }

   private static void GenerateAutoImplementedParsers(INamedTypeSymbol parserSymbol,
                                                      List<PropertyMetadata> properties,
                                                      INamedTypeSymbol toolboxSymbol,
                                                      StringBuilder sb,
                                                      ImmutableHashSet<string> handwrittenMethods,
                                                      string targetTypeName,
                                                      ImmutableArray<INamedTypeSymbol> parsers,
                                                      SourceProductionContext context)
   {
      sb.AppendLine("    #region Auto-Implemented Parsers");
      foreach (var prop in properties)
      {
         if (prop.IsEmbedded)
         {
            GenerateEmbeddedPropertyParser(parserSymbol, sb, targetTypeName, parsers, context, prop);
            continue;
         }

         // If a custom parser is specified, we DO NOT generate an implementation.
         if (GenerateToolMethodCall(parserSymbol,
                                    toolboxSymbol,
                                    prop,
                                    handwrittenMethods,
                                    out var wrapperMethodName,
                                    out var propTypeName,
                                    out var actionName,
                                    out var toolMethodCall))
            continue;

         sb.AppendLine($"    private static partial bool {wrapperMethodName}({prop.AstNodeType} node, {targetTypeName} target, LocationContext ctx, string source, ref bool validation)");
         sb.AppendLine("    {");
         sb.AppendLine($"        if ({toolMethodCall}(node, ctx, {actionName}, source, out {propTypeName} value, ref validation))");
         sb.AppendLine("        {");
         sb.AppendLine(IsFlagsEnum(prop)
                          ? $"            target.{prop.PropertyName} |= value;"
                          : $"            target.{prop.PropertyName} = value;");
         sb.AppendLine("            return true;");
         sb.AppendLine("        }");
         sb.AppendLine("        return false;");
         sb.AppendLine("    }");
         sb.AppendLine();
      }

      sb.AppendLine("    #endregion");
      sb.AppendLine("}");
   }

   private static void GenerateEmbeddedPropertyParser(INamedTypeSymbol parserSymbol,
                                                      StringBuilder sb,
                                                      string targetTypeName,
                                                      ImmutableArray<INamedTypeSymbol> parsers,
                                                      SourceProductionContext context,
                                                      PropertyMetadata prop)
   {
      var propTypeName2 = prop.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

      var nestedParserSymbol = FindParserForType(prop.PropertyType, parsers);

      if (nestedParserSymbol == null)
      {
         context.ReportDiagnostic(Diagnostic.Create(new(id: "PG002",
                                                        title: "Missing Parser for Embedded Type",
                                                        messageFormat:
                                                        $"No parser with [ParserFor(typeof({propTypeName2}))] was found for embedded property '{prop.PropertyName}' in parser '{parserSymbol.Name}'.",
                                                        category: "SourceGens",
                                                        DiagnosticSeverity.Error,
                                                        isEnabledByDefault: true),
                                                    Location.None));

         sb.AppendLine($"    // ERROR: No parser with [ParserFor(typeof({propTypeName2}))] was found for embedded property '{prop.PropertyName}'.");
         return;
      }

      var nestedParserName = nestedParserSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

      sb.AppendLine($"    private static partial bool {PropCustomParserMethodName(prop)}(BlockNode node, {targetTypeName} target, LocationContext ctx, string source, ref bool validation)");
      sb.AppendLine("    {");
      sb.AppendLine("        // This property is an embedded object. Initialize if null.");
      sb.AppendLine($"        if (target.{prop.PropertyName} == null) ");
      sb.AppendLine($"            target.{prop.PropertyName} = new {propTypeName2}();");
      sb.AppendLine();
      sb.AppendLine("        // Call the generated ParseProperties method of the nested object's parser.");
      sb.AppendLine($"        {nestedParserName}.ParseProperties(node, target.{prop.PropertyName}, ctx, source, ref validation);");
      sb.AppendLine("        return true;"); // The inner call handles validation.
      sb.AppendLine("    }");
      sb.AppendLine();
   }

   private static bool GenerateToolMethodCall(INamedTypeSymbol parserSymbol,
                                              INamedTypeSymbol toolboxSymbol,
                                              PropertyMetadata prop,
                                              ImmutableHashSet<string> handwrittenMethods,
                                              out string wrapperMethodName,
                                              out string propTypeName,
                                              out string actionName,
                                              out string toolMethodCall)
   {
      if (prop.CustomParserMethodName != null)
      {
         wrapperMethodName = null!;
         propTypeName = null!;
         actionName = null!;
         toolMethodCall = null!;
         return true;
      }

      wrapperMethodName = PropCustomParserMethodName(prop);

      // If the user has provided their own implementation, skip generation.
      if (handwrittenMethods.Contains(wrapperMethodName))
      {
         propTypeName = null!;
         actionName = null!;
         toolMethodCall = null!;
         return true;
      }

      var toolMethod = FindMatchingTool(toolboxSymbol, prop.AstNodeType, prop.PropertyType);
      if (toolMethod == null)
      {
         propTypeName = null!;
         actionName = null!;
         toolMethodCall = null!;
         return true;
      }

      propTypeName = prop.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
      actionName = $"\"{parserSymbol.Name}.{wrapperMethodName}\"";

      // For a generic tool, we need to specify the type argument, e.g., "ArcTryParse_Enum<MyEnum>"
      toolMethodCall = toolMethod.IsGenericMethod ? $"{toolMethod.Name}<{propTypeName}>" : toolMethod.Name;
      return false;
   }

   private static INamedTypeSymbol? FindParserForType(
      ITypeSymbol targetType,
      ImmutableArray<INamedTypeSymbol> allKnownParsers)
   {
      foreach (var potentialParser in allKnownParsers)
      {
         var attr = potentialParser.GetAttributes()
                                   .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == PARSER_FOR_ATTRIBUTE);

         if (attr?.ConstructorArguments.FirstOrDefault().Value is INamedTypeSymbol attrTargetType)
         {
            if (SymbolEqualityComparer.Default.Equals(attrTargetType, targetType))
               // We found it! This is the parser class for our target type.
               return potentialParser;
         }
      }

      // No parser was found in the compilation for the given target type.
      return null;
   }

   private static void GenerateParserMethodSignatures(List<PropertyMetadata> properties,
                                                      StringBuilder sb,
                                                      string targetTypeName)
   {
      sb.AppendLine("    #region Parser Method Signatures");
      foreach (var prop in properties)
      {
         // If a custom parser is specified, we DO NOT generate a signature for it.
         if (prop.CustomParserMethodName != null)
         {
            sb.AppendLine($"    // Property '{prop.PropertyName}' is handled by custom parser '{prop.CustomParserMethodName}'.");
            continue;
         }

         sb.AppendLine($"    private static partial bool {PropCustomParserMethodName(prop)}({prop.AstNodeType} node, {targetTypeName} target, LocationContext ctx, string source, ref bool validation);");
      }

      sb.AppendLine("    #endregion");
      sb.AppendLine();
   }

   private static void ParsePropertiesFromBlockNode(INamedTypeSymbol targetTypeSymbol,
                                                    StringBuilder sb,
                                                    string targetTypeName)
   {
      sb.AppendLine("    /// <summary>");
      sb.AppendLine("    /// Parses all properties of a " +
                    targetTypeSymbol.Name +
                    " object marked with [ParseAs] from a BlockNode.");
      sb.AppendLine("    /// This is a facade that calls the centralized logic in the Pdh helper class.");
      sb.AppendLine("    /// </summary>");
      sb.AppendLine("    /// <returns>A list of all child nodes that were not handled automatically.</returns>");
      sb.AppendLine($"    public static List<StatementNode> ParseProperties(BlockNode block, {targetTypeName} target, LocationContext ctx, string source, ref bool validation)");
      sb.AppendLine("    {");
      sb.AppendLine("        // Delegate the entire implementation to the Pdh helper.");
      sb.AppendLine("        return Pdh.ParseProperties(block, target, ctx, source, ref validation, _contentParsers, _blockParsers);");
      sb.AppendLine("    }");
      sb.AppendLine();
   }

   #endregion

   public const string ARC_PARSE_PREFIX = "ArcParse_";
   private const string TOOL_METHOD_PREFIX = "ArcTryParse";

   private static IMethodSymbol? FindMatchingTool(INamedTypeSymbol toolboxSymbol,
                                                  string astNodeType,
                                                  ITypeSymbol propertyType)
   {
      if (propertyType.BaseType != null && propertyType.BaseType.ToDisplayString() == "System.Enum")
      {
         var isFlagsEnum = propertyType.GetAttributes()
                                       .Any(attr => attr.AttributeClass?.ToDisplayString() == "System.FlagsAttribute");

         var genericToolName = isFlagsEnum
                                  ? $"{TOOL_METHOD_PREFIX}_FlagsEnum"
                                  : $"{TOOL_METHOD_PREFIX}_Enum";

         var methods = toolboxSymbol.GetMembers(genericToolName).OfType<IMethodSymbol>();

         foreach (var member in methods)
         {
            // A valid tool must be static, generic, and have the right number of parameters.
            if (!member.IsStatic || !member.IsGenericMethod || member.Parameters.Length < 5)
               continue;

            if (member.Parameters[0].Type.Name != astNodeType)
               continue;

            var outParam = member.Parameters.FirstOrDefault(p => p.RefKind == RefKind.Out);
            if (outParam == null || outParam.Type.TypeKind != TypeKind.TypeParameter)
               continue;

            return member;
         }

         return null;
      }

      var expectedToolName = GetExpectedToolName(propertyType, TOOL_METHOD_PREFIX);
      return FindToolByName(toolboxSymbol, expectedToolName, astNodeType, propertyType);
   }

   // Extracted the search logic to a reusable method
   private static IMethodSymbol? FindToolByName(INamedTypeSymbol toolboxSymbol,
                                                string toolName,
                                                string astNodeType,
                                                ITypeSymbol propertyType)
   {
      foreach (var member in toolboxSymbol.GetMembers(toolName).OfType<IMethodSymbol>())
      {
         if (!member.IsStatic || member.Parameters.Length < 4)
            continue;

         if (member.Parameters[0].Type.Name == astNodeType)
         {
            var outParam = member.Parameters.LastOrDefault(p => p.RefKind == RefKind.Out);
            if (outParam != null && SymbolEqualityComparer.Default.Equals(outParam.Type, propertyType))
               return member;
         }
      }

      return null;
   }

   private static string GetExpectedToolName(ITypeSymbol type, string prefix)
   {
      if (type is INamedTypeSymbol { IsGenericType: true } namedType)
      {
         var genericTypeName = namedType.Name;
         var itemTypeName = namedType.TypeArguments.FirstOrDefault()?.Name ?? "Object";
         return $"{prefix}_{genericTypeName}{itemTypeName}";
      }

      return $"{prefix}_{type.Name}";
   }

   private static (string HintName, string Source) GenerateKeywordsClass(
      INamedTypeSymbol parserSymbol,
      INamedTypeSymbol targetTypeSymbol,
      List<PropertyMetadata> properties)
   {
      var className = $"{targetTypeSymbol.Name}Keywords";
      var hintName = $"{parserSymbol.ContainingNamespace}.{className}.g.cs";

      var sb = new StringBuilder();
      sb.AppendLine("// <auto-generated/>");
      sb.AppendLine($"namespace {parserSymbol.ContainingNamespace};");
      sb.AppendLine();
      sb.AppendLine($"public static class {className}");
      sb.AppendLine("{");

      // Use a HashSet to ensure we only generate one const per unique keyword string
      var uniqueKeywords = new HashSet<string>();
      foreach (var prop in properties)
         if (uniqueKeywords.Add(prop.Keyword))
            sb.AppendLine($"    public const string {prop.KeywordConstantName} = \"{prop.Keyword}\";");

      sb.AppendLine("}");

      return (hintName, sb.ToString());
   }

   private record PropertyMetadata
   {
      public IPropertySymbol Symbol { get; }
      public AttributeData Attribute { get; }
      public string PropertyName => Symbol.Name;
      public ITypeSymbol PropertyType => Symbol.Type;
      public string Keyword { get; }
      public string KeywordConstantName => SanitizeToIdentifier(Keyword).ToUpper();
      public string AstNodeType { get; }
      public string? CustomParserMethodName { get; }
      public bool IsEmbedded { get; }

      public PropertyMetadata(IPropertySymbol symbol, AttributeData attribute, bool isEmbedded)
      {
         IsEmbedded = isEmbedded;
         Symbol = symbol;
         Attribute = attribute;

         CustomParserMethodName = attribute.NamedArguments
                                           .FirstOrDefault(arg => arg.Key == "CustomParser")
                                           .Value.Value as string;

         if (isEmbedded)
         {
            // For embedded types, the rule is simple.
            AstNodeType = "BlockNode";
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

               AstNodeType = enumMemberSymbol?.Name ?? "ERROR_EnumMemberNotFound";
            }
            else
            {
               AstNodeType = "ERROR_CouldNotDetermineNodeType";
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

   private static void ExtractMetadata(INamedTypeSymbol targetTypeSymbol,
                                       List<PropertyMetadata> propertiesToParse)
   {
      foreach (var member in targetTypeSymbol.GetMembers().OfType<IPropertySymbol>())
      {
         // Check for [ParseAs] first
         var parseAsAttr = member.GetAttributes()
                                 .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == PARSE_AS_ATTRIBUTE);
         if (parseAsAttr != null)
         {
            propertiesToParse.Add(new(member, parseAsAttr, isEmbedded: false));
            continue;
         }

         // Then check for [ParseAsEmbedded]
         var parseAsEmbeddedAttr = member.GetAttributes()
                                         .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                               PARSE_AS_EMBEDDED_ATTRIBUTE);
         if (parseAsEmbeddedAttr != null)
            propertiesToParse.Add(new(member, parseAsEmbeddedAttr, isEmbedded: true));
      }
   }

   private static void ReportMissingDependency(SourceProductionContext context)
   {
      // Report a diagnostic that the ParsingToolBox class is missing
      context.ReportDiagnostic(Diagnostic.Create(new(id: "PARSERGEN001",
                                                     title: "Missing Dependency",
                                                     messageFormat:
                                                     $"The required class '{PARSING_TOOLBOX_CLASS}' is not found. Ensure the necessary assembly is referenced.",
                                                     category: "SourceGens",
                                                     DiagnosticSeverity.Warning,
                                                     isEnabledByDefault: true),
                                                 Location.None));
   }
}