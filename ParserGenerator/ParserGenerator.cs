using System;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ParserGenerator;

[Generator]
public class ParserSourceGenerator : IIncrementalGenerator
{
   // The fully qualified name of the attribute we are looking for.
   // This is the "entry point" for our generator.
   private const string PARSER_FOR_ATTRIBUTE = "Arcanum.Core.CoreSystems.Parsing.ToolBox.ParserForAttribute";
   private const string PARSE_AS_ATTRIBUTE = "Arcanum.Core.CoreSystems.Parsing.ToolBox.ParseAsAttribute";
   private const string PARSING_TOOLBOX_CLASS = "Arcanum.Core.CoreSystems.Parsing.ToolBox.ParsingToolBox";

   public void Initialize(IncrementalGeneratorInitializationContext context)
   {
      // --- Step 1: The Provider Pipeline ---
      // This is the modern, efficient way to set up a source generator.

      // 1a. Find all classes that *could* be our parsers. We are looking for
      //     any class that has at least one attribute. This is a fast syntax-only filter.
      var provider = context.SyntaxProvider
                            .CreateSyntaxProvider(predicate: (node, _)
                                                     => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                                                  transform: GetParserClassSymbol)
                            .Where(s => s is not null); // Filter out classes that don't match our criteria

      // 1b. Collect all the found symbols into a list for the final generation step.
      var compilation = context.CompilationProvider.Combine(provider.Collect());

      // 1c. Register the final "Execute" step.
      context.RegisterSourceOutput(compilation, (spc, source) => { Generate(source.Left, source.Right, spc); });
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
                                System.Collections.Immutable.ImmutableArray<INamedTypeSymbol> parsers,
                                SourceProductionContext context)
   {
      if (parsers.IsDefaultOrEmpty)
         return;

      var toolboxSymbol = compilation.GetTypeByMetadataName(PARSING_TOOLBOX_CLASS);
      if (toolboxSymbol == null)
      {
         // Report a diagnostic that the ParsingToolBox class is missing
         context.ReportDiagnostic(Diagnostic.Create(new(id: "PARSERGEN001",
                                                        title: "Missing Dependency",
                                                        messageFormat:
                                                        $"The required class '{PARSING_TOOLBOX_CLASS}' is not found. Ensure the necessary assembly is referenced.",
                                                        category: "ParserGenerator",
                                                        DiagnosticSeverity.Warning,
                                                        isEnabledByDefault: true),
                                                    Location.None));
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
         foreach (var member in targetTypeSymbol.GetMembers().OfType<IPropertySymbol>())
         {
            // Find the [ParseAs] attribute on the property
            var parseAsAttr = member.GetAttributes()
                                    .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == PARSE_AS_ATTRIBUTE);
            if (parseAsAttr != null)
               // If found, create a metadata object and add it to our list
               propertiesToParse.Add(new(member, parseAsAttr));
         }

         // If no properties are marked for parsing, there's nothing to generate
         if (!propertiesToParse.Any())
            continue;

         // 1. Generate the Keywords class
         var (keywordsHintName, keywordsSource) =
            GenerateKeywordsClass(parserSymbol, targetTypeSymbol, propertiesToParse);
         context.AddSource(keywordsHintName, keywordsSource);

         // 2. Generate a placeholder for the parser class (to be replaced in the next step)
         var (parserHintName, parserSource) = GenerateParserClass(parserSymbol,
                                                                  targetTypeSymbol,
                                                                  propertiesToParse,
                                                                  toolboxSymbol,
                                                                  $"{parserSymbol.ContainingNamespace}.{targetTypeSymbol.Name}Keywords");
         context.AddSource(parserHintName, parserSource);
      }
   }

   private static (string HintName, string Source) GenerateParserClass(
      INamedTypeSymbol parserSymbol,
      INamedTypeSymbol targetTypeSymbol,
      List<PropertyMetadata> properties,
      INamedTypeSymbol toolboxSymbol,
      string fullyQualifiedKeywordClassName)
   {
      const string arcParsePrefix = "ArcParse_";
      var hintName = $"{parserSymbol.ContainingNamespace}.{parserSymbol.Name}.g.cs";
      var targetTypeName = targetTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

      var handwrittenMethods = parserSymbol.GetMembers()
                                           .OfType<IMethodSymbol>()
                                           .Select(m => m.Name)
                                           .ToImmutableHashSet();

      // Group properties by the AST node they parse from
      var contentNodeProps = properties.Where(p => p.AstNodeType == "ContentNode").ToList();
      var blockNodeProps = properties.Where(p => p.AstNodeType == "BlockNode").ToList();

      var sb = new StringBuilder();
      sb.AppendLine("// <auto-generated/>");
      sb.AppendLine($"namespace {parserSymbol.ContainingNamespace};");
      sb.AppendLine();

      // --- MODIFICATION 2: Add using statements ---
      sb.AppendLine("using System.Collections.Generic;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.Parsing.CeasarParser;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.Common;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.Parsing.ToolBox;");
      sb.AppendLine("using System.Collections.ObjectModel;");
      sb.AppendLine($"using {targetTypeSymbol.ContainingNamespace.ToDisplayString()};");

      // This makes the calls to the toolbox methods clean.
      sb.AppendLine($"using static {toolboxSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)};");
      sb.AppendLine();

      sb.AppendLine($"public partial class {parserSymbol.Name}");
      sb.AppendLine("{");

      // --- Dictionaries ---
      sb.AppendLine($"    private static readonly Dictionary<string, Pdh.ContentParser<{targetTypeName}>> _contentParsers = new()");
      sb.AppendLine("    {");
      // MODIFIED: Use the consistent name for the dictionary value
      foreach (var prop in contentNodeProps)
         sb.AppendLine($"        {{ {fullyQualifiedKeywordClassName}.{prop.KeywordConstantName}, {arcParsePrefix}{prop.PropertyName} }},");

      sb.AppendLine("    };");
      sb.AppendLine();
      sb.AppendLine($"    private static readonly Dictionary<string, Pdh.BlockParser<{targetTypeName}>> _blockParsers = new()");
      sb.AppendLine("    {");
      foreach (var prop in blockNodeProps)
         sb.AppendLine($"        {{ {fullyQualifiedKeywordClassName}.{prop.KeywordConstantName}, {arcParsePrefix}{prop.PropertyName} }},");
      sb.AppendLine("    };");
      sb.AppendLine();

      // --- THE NEWLY PLACED HELPER METHOD ---
      // This is now part of the generated parser class itself.
      sb.AppendLine("    /// <summary>");
      sb.AppendLine("    /// Parses all properties of a " +
                    targetTypeSymbol.Name +
                    " object marked with [ParseAs] from a BlockNode.");
      sb.AppendLine("    /// </summary>");
      sb.AppendLine("    /// <returns>A list of all child nodes that were not handled automatically.</returns>");
      sb.AppendLine($"    public static List<StatementNode> ParseProperties(BlockNode block, {targetTypeName} target, LocationContext ctx, string source, ref bool validation)");
      sb.AppendLine("    {");
      sb.AppendLine("        var unhandledNodes = new List<StatementNode>();");
      sb.AppendLine("        foreach (var propertyNode in block.Children)");
      sb.AppendLine("        {");
      sb.AppendLine("            bool wasHandled = false;");
      sb.AppendLine("            if (propertyNode is ContentNode cn)");
      sb.AppendLine("            {");
      sb.AppendLine("                var key = cn.KeyNode.GetLexeme(source);");
      sb.AppendLine("                if (_contentParsers.TryGetValue(key, out var parser))");
      sb.AppendLine("                {");
      sb.AppendLine("                    parser(cn, target, ctx, source, ref validation);");
      sb.AppendLine("                    wasHandled = true;");
      sb.AppendLine("                }");
      sb.AppendLine("            }");
      sb.AppendLine("            else if (propertyNode is BlockNode bn)");
      sb.AppendLine("            {");
      sb.AppendLine("                var key = bn.KeyNode.GetLexeme(source);");
      sb.AppendLine("                if (_blockParsers.TryGetValue(key, out var parser))");
      sb.AppendLine("                {");
      sb.AppendLine("                    parser(bn, target, ctx, source, ref validation);");
      sb.AppendLine("                    wasHandled = true;");
      sb.AppendLine("                }");
      sb.AppendLine("            }");
      sb.AppendLine();
      sb.AppendLine("            if (!wasHandled)");
      sb.AppendLine("            {");
      sb.AppendLine("                unhandledNodes.Add(propertyNode);");
      sb.AppendLine("            }");
      sb.AppendLine("        }");
      sb.AppendLine("        return unhandledNodes;");
      sb.AppendLine("    }");
      sb.AppendLine();

      //--- Generated Wrapper Methods (partial signatures) ---
      sb.AppendLine("    #region Parser Method Signatures");
      foreach (var prop in properties)
         sb.AppendLine($"    private static partial bool {arcParsePrefix}{prop.PropertyName}({prop.AstNodeType} node, {targetTypeName} target, LocationContext ctx, string source, ref bool validation);");
      sb.AppendLine("    #endregion");
      sb.AppendLine();

      // --- Generated Wrapper Methods ---
      sb.AppendLine("    #region Auto-Implemented Parsers");
      foreach (var prop in properties)
      {
         var wrapperMethodName = $"{arcParsePrefix}{prop.PropertyName}";

         // If the user has provided their own implementation, skip generation.
         if (handwrittenMethods.Contains(wrapperMethodName))
            continue;

         var toolMethod = FindMatchingTool(toolboxSymbol, prop.AstNodeType, prop.PropertyType);
         if (toolMethod == null)
            continue;

         var propTypeName = prop.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
         var actionName = $"\"{parserSymbol.Name}.{wrapperMethodName}\"";

         string toolMethodCall;
         if (toolMethod.IsGenericMethod)
            // For a generic tool, we need to specify the type argument, e.g., "ArcTryParse_Enum<MyEnum>"
            toolMethodCall = $"{toolMethod.Name}<{propTypeName}>";
         else
            // For non-generic tools, it's just the name.
            toolMethodCall = toolMethod.Name;

         sb.AppendLine($"    private static partial bool {wrapperMethodName}({prop.AstNodeType} node, {targetTypeName} target, LocationContext ctx, string source, ref bool validation)");
         sb.AppendLine("    {");
         sb.AppendLine($"        if ({toolMethodCall}(node, ctx, {actionName}, source, out {propTypeName} value, ref validation))");
         sb.AppendLine("        {");
         sb.AppendLine($"            target.{prop.PropertyName} = value;");
         sb.AppendLine("            return true;");
         sb.AppendLine("        }");
         sb.AppendLine("        return false;");
         sb.AppendLine("    }");
         sb.AppendLine();
      }

      sb.AppendLine("    #endregion");

      sb.AppendLine("}");

      return (hintName, sb.ToString());
   }

   private const string TOOL_METHOD_PREFIX = "ArcTryParse";

   private static IMethodSymbol? FindMatchingTool(INamedTypeSymbol toolboxSymbol,
                                                  string astNodeType,
                                                  ITypeSymbol propertyType)
   {
      if (propertyType.BaseType != null && propertyType.BaseType.ToDisplayString() == "System.Enum")
      {
         // The tool we are looking for is the generic "ArcTryParse_Enum"
         const string genericToolName = $"{TOOL_METHOD_PREFIX}_Enum";
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

      public PropertyMetadata(IPropertySymbol symbol, AttributeData attribute)
      {
         Symbol = symbol;
         Attribute = attribute;

         var astNodeTypeArg = attribute.ConstructorArguments[0];

         // Find the symbol for the enum member that this constant represents.
         // For 'AstNodeType.ContentNode', this will find the 'ContentNode' field symbol.
         var enumMemberSymbol = astNodeTypeArg.Type?.GetMembers()
                                              .OfType<IFieldSymbol>()
                                              .FirstOrDefault(f => f.ConstantValue != null &&
                                                                   f.ConstantValue.Equals(astNodeTypeArg.Value));

         // The name of that symbol is what we want (e.g., "ContentNode").
         // We provide a fallback just in case.
         AstNodeType = enumMemberSymbol?.Name ?? "Unknown";
         Keyword = attribute.ConstructorArguments[1].Value as string ?? ToSnakeCase(PropertyName);
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
}