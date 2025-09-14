using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ParserGenerator;

[Generator]
public class SavingGenerator : IIncrementalGenerator
{
   // --- Constants for all Attributes and Provider classes ---
   private const string AGS_INTERFACE = "Arcanum.Core.CoreSystems.SavingSystem.AGS.IAgs";
   private const string PARSE_AS_ATTRIBUTE = "Arcanum.Core.CoreSystems.Parsing.ToolBox.ParseAsAttribute";

   private const string SAVE_AS_ATTRIBUTE = "Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes.SaveAsAttribute";
   private const string SUPPRESS_AGS_ATTRIBUTE = "Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes.SuppressAgs";

   private const string SAVING_COMMENT_PROVIDER = "Arcanum.Core.CoreSystems.SavingSystem.AGS.SavingCommentProvider";
   private const string CUSTOM_SAVING_PROVIDER = "Arcanum.Core.CoreSystems.SavingSystem.AGS.SavingActionProvider";

   private const string OBJECT_SAVE_AS_ATTRIBUTE_NAME = "ObjectSaveAsAttribute";

   private static readonly DiagnosticDescriptor MissingSaveAsAttributeWarning = new(id: "AGS004",
       title: "Missing [SaveAs] attribute",
       messageFormat:
       "Property '{0}' will not be saved because it is missing a [SaveAs] attribute. Add the attribute to include it in serialization, or add [SuppressAgs] to explicitly ignore it.",
       category: "SavingGenerator",
       DiagnosticSeverity.Warning, // This is a warning, not an error.
       isEnabledByDefault: true);

   // An error for a missing keyword, which is unrecoverable.
   private static readonly DiagnosticDescriptor MissingParseAsKeywordError = new(id: "AGS002",
       title: "Invalid or Missing ParseAs Keyword",
       messageFormat:
       "Property '{0}' cannot be saved because it is missing a [ParseAs] attribute with a valid keyword.",
       category: "SavingGenerator",
       DiagnosticSeverity.Error,
       isEnabledByDefault: true);

   private static readonly DiagnosticDescriptor MissingNexusEnumPropertyKey = new(id: "AGS006",
       title:
       "Missing Nexus Enum Property Key",
       messageFormat:
       "The property '{0}' corresponding to enum field '{1}' was not found in class '{2}'. Ensure that the property exists and matches the enum field name.",
       category: "SavingGenerator",
       DiagnosticSeverity
         .Error, // This should be a build-breaking error.
       isEnabledByDefault: true);

   private static readonly DiagnosticDescriptor InvalidNexusKeyPropType = new("AGS010",
                                                                              "Incorrect Key Property Type",
                                                                              "The key-defining property '{0}' specified in [ObjectSaveAs] must be of type 'string', but was found to be of type '{1}'.",
                                                                              "SavingGenerator",
                                                                              DiagnosticSeverity.Error,
                                                                              true);

