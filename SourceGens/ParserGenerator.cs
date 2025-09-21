using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ParserGenerator.HelperClasses;

namespace ParserGenerator;

[Generator]
public class ParserSourceGenerator : IIncrementalGenerator
{
   private const string PARSER_FOR_ATTRIBUTE = "Arcanum.Core.CoreSystems.Parsing.ToolBox.ParserForAttribute";
   private const string PARSE_AS_ATTRIBUTE = "Arcanum.Core.CoreSystems.Parsing.ToolBox.ParseAsAttribute";
   private const string PARSING_TOOLBOX_CLASS = "Arcanum.Core.CoreSystems.Parsing.ToolBox.ParsingToolBox";

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

      if (Enumerable.Any(classSymbol.GetAttributes(),
                         attribute => string.Equals(attribute.AttributeClass?.ToDisplayString(),
                                                    PARSER_FOR_ATTRIBUTE,
                                                    StringComparison.Ordinal)))
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
         try
         {
            var attr = parserSymbol.GetAttributes()
                                   .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == PARSER_FOR_ATTRIBUTE);

            if (attr?.ConstructorArguments.FirstOrDefault().Value is not INamedTypeSymbol targetTypeSymbol)
               continue;

            var ignoredBlockKeys =
               AttributeHelper.GetAttributeArgumentValue(attr, 2, "ignoredBlockKeys", new string[] { }) ?? [];
            var ignoredContentKeys =
               AttributeHelper.GetAttributeArgumentValue(attr, 3, "ignoredContentKeys", new string[] { }) ?? [];

