using System.Text;
using Microsoft.CodeAnalysis;

namespace ParserGenerator.HelperClasses;

public static class AgsHelper
{
   private const string OBJECT_SAVE_AS_ATTRIBUTE =
      "Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes.ObjectSaveAsAttribute";

   private const string PARSE_AS_ATTRIBUTE = "Arcanum.Core.CoreSystems.Parsing.ToolBox.ParseAsAttribute";

   private const string SAVE_AS_ATTRIBUTE = "Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes.SaveAsAttribute";
   private const string SUPPRESS_AGS_ATTRIBUTE = "Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes.SuppressAgs";

   private const string SAVING_COMMENT_PROVIDER = "Arcanum.Core.CoreSystems.SavingSystem.AGS.SavingCommentProvider";
   private const string CUSTOM_SAVING_PROVIDER = "Arcanum.Core.CoreSystems.SavingSystem.AGS.SavingActionProvider";

   public static void RunSavingGenerator(INamedTypeSymbol classSymbol, SourceProductionContext context)
   {
      var nexusProperties = Helpers.FindModifiableMembers(classSymbol, context).OfType<IPropertySymbol>().ToList();
      var objectSaveAsAttr = classSymbol.GetAttributes()
                                        .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                              OBJECT_SAVE_AS_ATTRIBUTE);

      if (objectSaveAsAttr == null)
      {
         var diagnostic = Diagnostic.Create(DefinedDiagnostics.MissingObjectSaveAsAttribute,
                                            classSymbol.Locations.FirstOrDefault(),
                                            classSymbol.Name);
         context.ReportDiagnostic(diagnostic);
         return;
      }

      if (objectSaveAsAttr.ConstructorArguments.Length < 1 ||
          objectSaveAsAttr.ConstructorArguments[0].Value is not string keyDefiningPropertyName)
      {
         var diagnostic = Diagnostic.Create(DefinedDiagnostics.InvalidObjectSaveAsAttribute,
                                            classSymbol.Locations.FirstOrDefault(),
                                            classSymbol.Name);
         context.ReportDiagnostic(diagnostic);
         return;
      }

      var propertySymbol = classSymbol.GetMembers(keyDefiningPropertyName)
                                      .OfType<IPropertySymbol>()
                                      .FirstOrDefault();

      if (propertySymbol == null)
      {
         var diagnostic = Diagnostic.Create(DefinedDiagnostics.InvalidKeyTargetProperty,
                                            classSymbol.Locations.FirstOrDefault(),
                                            classSymbol.Name);
         context.ReportDiagnostic(diagnostic);
         return;
      }

      if (propertySymbol.Type.SpecialType != SpecialType.System_String)
      {
         var diagnostic = Diagnostic.Create(DefinedDiagnostics.InvalidKeyTargetPropertyType,
                                            propertySymbol.Locations.FirstOrDefault(),
                                            propertySymbol.Name,
                                            classSymbol.Name);
         context.ReportDiagnostic(diagnostic);
         return;
      }

      var saveAsProps = new List<IPropertySymbol>();

      GetValidProperties(context, nexusProperties, saveAsProps);
      var (saverHintName, saverSource) =
         GenerateSaverClass(classSymbol, objectSaveAsAttr, saveAsProps, context);
      context.AddSource(saverHintName, saverSource);
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
            context.ReportDiagnostic(Diagnostic.Create(DefinedDiagnostics.MissingSaveAsAttributeWarning,
                                                       member.Locations.FirstOrDefault(),
                                                       member.Name));
            continue;
         }

         saveAsProps.Add(member);
      }
   }

   private static (string HintName, string Source) GenerateSaverClass(INamedTypeSymbol agsClassSymbol,
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
         context.ReportDiagnostic(Diagnostic.Create(DefinedDiagnostics.InvalidDefineKeywordAttribute,
                                                    prop.Locations.FirstOrDefault(),
                                                    prop.Name,
                                                    prop.ContainingType.Name));
         return "undefined_keyword";
      }

      return keyword!;
   }
}