   public void Initialize(IncrementalGeneratorInitializationContext context)
   {
      // The pipeline to find all classes that implement our target IAgs interface.
      var provider = context.SyntaxProvider.CreateSyntaxProvider(predicate: (node, _) => node is ClassDeclarationSyntax,
                                                                 transform: GetAgsClassSymbol)
                            .Where(s => s is not null);

      context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
                                   (spc, source) => Execute(source.Right!, spc));
   }

   private static INamedTypeSymbol? GetAgsClassSymbol(GeneratorSyntaxContext context, CancellationToken token)
   {
      var classDeclaration = (ClassDeclarationSyntax)context.Node;

      if (context.SemanticModel.GetDeclaredSymbol(classDeclaration, token) is not INamedTypeSymbol classSymbol)
         return null;

      var agsInterface = context.SemanticModel.Compilation.GetTypeByMetadataName(AGS_INTERFACE);
      if (agsInterface != null && classSymbol.AllInterfaces.Contains(agsInterface, SymbolEqualityComparer.Default))
         return classSymbol;

      return null;
   }

   private void Execute(ImmutableArray<INamedTypeSymbol> agsClasses,
                        SourceProductionContext context)
   {
      if (agsClasses.IsDefaultOrEmpty)
         return;

      foreach (var agsClassSymbol in agsClasses.Distinct(SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>())
      {
         AttributeData? objectSaveAsAttr = null;

         foreach (var attribute in agsClassSymbol.GetAttributes())
         {
            // Get the symbol for the attribute class
            if (attribute.AttributeClass is not { } attributeSymbol)
               continue;

            // Check if it's a generic type and its name matches
            if (attributeSymbol is { IsGenericType: true, Name: OBJECT_SAVE_AS_ATTRIBUTE_NAME })
            {
               objectSaveAsAttr = attribute;
               break;
            }
         }

         // If the attribute is MISSING, report a diagnostic and skip this class.
         if (objectSaveAsAttr == null)
         {
            context.ReportDiagnostic(Diagnostic.Create(MissingParseAsKeywordError,
                                                       agsClassSymbol.Locations.FirstOrDefault(),
                                                       agsClassSymbol.Name));
            continue;
         }

         var keyWordArg = objectSaveAsAttr.ConstructorArguments[0];
         var keyDefiningPropertyName = Helpers.GetEnumMemberName(keyWordArg);
         var propertySymbol = agsClassSymbol.GetMembers(keyDefiningPropertyName!)
                                            .OfType<IPropertySymbol>()
                                            .FirstOrDefault();

         if (propertySymbol == null)
         {
            context.ReportDiagnostic(Diagnostic.Create(MissingNexusEnumPropertyKey,
                                                       agsClassSymbol.Locations.FirstOrDefault(),
                                                       keyDefiningPropertyName,
                                                       keyDefiningPropertyName,
                                                       agsClassSymbol.Name));
            continue;
         }

         if (propertySymbol.Type.SpecialType != SpecialType.System_String)
         {
            context.ReportDiagnostic(Diagnostic.Create(InvalidNexusKeyPropType,
                                                       propertySymbol.Locations.FirstOrDefault(),
                                                       propertySymbol.Name,
                                                       propertySymbol.Type.Name));
            continue;
         }

         var nexusMembers = Helpers.FindModifiableMembers(agsClassSymbol, context);
         var nexusProperties = nexusMembers.OfType<IPropertySymbol>().ToList();
         var saveAsProps = new List<IPropertySymbol>();

         GetValidProperties(context, nexusProperties, saveAsProps);

         var (saverHintName, saverSource) =
            GenerateSaverClass(agsClassSymbol, objectSaveAsAttr, saveAsProps, context);
         context.AddSource(saverHintName, saverSource);
      }
   }

   private static void GetValidProperties(SourceProductionContext context,
                                          List<IPropertySymbol> nexusProperties,
                                          List<IPropertySymbol> saveAsProps)
   {
      foreach (var member in nexusProperties)
      {
         if (member is null)
            continue;

         // If [SuppressAgs] is present, silently skip this property.
         if (member.GetAttributes().Any(ad => ad.AttributeClass?.ToDisplayString() == SUPPRESS_AGS_ATTRIBUTE))
            continue;

         // The [SaveAs] attribute is required.
         var saveAsAttr = member.GetAttributes()
                                .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == SAVE_AS_ATTRIBUTE);
         if (saveAsAttr == null)
         {
            context.ReportDiagnostic(Diagnostic.Create(MissingSaveAsAttributeWarning,
                                                       member.Locations.FirstOrDefault(),
                                                       member.Name));
            continue;
         }

         saveAsProps.Add(member);
      }
   }

   private (string HintName, string Source) GenerateSaverClass(INamedTypeSymbol agsClassSymbol,
                                                               AttributeData objectSaveAsAttr,
                                                               List<IPropertySymbol> nexusProperties,
                                                               SourceProductionContext context)
   {
      var className = agsClassSymbol.Name;
      var hintName = $"{agsClassSymbol.ContainingNamespace}.{className}Ags.g.cs";
      var namespaceName = agsClassSymbol.ContainingNamespace.ToDisplayString();

      var sb = new StringBuilder();
      sb.AppendLine("// <auto-generated/>");
      sb.AppendLine("#nullable enable");
      sb.AppendLine("using System;");
      sb.AppendLine("using System.Collections.Generic;");
      sb.AppendLine("using System.Linq;");
      sb.AppendLine("using System.Collections;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.SavingSystem.AGS;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.Common;");
      sb.AppendLine($"using {agsClassSymbol.ContainingNamespace.ToDisplayString()};");
      sb.AppendLine();
      sb.AppendLine($"namespace {agsClassSymbol.ContainingNamespace.ToDisplayString()}");
      sb.AppendLine("{");
      sb.AppendLine($"    public partial class {className}");
      sb.AppendLine("    {");
      sb.AppendLine("        private static readonly IReadOnlyList<PropertySavingMetaData> _allProperties;");
      sb.AppendLine();
      sb.AppendLine($"        static {className}()");
      sb.AppendLine("        {");
      sb.AppendLine("            _allProperties = new List<PropertySavingMetaData>");
      sb.AppendLine("            {");

      foreach (var prop in nexusProperties)
         GenerateMetadataEntry(sb, prop, context, namespaceName);

      sb.AppendLine("            };");
      sb.AppendLine("        }");

      // --- The private static fields ---
      sb.AppendLine("        // Pre-built metadata for the class itself.");
      sb.AppendLine("        private static readonly ClassSavingMetadata _classMetadata = new(");
      sb.AppendLine($"            {namespaceName}.{className}.Field.{objectSaveAsAttr.ConstructorArguments[0].Value},");
      sb.AppendLine($"            TokenType.{Helpers.GetEnumMemberName(objectSaveAsAttr.ConstructorArguments[1])},");
      sb.AppendLine($"            TokenType.{Helpers.GetEnumMemberName(objectSaveAsAttr.ConstructorArguments[2])},");
      sb.AppendLine($"            TokenType.{Helpers.GetEnumMemberName(objectSaveAsAttr.ConstructorArguments[3])},");
      sb.AppendLine($"            {GetNullOrString(objectSaveAsAttr.ConstructorArguments[4], CUSTOM_SAVING_PROVIDER)},");
      sb.AppendLine($"            {GetNullOrString(objectSaveAsAttr.ConstructorArguments[5], SAVING_COMMENT_PROVIDER)}");
      sb.AppendLine($"        );");
      sb.AppendLine();

      sb.AppendLine("        public static ClassSavingMetadata ClassMetadata => _classMetadata;");
      sb.AppendLine();

      // --- The public accessor to the list ---
      sb.AppendLine();
      sb.AppendLine("        public IReadOnlyList<PropertySavingMetaData> SaveableProps => _allProperties;");
      sb.AppendLine("    }");
      sb.AppendLine("}");

      return (hintName, sb.ToString());
   }

   private static void GenerateMetadataEntry(StringBuilder sb,
                                             IPropertySymbol prop,
                                             SourceProductionContext context,
                                             string namespaceName)
   {
      var keyword = GetPropertyMetadata(prop, context);

      if (prop.GetAttributes().Any(ad => ad.AttributeClass?.ToDisplayString() == SUPPRESS_AGS_ATTRIBUTE))
         return;

      var saveAs = prop.GetAttributes()
                       .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == SAVE_AS_ATTRIBUTE);

      // Report diagnostics if SaveAs is missing
      if (saveAs == null)
      {
         context.ReportDiagnostic(Diagnostic.Create(new(id: "AGS001",
                                                        title: "Missing SaveAs Attribute",
                                                        messageFormat:
                                                        "Property '{0}' in class '{1}' is missing the required [SaveAs] attribute.",
                                                        category: "SavingGenerator",
                                                        DiagnosticSeverity.Error,
                                                        isEnabledByDefault: true),
                                                    prop.Locations.FirstOrDefault(),
                                                    prop.Name,
                                                    prop.ContainingType.Name));
         return;
      }

      sb.AppendLine("                new()");
      sb.AppendLine("                {");
      sb.AppendLine($"                    NxProp = {namespaceName}.{prop.ContainingType.Name}.Field.{prop.Name},");
      sb.AppendLine($"                    Keyword = \"{keyword}\",");
      sb.AppendLine($"                    CommentProvider = {GetNullOrString(saveAs.ConstructorArguments[2], SAVING_COMMENT_PROVIDER)},");
      sb.AppendLine($"                    SavingMethod = {GetNullOrString(saveAs.ConstructorArguments[3], CUSTOM_SAVING_PROVIDER)},");
      sb.AppendLine($"                    ValueType = SavingValueType.{Helpers.GetEnumMemberName(saveAs.ConstructorArguments[0])},");
      sb.AppendLine($"                    Separator = TokenType.{Helpers.GetEnumMemberName(saveAs.ConstructorArguments[1])},");

      sb.AppendLine("                },");
   }

   private static string GetNullOrString(TypedConstant value, string providerName)
   {
      var bv = value.Value as string;
      return string.IsNullOrEmpty(bv) ? "null" : $"{providerName}.{bv}";
   }

   private static string GetPropertyMetadata(IPropertySymbol prop, SourceProductionContext context)
   {
      var parseAsAttr = prop.GetAttributes().First(ad => ad.AttributeClass?.ToDisplayString() == PARSE_AS_ATTRIBUTE);
      var keyword = parseAsAttr.ConstructorArguments[1].Value as string;
      // Report diagnostics if keyword is null or empty
      if (string.IsNullOrEmpty(keyword))
      {
         context.ReportDiagnostic(Diagnostic.Create(new(id: "AGS002",
                                                        title: "Invalid ParseAs Keyword",
                                                        messageFormat:
                                                        "Property '{0}' in class '{1}' has an invalid or empty keyword in its [ParseAs] attribute.",
                                                        category: "SavingGenerator",
                                                        DiagnosticSeverity.Error,
                                                        isEnabledByDefault: true),
                                                    prop.Locations.FirstOrDefault(),
                                                    prop.Name,
                                                    prop.ContainingType.Name));
         return "undefined_keyword";
      }

      return keyword!;
   }
}