            // --- Collect Metadata from Target Type's Properties ---
            var propertiesToParse = new List<PropertyMetadata>();
            ExtractMetadata(targetTypeSymbol, propertiesToParse);
            //
            // // If no properties are marked for parsing, there's nothing to generate
            // if (propertiesToParse.Count == 0)
            //    continue;

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
                                                                     context,
                                                                     ignoredBlockKeys,
                                                                     ignoredContentKeys);
            context.AddSource(parserHintName, parserSource);
         }
         catch (Exception ex)
         {
            // ADD THIS to see the real error
            context.ReportDiagnostic(Diagnostic.Create(new("GEN001",
                                                           "Generator Crash",
                                                           "Generator failed for parser '{0}'. Error: {1}",
                                                           "Generator",
                                                           DiagnosticSeverity.Error,
                                                           true),
                                                       parserSymbol.Locations.FirstOrDefault(),
                                                       parserSymbol.Name,
                                                       ex.ToString()));
         }
      }
   }

   private static (string HintName, string Source) GenerateParserClass(INamedTypeSymbol parserSymbol,
                                                                       INamedTypeSymbol targetTypeSymbol,
                                                                       List<PropertyMetadata> properties,
                                                                       INamedTypeSymbol toolboxSymbol,
                                                                       string fullyQualifiedKeywordClassName,
                                                                       ImmutableArray<INamedTypeSymbol> parsers,
                                                                       SourceProductionContext context,
                                                                       string[] ignoredBlockKeys,
                                                                       string[] ignoredContentKeys)
   {
      var hintName = $"{parserSymbol.ContainingNamespace}.{parserSymbol.Name}.g.cs";
      var targetTypeName = targetTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

      var handwrittenMethods = parserSymbol.GetMembers()
                                           .OfType<IMethodSymbol>()
                                           .Select(m => m.Name)
                                           .ToImmutableHashSet();

      // Group properties by the AST node they parse from
      var contentNodeProps = properties.Where(p => p.AstNodeType == NodeType.ContentNode).ToList();
      var embeddedBlockProps = properties.Where(p => p.IsEmbedded).ToList();
      var standardBlockProps = properties.Where(p => !p.IsEmbedded && p.AstNodeType == NodeType.BlockNode).ToList();
      var statementNodeProps = properties.Where(p => p.AstNodeType == NodeType.StatementNode).ToList();

      var sb = new StringBuilder();
      sb.AppendLine("// <auto-generated/>");
      sb.AppendLine($"namespace {parserSymbol.ContainingNamespace};");
      sb.AppendLine();
      // --- Usings ---
      GenerateUsings(targetTypeSymbol, sb, toolboxSymbol);

      sb.AppendLine($"public partial class {parserSymbol.Name}");
      sb.AppendLine("{");

      // --- Dictionaries ---
      try
      {
         GenerateParserDictionaries(fullyQualifiedKeywordClassName,
                                    sb,
                                    targetTypeName,
                                    contentNodeProps,
                                    standardBlockProps,
                                    embeddedBlockProps,
                                    statementNodeProps);

         // --- ParseProperties Method ---
         GenerateParsePropertiesMethod(targetTypeSymbol,
                                       sb,
                                       targetTypeName,
                                       ignoredBlockKeys,
                                       ignoredContentKeys);

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
      }
      catch (Exception e)
      {
         context.ReportDiagnostic(Diagnostic.Create(new(id: "GEN002",
                                                        title: "Generator Crash",
                                                        messageFormat:
                                                        $"Generator failed while processing parser '{parserSymbol.Name}' for target type '{targetTypeSymbol.Name}'. Error: {e}",
                                                        category: "Generator",
                                                        DiagnosticSeverity.Error,
                                                        isEnabledByDefault: true),
                                                    parserSymbol.Locations.FirstOrDefault()));
         throw;
      }

      return (hintName, sb.ToString());
   }

   #region Generator Helpers

   private static void GenerateParserDictionaries(string fullyQualifiedKeywordClassName,
                                                  StringBuilder sb,
                                                  string targetTypeName,
                                                  List<PropertyMetadata> contentNodeProps,
                                                  List<PropertyMetadata> blockNodeProps,
                                                  List<PropertyMetadata> embeddedBlockNodeProps,
                                                  List<PropertyMetadata> statementNodeProps)
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
      sb.AppendLine($"    private static readonly Dictionary<string, Pdh.StatementParser<{targetTypeName}>> _statementParsers = new()");
      sb.AppendLine("    {");

      foreach (var prop in statementNodeProps)
         AppendParserMethodMapping(fullyQualifiedKeywordClassName, sb, prop);

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
   {
      if (prop.CustomParserMethodName != null)
         return prop.CustomParserMethodName;

      return $"{ARC_PARSE_PREFIX}{FlagsPrefix(prop)}{prop.PropertyName}{IsContentNodeListSuffix(prop)}";
   }

   private static string IsContentNodeListSuffix(PropertyMetadata prop)
   {
      if (prop.IsShatteredList)
         return "_PartList";
      else
         return string.Empty;
   }

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
         if (GatherMethodCreationData(parserSymbol,
                                      toolboxSymbol,
                                      sb,
                                      handwrittenMethods,
                                      parsers,
                                      context,
                                      prop,
                                      out var wrapperMethodName,
                                      out var propTypeName,
                                      out var actionName,
                                      out var toolMethodCall,
                                      out var genericType,
                                      out var hasEmbeddedParser,
                                      out var customParserName,
                                      out var allowUnknownNodes))
            continue;

         if (hasEmbeddedParser)
         {
            if (!prop.IsCollection)
            {
               GenerateEmbeddedPropertyParserMethod(sb, targetTypeName, prop, customParserName!, allowUnknownNodes);
               continue;
            }

            if (!prop.IsShatteredList)
            {
               GenerateEmbeddedCollectionPropertyParserMethod(sb,
                                                              targetTypeName,
                                                              actionName,
                                                              prop,
                                                              wrapperMethodName,
                                                              genericType,
                                                              customParserName!,
                                                              allowUnknownNodes);
               continue;
            }

            GenerateShatteredEmbeddedCollectionPropertyParserMethod(sb,
                                                                    targetTypeName,
                                                                    prop,
                                                                    wrapperMethodName,
                                                                    genericType,
                                                                    customParserName!,
                                                                    allowUnknownNodes);
            continue;
         }

         if (prop.IsCollection && !prop.IsShatteredList)
         {
            GenerateCollectionMethodCall(sb,
                                         prop,
                                         wrapperMethodName,
                                         toolMethodCall,
                                         genericType,
                                         targetTypeName,
                                         actionName);
            continue;
         }

         GeneratePropertyParserMethod(sb,
                                      targetTypeName,
                                      prop,
                                      genericType,
                                      propTypeName,
                                      wrapperMethodName,
                                      toolMethodCall,
                                      actionName);
      }

      sb.AppendLine("    #endregion");
      sb.AppendLine("}");
   }

   private static bool GatherMethodCreationData(INamedTypeSymbol parserSymbol,
                                                INamedTypeSymbol toolboxSymbol,
                                                StringBuilder sb,
                                                ImmutableHashSet<string> handwrittenMethods,
                                                ImmutableArray<INamedTypeSymbol> parsers,
                                                SourceProductionContext context,
                                                PropertyMetadata prop,
                                                out string wrapperMethodName,
                                                out string propTypeName,
                                                out string actionName,
                                                out string toolMethodCall,
                                                out ITypeSymbol? genericType,
                                                out bool hasEmbeddedParser,
                                                out string? customParserName,
                                                out bool allowUnknownNodes)
   {
      // If a custom parser is specified, we DO NOT generate an implementation.
      if (GenerateToolMethodCall(parserSymbol,
                                 toolboxSymbol,
                                 prop,
                                 handwrittenMethods,
                                 out wrapperMethodName,
                                 out propTypeName,
                                 out actionName,
                                 out toolMethodCall,
                                 out genericType))
      {
         sb.AppendLine($"    // Property '{prop.PropertyName}' is handled by custom parser '{toolMethodCall}'.");
         hasEmbeddedParser = false;
         customParserName = null;
         allowUnknownNodes = false;
         return true;
      }

      // We have a pure embedded property, not in combination with a list.
      hasEmbeddedParser = TryGetEmbeddedParserName(parserSymbol,
                                                   sb,
                                                   parsers,
                                                   context,
                                                   prop,
                                                   out customParserName,
                                                   out allowUnknownNodes);
      return false;
   }

   private static void GeneratePropertyParserMethod(StringBuilder sb,
                                                    string targetTypeName,
                                                    PropertyMetadata prop,
                                                    ITypeSymbol? genericType,
                                                    string propTypeName,
                                                    string wrapperMethodName,
                                                    string toolMethodCall,
                                                    string actionName)
   {
      var outvalue = prop.IsCollection ? genericType?.Name ?? "object" : propTypeName;

      sb.AppendLine("// ### Property Parser ###");
      sb.AppendLine($"    private static partial bool {wrapperMethodName}({prop.AstNodeType} node, {targetTypeName} target, LocationContext ctx, string source, ref bool validation)");
      sb.AppendLine("    {");
      sb.AppendLine($"        if ({toolMethodCall}(node, ctx, {actionName}, source, out {outvalue} value, ref validation))");
      sb.AppendLine("        {");
      if (IsFlagsEnum(prop))
         sb.AppendLine($"            target.{prop.PropertyName} |= value;");
      else if (prop.IsShatteredList)
      {
         sb.AppendLine($"            if (target.{prop.PropertyName} == null)");
         sb.AppendLine($"                target.{prop.PropertyName} = new {propTypeName}();");
         sb.AppendLine();
         sb.AppendLine($"            target.{prop.PropertyName}.Add(value);");
      }
      else
         sb.AppendLine($"            target.{prop.PropertyName} = value;");

      sb.AppendLine("            return true;");
      sb.AppendLine("        }");
      sb.AppendLine("        return false;");

      sb.AppendLine("    }");
   }

   private static void GenerateShatteredEmbeddedCollectionPropertyParserMethod(StringBuilder sb,
                                                                               string targetTypeName,
                                                                               PropertyMetadata prop,
                                                                               string wrapperMethodName,
                                                                               ITypeSymbol? genericType,
                                                                               string customParserName,
                                                                               bool allowUnknownNodes)
   {
      sb.AppendLine("// ### Embedded Shattered Collection Property Parser ###");
      var itemTypeName = genericType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";

      sb.AppendLine($"    private static partial bool {wrapperMethodName}({prop.AstNodeType} node, {targetTypeName} target, LocationContext ctx, string source, ref bool validation)");
      sb.AppendLine("    {");
      sb.AppendLine($"        var newInstance = ({itemTypeName})Activator.CreateInstance(typeof({itemTypeName}))!;");
      sb.AppendLine($"        {customParserName}.ParseProperties(node, newInstance, ctx, source, ref validation, {allowUnknownNodes.ToString().ToLower()});");
      sb.AppendLine($"        target.{prop.PropertyName}.Add(newInstance);");
      sb.AppendLine("        return true;");
      sb.AppendLine("    }");
   }

   private static void GenerateEmbeddedCollectionPropertyParserMethod(StringBuilder sb,
                                                                      string targetTypeName,
                                                                      string actionName,
                                                                      PropertyMetadata prop,
                                                                      string wrapperMethodName,
                                                                      ITypeSymbol? genericType,
                                                                      string customParserName,
                                                                      bool allowUnknownNodes)
   {
      sb.AppendLine("// ### Embedded Collection Property Parser ###");
      if (prop.AstNodeType != NodeType.BlockNode)
      {
         sb.AppendLine($"    // ERROR: Embedded collection property '{prop.PropertyName}' must be parsed from a BlockNode.");
         return;
      }

      var itemTypeName = genericType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";

      var pdhMethodName = prop.IsEmbedded
                             ? "ParseEmbeddedCollection"
                             : prop.ItemNodeType switch
                             {
                                NodeType.ContentNode => "ParseContentCollection",
                                NodeType.KeyOnlyNode => "ParseKeyOnlyCollection",
                                NodeType.BlockNode => "ParseBlockCollection",
                                NodeType.StatementNode => "ParseStatementCollection",
                                _ => throw new ArgumentOutOfRangeException(),
                             };

      sb.AppendLine($"    private static partial bool {wrapperMethodName}({prop.AstNodeType} node, {targetTypeName} target, LocationContext ctx, string source, ref bool validation)");
      sb.AppendLine("    {");
      sb.AppendLine($"        target.{prop.PropertyName} = Pdh.{pdhMethodName}<{itemTypeName}>(node, ctx, source, {actionName}, ref validation, {customParserName}.ParseProperties, {allowUnknownNodes.ToString().ToLower()});");
      sb.AppendLine("        return true;");
      sb.AppendLine("    }");
   }

   private static void GenerateCollectionMethodCall(StringBuilder sb,
                                                    PropertyMetadata prop,
                                                    string wrapperMethodName,
                                                    string toolMethodCall,
                                                    ITypeSymbol? genericType,
                                                    string targetTypeName,
                                                    string actionName)
   {
      sb.AppendLine("// ### Collection Property Parser ###");
      if (prop.AstNodeType != NodeType.BlockNode)
      {
         sb.AppendLine($"    // ERROR: Collection property '{prop.PropertyName}' must be parsed from a BlockNode.");
         return;
      }

      var itemTypeName = genericType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";

      var pdhMethodName = prop.ItemNodeType switch
      {
         NodeType.ContentNode => "ParseContentCollection",
         NodeType.KeyOnlyNode => "ParseKeyOnlyCollection",
         NodeType.BlockNode => "ParseBlockCollection",
         NodeType.StatementNode => "ParseStatementCollection",
         _ => throw new ArgumentOutOfRangeException(),
      };

      sb.AppendLine($"    private static partial bool {wrapperMethodName}({prop.AstNodeType} node, {targetTypeName} target, LocationContext ctx, string source, ref bool validation)");
      sb.AppendLine("    {");
      sb.AppendLine($"        target.{prop.PropertyName} = Pdh.{pdhMethodName}<{itemTypeName}>(node, ctx, source, {actionName}, ref validation, {toolMethodCall});");
      sb.AppendLine("        return true;");
      sb.AppendLine("    }");
   }

   private static void WriteIgnoreList(StringBuilder sb, string ignoredblocktypes, string[] ignoredBlockKeys)
   {
      sb.AppendLine($"    private static readonly HashSet<string> {ignoredblocktypes} = new(StringComparer.OrdinalIgnoreCase)");
      sb.AppendLine("    {");
      foreach (var key in ignoredBlockKeys)
         sb.AppendLine($"        \"{key}\",");
      sb.AppendLine("    };");
   }

   private static bool TryGetEmbeddedParserName(INamedTypeSymbol parserSymbol,
                                                StringBuilder sb,
                                                ImmutableArray<INamedTypeSymbol> parsers,
                                                SourceProductionContext context,
                                                PropertyMetadata prop,
                                                out string? customParserName,
                                                out bool allowUnknownNodes)
   {
      customParserName = null;
      allowUnknownNodes = false;

      if (!prop.IsEmbedded)
         return false;

      var nestedParserSymbol = FindParserForType(prop.IsCollection ? prop.ItemType : prop.PropertyType,
                                                 out allowUnknownNodes,
                                                 parsers);

      if (nestedParserSymbol == null)
      {
         var propTypeName2 = prop.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
         context.ReportDiagnostic(Diagnostic.Create(new(id: "PG002",
                                                        title: "Missing Parser for Embedded Type",
                                                        messageFormat:
                                                        $"No parser with [ParserFor(typeof({propTypeName2}))] was found for embedded property '{prop.PropertyName}' in parser '{parserSymbol.Name}'.",
                                                        category: "SourceGens",
                                                        DiagnosticSeverity.Error,
                                                        isEnabledByDefault: true),
                                                    Location.None));

         sb.AppendLine($"    // ERROR: No parser with [ParserFor(typeof({propTypeName2}))] was found for embedded property '{prop.PropertyName}'.");
         return true;
      }

      customParserName = nestedParserSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
      return true;
   }

   private static void GenerateEmbeddedPropertyParserMethod(StringBuilder sb,
                                                            string targetTypeName,
                                                            PropertyMetadata prop,
                                                            string customParserName,
                                                            bool allowUnknownNodes)
   {
      sb.AppendLine("// ### Embedded Property Parser ###");
      var propTypeName2 = prop.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
      sb.AppendLine($"    private static partial bool {PropCustomParserMethodName(prop)}(BlockNode node, {targetTypeName} target, LocationContext ctx, string source, ref bool validation)");
      sb.AppendLine("    {");
      sb.AppendLine($"        target.{prop.PropertyName} = new {propTypeName2}();");
      sb.AppendLine($"        {customParserName}.ParseProperties(node, target.{prop.PropertyName}, ctx, source, ref validation, {allowUnknownNodes.ToString().ToLower()});");
      sb.AppendLine("        return true;");
      sb.AppendLine("    }");
   }

   /// <summary>
   /// For Collections the tool method will be for the item type, e.g., <c>ArcTryParse_String</c> for <c>List&lt;string&gt;</c> <br/>
   /// For Enums the tool method will be <c>ArcTryParse_Enum&lt;T&gt;</c> <br/>
   /// For Flags Enums the tool method will be <c>ArcTryParse_FlagsEnum&lt;T&gt;</c> <br/>
   /// For all other types the tool method will be <c>ArcTryParse_Typename</c> <br/>
   /// </summary>
   private static bool GenerateToolMethodCall(INamedTypeSymbol parserSymbol,
                                              INamedTypeSymbol toolboxSymbol,
                                              PropertyMetadata prop,
                                              ImmutableHashSet<string> handwrittenMethods,
                                              out string wrapperMethodName,
                                              out string propTypeName,
                                              out string actionName,
                                              out string toolMethodCall,
                                              out ITypeSymbol? genericType)
   {
      if (prop.CustomParserMethodName != null)
      {
         wrapperMethodName = null!;
         propTypeName = null!;
         actionName = null!;
         toolMethodCall = prop.CustomParserMethodName;
         genericType = null;
         return true;
      }

      wrapperMethodName = PropCustomParserMethodName(prop);

      // If the user has provided their own implementation, skip generation.
      if (handwrittenMethods.Contains(wrapperMethodName))
      {
         propTypeName = null!;
         actionName = null!;
         toolMethodCall = wrapperMethodName;
         genericType = null;
         return true;
      }

      actionName = $"\"{parserSymbol.Name}.{wrapperMethodName}\"";
      propTypeName = prop.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
      var toolMethod = FindMatchingTool(toolboxSymbol, prop, out toolMethodCall, out var message, out genericType);

      if (prop.IsEmbedded)
         return false;

      // We could not find a matching tool method.
      if (toolMethod == null)
      {
         toolMethodCall = message;
         return false;
      }

      // For a generic tool, we need to specify the type argument, e.g., "ArcTryParse_Enum<MyEnum>"
      toolMethodCall = toolMethod.IsGenericMethod ? $"{toolMethod.Name}<{propTypeName}>" : toolMethod.Name;
      return false;
   }

   private static INamedTypeSymbol? FindParserForType(
      ITypeSymbol targetType,
      out bool allowUnknownNodes,
      ImmutableArray<INamedTypeSymbol> allKnownParsers)
   {
      allowUnknownNodes = false;
      foreach (var potentialParser in allKnownParsers)
      {
         var attr = potentialParser.GetAttributes()
                                   .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == PARSER_FOR_ATTRIBUTE);

         if (attr?.ConstructorArguments.FirstOrDefault().Value is not INamedTypeSymbol attrTargetType)
            continue;

         allowUnknownNodes = AttributeHelper.GetAttributeArgumentValue(attr, 1, "allowUnknownNodes", false);

         if (SymbolEqualityComparer.Default.Equals(attrTargetType, targetType))
            return potentialParser;
      }

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
         if (!string.IsNullOrWhiteSpace(prop.CustomParserMethodName))
         {
            sb.AppendLine($"    // Property '{prop.PropertyName}' is handled by custom parser '{prop.CustomParserMethodName}'.");
            continue;
         }

         sb.AppendLine($"    private static partial bool {PropCustomParserMethodName(prop)}({prop.AstNodeType} node, {targetTypeName} target, LocationContext ctx, string source, ref bool validation);");
      }

      sb.AppendLine("    #endregion");
      sb.AppendLine();
   }

   private static void GenerateParsePropertiesMethod(INamedTypeSymbol targetTypeSymbol,
                                                     StringBuilder sb,
                                                     string targetTypeName,
                                                     string[] ignoredBlockKeys,
                                                     string[] ignoredContentKeys)
   {
      // Write the ignored keys lists
      WriteIgnoreList(sb, "_ignoredBlockTypes", ignoredBlockKeys);
      sb.AppendLine();
      WriteIgnoreList(sb, "_ignoredContentTypes", ignoredContentKeys);

      sb.AppendLine("    /// <summary>");
      sb.AppendLine("    /// Parses all properties of a " +
                    targetTypeSymbol.Name +
                    " object marked with [ParseAs] from a BlockNode.");
      sb.AppendLine("    /// This is a facade that calls the centralized logic in the Pdh helper class.");
      sb.AppendLine("    /// </summary>");
      sb.AppendLine("    /// <returns>A list of all child nodes that were not handled automatically.</returns>");
      sb.AppendLine($"    public static void ParseProperties(BlockNode block, {targetTypeName} target, LocationContext ctx, string source, ref bool validation, bool allowUnknownNodes)");
      sb.AppendLine("    {");
      sb.AppendLine($"        Pdh.ParseProperties(block, target, ctx, source, ref validation, _contentParsers, _blockParsers, _statementParsers, _ignoredBlockTypes, _ignoredContentTypes, allowUnknownNodes);");
      sb.AppendLine("    }");
      sb.AppendLine();
   }

   #endregion

   public const string ARC_PARSE_PREFIX = "ArcParse_";
   private const string TOOL_METHOD_PREFIX = "ArcTryParse";

   private static IMethodSymbol? FindMatchingTool(INamedTypeSymbol toolboxSymbol,
                                                  PropertyMetadata prop,
                                                  out string expectedToolName,
                                                  out string message,
                                                  out ITypeSymbol? genericType)
   {
      message = string.Empty;
      var propertyType = prop.PropertyType;
      if (propertyType.BaseType != null && propertyType.BaseType.ToDisplayString() == "System.Enum")
      {
         var isFlagsEnum = propertyType.GetAttributes()
                                       .Any(attr => attr.AttributeClass?.ToDisplayString() == "System.FlagsAttribute");

         var genericToolName = isFlagsEnum
                                  ? $"{TOOL_METHOD_PREFIX}_FlagsEnum"
                                  : $"{TOOL_METHOD_PREFIX}_Enum";

         expectedToolName = genericToolName;

         var methods = toolboxSymbol.GetMembers(genericToolName).OfType<IMethodSymbol>();

         foreach (var member in methods)
         {
            // A valid tool must be static, generic, and have the right number of parameters.
            if (!member.IsStatic || !member.IsGenericMethod || member.Parameters.Length < 5)
               continue;

            if (member.Parameters[0].Type.Name != prop.AstNodeType.ToString())
               continue;

            var outParam = member.Parameters.FirstOrDefault(p => p.RefKind == RefKind.Out);
            if (outParam == null || outParam.Type.TypeKind != TypeKind.TypeParameter)
               continue;

            genericType = member.TypeArguments.FirstOrDefault();
            return member;
         }

         genericType = null;
         return null;
      }

      // We have a block which represents a list of content or keyOnly nodes, e.g.
      if (prop.IsCollection)
      {
      }

      expectedToolName = GetExpectedToolName(propertyType, TOOL_METHOD_PREFIX, prop, out genericType);
      return FindToolByName(toolboxSymbol,
                            expectedToolName,
                            prop.IsCollection ? prop.ItemNodeType : prop.AstNodeType,
                            prop.IsCollection ? genericType! : propertyType,
                            prop,
                            out message);
   }

   // Extracted the search logic to a reusable method
   private static IMethodSymbol? FindToolByName(INamedTypeSymbol toolboxSymbol,
                                                string toolName,
                                                NodeType astNodeType,
                                                ITypeSymbol propertyType,
                                                PropertyMetadata prop,
                                                out string message)
   {
      message = string.Empty;

      message +=
         $"\n\n//Searching for method '{toolName}' in ParsingToolBox for AST node type '{astNodeType}' and property type '{propertyType.Name}'.";

      foreach (var member in toolboxSymbol.GetMembers(toolName).OfType<IMethodSymbol>())
      {
         if (!member.IsStatic || member.Parameters.Length < 4)
            continue;

         if (member.Parameters[0].Type.Name == astNodeType.ToString())
         {
            var outParam = member.Parameters.LastOrDefault(p => p.RefKind == RefKind.Out);
            if (outParam != null && SymbolEqualityComparer.Default.Equals(outParam.Type, propertyType))
            {
               message = string.Empty;
               return member;
            }

            if (prop.IsShatteredList)
            {
               var genericType = propertyType is INamedTypeSymbol { IsGenericType: true } namedType
                                    ? namedType.TypeArguments.FirstOrDefault()
                                    : null;
               if (genericType != null &&
                   outParam != null &&
                   SymbolEqualityComparer.Default.Equals(outParam.Type, genericType))
               {
                  message = string.Empty;
                  return member;
               }
            }

            message =
               $"Found method '{toolName}' but its 'out' parameter type '{outParam?.Type.Name}' does not match the property type '{propertyType.Name}'.";
            return null;
         }
      }

      message +=
         $"\n\n//No matching method '{toolName}' found." +
         $"\n//Found methods:" +
         $"\n//-  {string.Join("\n//- ", toolboxSymbol.GetMembers(toolName).OfType<IMethodSymbol>().Select(m => m.ToDisplayString()))}";
      return null;
   }

   /// <summary>
   /// For a collection returns the expected tool name for the item type. <br/>
   /// For non-collections returns the expected tool name for the property type.
   /// </summary>
   private static string GetExpectedToolName(ITypeSymbol type,
                                             string prefix,
                                             PropertyMetadata prop,
                                             out ITypeSymbol? genericType)
   {
      genericType = null;
      if (!prop.IsCollection)
         return $"{prefix}_{type.Name}";

      if (type is not INamedTypeSymbol { IsGenericType: true } namedType)
         return $"{prefix}_Object";

      var itemType = namedType.TypeArguments.FirstOrDefault();
      if (itemType == null)
         return $"{prefix}_Object";

      // If we have a collection we only need the method for the item type
      genericType = itemType;
      return $"{prefix}_{itemType.Name}";
   }

   public enum NodeType
   {
      ContentNode,
      BlockNode,
      KeyOnlyNode,
      StatementNode,
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
      public string PropertyName => Symbol.Name;
      public ITypeSymbol PropertyType => Symbol.Type;
      public ITypeSymbol ItemType => PropertyType is INamedTypeSymbol { IsGenericType: true } namedType
                                        ? namedType.TypeArguments.FirstOrDefault() ?? Symbol.Type
                                        : Symbol.Type;
      public string Keyword { get; }
      public string KeywordConstantName => SanitizeToIdentifier(Keyword).ToUpper();
      public NodeType AstNodeType { get; }
      public string? CustomParserMethodName { get; }
      public bool IsEmbedded { get; }
      public bool IsShatteredList { get; }
      public bool IsCollection { get; }
      public NodeType ItemNodeType { get; }

      public PropertyMetadata(IPropertySymbol symbol, AttributeData attribute)
      {
         Symbol = symbol;

         IsShatteredList = AttributeHelper.SimpleGetAttributeArgumentValue<bool>(attribute, 3, "isShatteredList");
         CustomParserMethodName =
            AttributeHelper.SimpleGetAttributeArgumentValue<string?>(attribute, 2, "customParser");
         ItemNodeType =
            AttributeHelper.SimpleGetAttributeArgumentValue(attribute, 4, "itemNodeType", NodeType.KeyOnlyNode);
         IsEmbedded = AttributeHelper.SimpleGetAttributeArgumentValue<bool>(attribute, 5, "isEmbedded");
         IsCollection = PropertyType.AllInterfaces.Any(i => i.ToDisplayString() == "System.Collections.ICollection");

         if (IsEmbedded || (IsCollection && !IsShatteredList))
         {
            AstNodeType = NodeType.BlockNode;
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

               AstNodeType = Enum.TryParse(enumMemberSymbol?.Name, out NodeType nt) ? nt : NodeType.ContentNode;
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
            propertiesToParse.Add(new(member, parseAsAttr));
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