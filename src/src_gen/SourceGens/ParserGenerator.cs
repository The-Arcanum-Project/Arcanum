using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ParserGenerator.ParserGen;
using ParserGenerator.SubClasses;

namespace ParserGenerator;

[Generator]
public class ParserSourceGenerator : IIncrementalGenerator
{
   public enum NodeType
   {
      ContentNode,
      BlockNode,
      KeyOnlyNode,
      StatementNode,
   }

   private const string PARSER_FOR_ATTRIBUTE = "Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox.ParserForAttribute";
   private const string PARSE_AS_ATTRIBUTE = "Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox.ParseAsAttribute";
   private const string SAVE_AS_ATTRIBUTE = "Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox.SaveAsAttribute";
   private const string PARSING_TOOLBOX_CLASS = "Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox.ParsingToolBox";

   public const string ARC_PARSE_PREFIX = "ArcParse_";
   private const string TOOL_METHOD_PREFIX = "ArcTryParse";

   public void Initialize(IncrementalGeneratorInitializationContext context)
   {
      var provider = context.SyntaxProvider
                            .CreateSyntaxProvider((node, _)
                                                     => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                                                  GetParserClassSymbol)
                            .Where(s => s is not null); // Filter out classes that don't match our criteria

      context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
                                   (spc, source) => { Generate(source.Left, source.Right, spc); });
   }

   /// <summary>
   ///    This "transform" step is a semantic filter. It takes the candidate classes
   ///    from the predicate and checks if they *actually* have our [ParserFor] attribute.
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
   ///    This is the main method where we will eventually generate all our code.
   ///    For now, it will just prove that we've found the right classes.
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
         try
         {
            var attr = parserSymbol.GetAttributes()
                                   .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == PARSER_FOR_ATTRIBUTE);

            if (attr?.ConstructorArguments.FirstOrDefault().Value is not INamedTypeSymbol targetTypeSymbol)
               continue;

            var cMetadata = new ParserClassMetadata(attr);

            // --- Collect Metadata from Target Type's Properties ---
            var propertiesToParse = new List<PropertyMetadata>();
            ExtractMetadata(targetTypeSymbol, propertiesToParse, context);

            // Generate the Parser class
            var (parserHintName, parserSource) = GenerateParserClass(parserSymbol,
                                                                     targetTypeSymbol,
                                                                     propertiesToParse,
                                                                     toolboxSymbol,
                                                                     parsers,
                                                                     context,
                                                                     cMetadata);
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

   private static (string HintName, string Source) GenerateParserClass(INamedTypeSymbol parserSymbol,
                                                                       INamedTypeSymbol targetTypeSymbol,
                                                                       List<PropertyMetadata> properties,
                                                                       INamedTypeSymbol toolboxSymbol,
                                                                       ImmutableArray<INamedTypeSymbol> parsers,
                                                                       SourceProductionContext context,
                                                                       ParserClassMetadata? classMetadata)
   {
      var hintName = $"{parserSymbol.ContainingNamespace}.{parserSymbol.Name}.g.cs";
      var targetTypeName = targetTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

      var handwrittenMethods = parserSymbol.GetMembers()
                                           .OfType<IMethodSymbol>()
                                           .Select(m => m.Name)
                                           .ToImmutableHashSet();

      // Group properties by the AST node they parse from

      properties.RemoveAll(x => x.Ignore);
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
         // --- Wrapper Methods (partial signatures) ---
         GenerateParserMethodSignatures(properties, sb, targetTypeName);

         if (classMetadata != null)
            // --- Generated Wrapper Methods ---
            GenerateAutoImplementedParsers(parserSymbol,
                                           properties,
                                           toolboxSymbol,
                                           sb,
                                           handwrittenMethods,
                                           targetTypeName,
                                           parsers,
                                           context,
                                           classMetadata,
                                           targetTypeSymbol);
      }
      catch (Exception e)
      {
         context.ReportDiagnostic(Diagnostic.Create(new("GEN002",
                                                        "Generator Crash",
                                                        $"Generator failed while processing parser '{parserSymbol.Name}' for target type '{targetTypeSymbol.Name}'. Error: {e}",
                                                        "Generator",
                                                        DiagnosticSeverity.Error,
                                                        true),
                                                    parserSymbol.Locations.FirstOrDefault()));
         throw;
      }

      return (hintName, sb.ToString());
   }

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

         // message +=
         //    $"\n//Expected method signature: static bool {genericToolName}<{propertyType.Name}>({prop.AstNodeType} node, ref ParsingContext pc, out {propertyType.Name} value)";
         // message += $"Possible candidates:";

         foreach (var member in methods)
         {
            // message += $"\n//- {member.ToDisplayString()}";
            // A valid tool must be static, generic, and have the right number of parameters.
            if (!member.IsStatic || !member.IsGenericMethod || member.Parameters.Length != 3)
               // message +=
               //    $"\n//Skipping method '{member.Name}' - must be static, generic, and have 3 parameters.";
               continue;

            if (member.Parameters[0].Type.Name != prop.AstNodeType.ToString())
               // message +=
               //    $"\n//Skipping method '{member.Name}' - first parameter type '{member.Parameters[0].Type.Name}' does not match AST node type '{prop.AstNodeType}'.";
               continue;

            var outParam = member.Parameters.FirstOrDefault(p => p.RefKind == RefKind.Out);
            if (outParam == null || outParam.Type.TypeKind != TypeKind.TypeParameter)
               // message +=
               //    $"\n//Skipping method '{member.Name}' - must have an 'out' parameter of the generic type.";
               continue;

            genericType = member.TypeArguments.FirstOrDefault();
            // message += $"\n//Found matching generic tool method '{member.Name}' for enum property '{prop.PropertyName}'.";
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

      // Expected signature details
      message +=
         $"\n//Expected method signature: static bool {toolName}({astNodeType} node, ref ParsingContext pc, out {propertyType.Name} value)";

      message +=
         $"\n==>IsCollection: {prop.IsCollection}; IsShatteredList: {prop.IsShatteredList}; IsEmbedded: {prop.IsEmbedded}; IsHashSet: {prop.IsHashSet}";

      foreach (var member in toolboxSymbol.GetMembers(toolName).OfType<IMethodSymbol>())
      {
         if (!member.IsStatic || member.Parameters.Length != 3)
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
   ///    For a collection returns the expected tool name for the item type. <br />
   ///    For non-collections returns the expected tool name for the property type.
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

   private static void ExtractMetadata(INamedTypeSymbol targetTypeSymbol,
                                       List<PropertyMetadata> propertiesToParse,
                                       SourceProductionContext context)
   {
      foreach (var member in targetTypeSymbol.GetMembers().OfType<IPropertySymbol>())
      {
         // Check for [ParseAs] first
         var parseAsAttr = member.GetAttributes()
                                 .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == PARSE_AS_ATTRIBUTE);
         if (parseAsAttr != null)
            propertiesToParse.Add(new(member, parseAsAttr));
         else if (member.GetAttributes()
                        .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == SAVE_AS_ATTRIBUTE) !=
                  null)
            // Report warning
            context.ReportDiagnostic(Diagnostic.Create(new("PG001",
                                                           "Missing [ParseAs] Attribute",
                                                           $"Property '{member.Name}' in class '{targetTypeSymbol.Name}' is missing the required [ParseAs] attribute and will be ignored by the parser generator.",
                                                           "SourceGens",
                                                           DiagnosticSeverity.Warning,
                                                           true),
                                                       Location.None));
      }
   }

   private static void ReportMissingDependency(SourceProductionContext context)
   {
      // Report a diagnostic that the ParsingToolBox class is missing
      context.ReportDiagnostic(Diagnostic.Create(new("PARSERGEN001",
                                                     "Missing Dependency",
                                                     $"The required class '{PARSING_TOOLBOX_CLASS}' is not found. Ensure the necessary assembly is referenced.",
                                                     "SourceGens",
                                                     DiagnosticSeverity.Warning,
                                                     true),
                                                 Location.None));
   }

   #region Generator Helpers

   private static void GenerateParserDictionaries(StringBuilder sb,
                                                  string targetTypeName,
                                                  List<PropertyMetadata> dynamicBlockNodeProps,
                                                  List<PropertyMetadata> dynamicContentParsers)
   {
      sb.AppendLine();

      sb.AppendLine($"    internal static readonly Pdh.BlockParser<{targetTypeName}>[] _dynamicBlockParsers = ");
      sb.AppendLine("    [");
      foreach (var prop in dynamicBlockNodeProps)
         sb.AppendLine($"        {PropCustomParserMethodName(prop)},");
      sb.AppendLine("    ];");
      sb.AppendLine();

      sb.AppendLine($"    internal static readonly Pdh.ContentParser<{targetTypeName}>[] _dynamicContentParsers = ");
      sb.AppendLine("    [");
      foreach (var prop in dynamicContentParsers)
         sb.AppendLine($"        {PropCustomParserMethodName(prop)},");
      sb.AppendLine("    ];");
      sb.AppendLine();
   }

   private static string PropCustomParserMethodName(PropertyMetadata prop)
   {
      if (prop.CustomParserMethodName != null)
         return prop.CustomParserMethodName;

      return $"{ARC_PARSE_PREFIX}{FlagsPrefix(prop)}{prop.PropertyName}{IsContentNodeListSuffix(prop)}";
   }

   private static string IsContentNodeListSuffix(PropertyMetadata prop) => prop.IsShatteredList ? "_PartList" : string.Empty;

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
      sb.AppendLine("using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;");
      sb.AppendLine("using System.Collections.ObjectModel;");
      sb.AppendLine("using Arcanum.Core.Registry;");
      sb.AppendLine("using System.Runtime.CompilerServices;");
      sb.AppendLine("using Arcanum.Core.GameObjects.BaseTypes;");
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
                                                      SourceProductionContext context,
                                                      ParserClassMetadata classMetadata,
                                                      INamedTypeSymbol targetTypeSymbol)
   {
      sb.AppendLine("    #region Auto-Implemented Parsers");
      List<PropertyData> propDataList = new(properties.Count);

      foreach (var prop in properties)
      {
         var propData = new PropertyData { PropertyMetadata = prop };
         propDataList.Add(propData);
         propData.MethodCall = propData.PropertyMetadata.CustomParserMethodName ?? string.Empty;

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
                                      out var customParserName))

         {
            if (string.IsNullOrEmpty(propData.MethodCall))
               propData.MethodCall = wrapperMethodName;
            continue;
         }

         propData.MethodCall = propData.MethodCall == string.Empty ? wrapperMethodName : propData.MethodCall;
         //sb.AppendLine($"// Found method candidates: wrapper: '{wrapperMethodName}', tool: '{toolMethodCall}', custom: '{customParserName}'");

         if (AddDynamicBlockParser(prop, sb, targetTypeName, customParserName, toolMethodCall))
            continue;

         if (hasEmbeddedParser)
         {
            if (!prop.IsCollection)
            {
               GenerateEmbeddedPropertyParserMethod(sb, targetTypeName, prop, customParserName!, classMetadata);
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
                                                              classMetadata);
               continue;
            }

            GenerateShatteredEmbeddedCollectionPropertyParserMethod(sb,
                                                                    targetTypeName,
                                                                    prop,
                                                                    wrapperMethodName,
                                                                    genericType,
                                                                    customParserName!,
                                                                    classMetadata);
            continue;
         }

         if ((prop.IsHashSet || prop.IsCollection) && !prop.IsShatteredList)
         {
            GenerateCollectionMethodCall(sb,
                                         prop,
                                         wrapperMethodName,
                                         toolMethodCall,
                                         genericType,
                                         targetTypeName,
                                         parsers,
                                         classMetadata);
            continue;
         }

         GeneratePropertyParserMethod(sb,
                                      targetTypeName,
                                      prop,
                                      genericType,
                                      propTypeName,
                                      wrapperMethodName,
                                      toolMethodCall,
                                      classMetadata);
      }

      sb.AppendLine("    #endregion");

      var dynamicBlockProps = properties.Where(p => p.IEu5KeyType != null && p.AstNodeType == NodeType.BlockNode).ToList();
      var dynamicContentProps = properties.Where(p => p.IEu5KeyType != null && p.AstNodeType == NodeType.ContentNode).ToList();

      DispatchGenerator.GenerateMainDispatcher(sb, propDataList, targetTypeSymbol, dynamicContentProps, dynamicBlockProps);
      DispatchGenerator.AppendIsIgnoredCheck(sb, classMetadata.IgnoredBlockKeys, classMetadata.IgnoredContentKeys);
      sb.AppendLine("}");
   }

   private static bool AddDynamicBlockParser(PropertyMetadata prop,
                                             StringBuilder sb,
                                             string targetTypeName,
                                             string? customParserName,
                                             string? toolBoxClass)
   {
      if (prop.IEu5KeyType == null)
         return false;

      var objTypSymbol = prop.IsCollection ? prop.ItemType : prop.PropertyType;
      var globalsType = prop.CustomGlobalsSource == null
                           ? prop.IEu5KeyType!.ToDisplayString()
                           : prop.CustomGlobalsSource.ToDisplayString();

      var objStr = objTypSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

      sb.AppendLine($"// ### Dynamic {prop.AstNodeType} Parser ###");

      sb.AppendLine($"    private static partial bool {PropCustomParserMethodName(prop)}({prop.AstNodeType} node, {targetTypeName} target, ref ParsingContext pc)");
      sb.AppendLine("    {");

      sb.AppendLine($"        var globals = {globalsType}.GetGlobalItems();");
      sb.AppendLine("        var keySpan = pc.SliceSource(node);");
      sb.AppendLine("        if (!globals.GetAlternateLookup<ReadOnlySpan<char>>().ContainsKey(keySpan))");
      sb.AppendLine("            return false;");

      sb.AppendLine("        var key = keySpan.ToString();");

      if (prop.AstNodeType == NodeType.ContentNode)
      {
         sb.AppendLine($"        {toolBoxClass}(node, ref pc, out var value);");
         sb.AppendLine("        if (value == null)");
         sb.AppendLine("            return false;");
      }
      else
      {
         sb.AppendLine($"        var newInstance = new {objStr}();");
         sb.AppendLine("        newInstance.UniqueId = key;");
         sb.AppendLine($"        {customParserName}.ParseProperties(node, newInstance, ref pc, false);");
         sb.AppendLine("        var value = newInstance;");
      }

      if (prop.IsCollection)
      {
         sb.AppendLine($"        if (target.{prop.PropertyName} == null)");
         sb.AppendLine($"            target.{prop.PropertyName} = new {prop.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}();");
         sb.AppendLine();
         sb.AppendLine($"        target.{prop.PropertyName}.Add(({objStr})value);");
      }
      else
         sb.AppendLine($"        target.{prop.PropertyName} = ({objStr})value;");

      sb.AppendLine("        return true;");
      sb.AppendLine("    }");
      return true;
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
                                                out string? customParserName)
   {
      if (prop.AstNodeType == NodeType.ContentNode && prop.IEu5KeyType != null)
      {
         sb.AppendLine($"// Property '{prop.PropertyName}' is a dynamic content node and will be handled by a special parser.");
         propTypeName = prop.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
         actionName = $"\"{parserSymbol.Name}.\"";
         toolMethodCall =
            FormatToolMethodCall(FindMatchingTool(toolboxSymbol,
                                                  prop,
                                                  out toolMethodCall,
                                                  out var message,
                                                  out genericType),
                                 prop,
                                 message);
         genericType = null;
         hasEmbeddedParser = false;
         wrapperMethodName = null!;
         customParserName = null;
         return false; //??? what does this return
      }

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
         return true;
      }

      // We have a pure embedded property, not in combination with a list.
      hasEmbeddedParser = TryGetEmbeddedParserName(parserSymbol,
                                                   sb,
                                                   parsers,
                                                   context,
                                                   prop,
                                                   out customParserName);
      return false;
   }

   private static void GeneratePropertyParserMethod(StringBuilder sb,
                                                    string targetTypeName,
                                                    PropertyMetadata prop,
                                                    ITypeSymbol? genericType,
                                                    string propTypeName,
                                                    string wrapperMethodName,
                                                    string toolMethodCall,
                                                    ParserClassMetadata pmc)
   {
      var outvalue = prop.IsCollection ? genericType?.ToDisplayString() ?? "object" : propTypeName;

      sb.AppendLine("// ### Property Parser ###");
      sb.AppendLine($"    private static partial bool {wrapperMethodName}({prop.AstNodeType} node, {targetTypeName} target, ref ParsingContext pc)");
      sb.AppendLine("    {");

      if (!pmc.ContainsOnlyChildObjects)
      {
         sb.AppendLine($"        if ({toolMethodCall}(node, ref pc, out {outvalue} value))");
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
      }
      else
      {
         sb.AppendLine("        // Skipped: Class is marked with [ParserFor(containsOnlyChildObjects:true)]");
         sb.AppendLine("        return true;");
      }

      sb.AppendLine("    }");
   }

   private static void GenerateShatteredEmbeddedCollectionPropertyParserMethod(StringBuilder sb,
                                                                               string targetTypeName,
                                                                               PropertyMetadata prop,
                                                                               string wrapperMethodName,
                                                                               ITypeSymbol? genericType,
                                                                               string customParserName,
                                                                               ParserClassMetadata pmc)
   {
      sb.AppendLine("// ### Embedded Shattered Collection Property Parser ###");
      var itemTypeName = genericType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";

      sb.AppendLine($"    private static partial bool {wrapperMethodName}({prop.AstNodeType} node, {targetTypeName} target, ref ParsingContext pc)");
      sb.AppendLine("    {");

      if (!pmc.ContainsOnlyChildObjects)
      {
         sb.AppendLine($"        var newInstance = new {itemTypeName}();");
         sb.AppendLine($"        {customParserName}.ParseProperties(node, newInstance, ref pc, {pmc.AllowUnknownNodes.ToString().ToLower()});");
         sb.AppendLine($"        target.{prop.PropertyName}.Add(newInstance);");
      }

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
                                                                      ParserClassMetadata pmc)
   {
      sb.AppendLine("// ### Embedded Collection Property Parser ###");
      if (prop.AstNodeType != NodeType.BlockNode)
      {
         sb.AppendLine($"    // ERROR: Embedded collection property '{prop.PropertyName}' must be parsed from a BlockNode.");
         return;
      }

      var itemTypeName = genericType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";
      var collectionKeyName = prop.IsHashSet ? "ObservableHashSet" : "Collection";

      var pdhMethodName = prop.IsEmbedded
                             ? $"ParseEmbedded{collectionKeyName}"
                             : prop.ItemNodeType switch
                             {
                                NodeType.ContentNode => $"ParseContent{collectionKeyName}",
                                NodeType.KeyOnlyNode => $"ParseKeyOnly{collectionKeyName}",
                                NodeType.BlockNode => $"ParseBlock{collectionKeyName}",
                                NodeType.StatementNode => $"ParseStatement{collectionKeyName}",
                                _ => throw new ArgumentOutOfRangeException(),
                             };
      sb.AppendLine($"    private static partial bool {wrapperMethodName}({prop.AstNodeType} node, {targetTypeName} target, ref ParsingContext pc)");
      sb.AppendLine("    {");
      if (!pmc.ContainsOnlyChildObjects)
         sb.AppendLine($"        target.{prop.PropertyName} = Pdh.{pdhMethodName}<{itemTypeName}>(node, ref pc, {customParserName}.ParseProperties, {pmc.AllowUnknownNodes.ToString().ToLower()});");
      sb.AppendLine("        return true;");
      sb.AppendLine("    }");
   }

   private static void GenerateCollectionMethodCall(StringBuilder sb,
                                                    PropertyMetadata prop,
                                                    string wrapperMethodName,
                                                    string toolMethodCall,
                                                    ITypeSymbol? genericType,
                                                    string targetTypeName,
                                                    ImmutableArray<INamedTypeSymbol> parsers,
                                                    ParserClassMetadata pmc)
   {
      sb.AppendLine("// ### Collection Property Parser ###");
      if (prop.AstNodeType != NodeType.BlockNode)
      {
         sb.AppendLine($"    // ERROR: Collection property '{prop.PropertyName}' must be parsed from a BlockNode.");
         return;
      }

      var itemTypeName = genericType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";

      var collectionType = prop.IsHashSet
                              ? "global::Arcanum.Core.CoreSystems.NUI.ObservableHashSet"
                              : "global::Arcanum.Core.CoreSystems.NUI.ObservableRangeCollection";

      var nodeCheckMethod = prop.ItemNodeType switch
      {
         NodeType.ContentNode => "IsContentNode",
         NodeType.KeyOnlyNode => "IsKeyOnlyNode",
         NodeType.BlockNode => "IsBlockNode",
         _ => "IsContentNode",
      };

      sb.AppendLine($"    private static partial bool {wrapperMethodName}({prop.AstNodeType} node, {targetTypeName} target, ref ParsingContext pc)");
      sb.AppendLine("    {");

      // Early exit if using specific custom handling
      if (pmc.ContainsOnlyChildObjects)
      {
         sb.AppendLine("        // Custom logic skipped for standard collection generation");
         sb.AppendLine("        return true;");
         sb.AppendLine("    }");
         return;
      }

      sb.AppendLine("        if (node.Children.Count > 0)");
      sb.AppendLine("        {");
      sb.AppendLine($"           var collection = new {collectionType}<{itemTypeName}>();");
      sb.AppendLine("            using var scope = pc.PushScope();");
      sb.AppendLine("            foreach (var sn in node.Children)");
      sb.AppendLine("            {");

      sb.AppendLine($"                if (!sn.{nodeCheckMethod}(ref pc, out var childNode))");
      sb.AppendLine("                    continue;");
      sb.AppendLine("");

      if (prop.ItemNodeType == NodeType.BlockNode)
      {
         sb.AppendLine($"                var item = new {itemTypeName}();");
         if (prop.IsArray)
         {
            var nestedParserSymbol = FindParserForType(prop.IsCollection ? prop.ItemType : prop.PropertyType, parsers);
            if (nestedParserSymbol == null)
            {
               sb.AppendLine($"    // ERROR: No parser with [ParserFor(typeof({prop.ItemType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}))] was found for array property '{prop.PropertyName}'.");
               sb.AppendLine("                continue;");
            }
            else
            {
               var nestedParserName = nestedParserSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
               sb.AppendLine($"                {nestedParserName}.ParseProperties(childNode, item, ref pc, {pmc.AllowUnknownNodes.ToString().ToLower()});");
            }
         }
         else
         {
            sb.AppendLine($"                if ({toolMethodCall}(childNode, item, ref pc))");
            sb.AppendLine("                {");
         }
      }
      else
      {
         sb.AppendLine($"                if ({toolMethodCall}(childNode, ref pc, out var item))");
         sb.AppendLine("                {");
      }

      GenerateAddLogic(sb, prop.IsHashSet, "collection", "item", "sn", "node", !prop.IsArray ? new(' ', 20) : new(' ', 16));
      if (!prop.IsArray)
         sb.AppendLine("                }");

      sb.AppendLine("            }"); // End foreach
      sb.AppendLine($"           target.{prop.PropertyName} = collection;");
      sb.AppendLine("        }"); // End if count > 0

      sb.AppendLine("        return true;");
      sb.AppendLine("    }");
   }

   private static void GenerateAddLogic(StringBuilder sb,
                                        bool isHashSet,
                                        string collectionVar,
                                        string itemVar,
                                        string nodeVar,
                                        string parentNodeVar,
                                        string defaultOffset = "                    ")
   {
      if (isHashSet)
      {
         sb.AppendLine($"{defaultOffset}if (!{collectionVar}.Add({itemVar}))");
         sb.AppendLine($"{defaultOffset}{{");
         sb.AppendLine($"{defaultOffset}    pc.SetContext({nodeVar});");
         sb.AppendLine($"{defaultOffset}    DiagnosticException.LogWarning(ref pc,");
         sb.AppendLine($"{defaultOffset}        ParsingError.Instance.DuplicateItemInCollection,");
         sb.AppendLine($"{defaultOffset}        pc.SliceString({nodeVar}),");
         sb.AppendLine($"{defaultOffset}        pc.SliceString({parentNodeVar}));");
         sb.AppendLine($"{defaultOffset}}}");
      }
      else
         sb.AppendLine($"                    {collectionVar}.Add({itemVar});");
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
                                                out string? customParserName)
   {
      customParserName = null;

      if (prop is { IsEmbedded: false, IEu5KeyType: null })
         return false;

      var nestedParserSymbol = FindParserForType(prop.IsCollection ? prop.ItemType : prop.PropertyType,
                                                 parsers);

      if (nestedParserSymbol == null)
      {
         var propTypeName2 = prop.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
         context.ReportDiagnostic(Diagnostic.Create(new("PG002",
                                                        "Missing Parser for Embedded Type",
                                                        $"No parser with [ParserFor(typeof({propTypeName2}))] was found for embedded property '{prop.PropertyName}' in parser '{parserSymbol.Name}'.",
                                                        "SourceGens",
                                                        DiagnosticSeverity.Error,
                                                        true),
                                                    Location.None));

         sb.AppendLine($"    // ERROR: No parser with [ParserFor(typeof({propTypeName2}))] was found for embedded property '{prop.PropertyName}', after looking for {nestedParserSymbol}");
         return true;
      }

      customParserName = nestedParserSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
      return true;
   }

   private static void GenerateEmbeddedPropertyParserMethod(StringBuilder sb,
                                                            string targetTypeName,
                                                            PropertyMetadata prop,
                                                            string customParserName,
                                                            ParserClassMetadata pmc)
   {
      sb.AppendLine("// ### Embedded Property Parser ###");
      var propTypeName2 = prop.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
      sb.AppendLine($"    private static partial bool {PropCustomParserMethodName(prop)}(BlockNode node, {targetTypeName} target, ref ParsingContext pc)");
      sb.AppendLine("    {");
      if (!pmc.ContainsOnlyChildObjects)
      {
         sb.AppendLine($"        target.{prop.PropertyName} = new {propTypeName2}();");
         sb.AppendLine($"        {customParserName}.ParseProperties(node, target.{prop.PropertyName}, ref pc, {pmc.AllowUnknownNodes.ToString().ToLower()});");
      }

      sb.AppendLine("        return true;");
      sb.AppendLine("    }");
   }

   /// <summary>
   ///    For Collections the tool method will be for the item type, e.g., <c>ArcTryParse_String</c> for
   ///    <c>List&lt;string&gt;</c> <br />
   ///    For Enums the tool method will be <c>ArcTryParse_Enum&lt;T&gt;</c> <br />
   ///    For Flags Enums the tool method will be <c>ArcTryParse_FlagsEnum&lt;T&gt;</c> <br />
   ///    For all other types the tool method will be <c>ArcTryParse_Typename</c> <br />
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
      if (prop.IEu5KeyType != null && prop.AstNodeType == NodeType.BlockNode)
      {
         wrapperMethodName = null!;
         propTypeName = null!;
         actionName = null!;
         toolMethodCall = null!;
         genericType = null;
         return false;
      }

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

      // For a generic tool, we need to specify the type argument, e.g., "ArcTryParse_Enum<MyEnum>"
      toolMethodCall = FormatToolMethodCall(toolMethod, prop, message);
      return false;
   }

   private static string FormatToolMethodCall(IMethodSymbol? toolMethod,
                                              PropertyMetadata prop,
                                              string message = "MISSING_TOOL_METHOD")
   {
      if (toolMethod == null)
         return message;

      return toolMethod.IsGenericMethod
                ? $"{toolMethod.Name}<{prop.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>"
                : toolMethod.Name;
   }

   private static INamedTypeSymbol? FindParserForType(
      ITypeSymbol targetType,
      ImmutableArray<INamedTypeSymbol> allKnownParsers)
   {
      foreach (var potentialParser in allKnownParsers)
      {
         var attr = potentialParser.GetAttributes()
                                   .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == PARSER_FOR_ATTRIBUTE);

         if (attr?.ConstructorArguments.FirstOrDefault().Value is not INamedTypeSymbol attrTargetType)
            continue;

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

         sb.AppendLine($"    private static partial bool {PropCustomParserMethodName(prop)}({prop.AstNodeType} node, {targetTypeName} target, ref ParsingContext pc);");
      }

      sb.AppendLine("    #endregion");
      sb.AppendLine();
   }

   #endregion
}