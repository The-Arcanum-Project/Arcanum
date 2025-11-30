using System.Text;
using Microsoft.CodeAnalysis;
using ParserGenerator.SubClasses;

namespace ParserGenerator.HelperClasses;

public static class AgsHelper
{
   private const string OBJECT_SAVE_AS_ATTRIBUTE =
      "Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes.ObjectSaveAsAttribute";

   private const string PARSE_AS_ATTRIBUTE = "Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox.ParseAsAttribute";

   private const string PARSE_AS_EMBEDDED_ATTRIBUTE =
      "Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox.ParseAsEmbeddedAttribute";

   private const string SAVE_AS_ATTRIBUTE = "Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes.SaveAsAttribute";
   private const string SUPPRESS_AGS_ATTRIBUTE = "Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes.SuppressAgs";
   private const string ENUM_AGS_DATA_ATTRIBUTE = "Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes.EnumAgsData";

   private const string SAVING_COMMENT_PROVIDER = "Arcanum.Core.CoreSystems.SavingSystem.AGS.SavingCommentProvider";
   private const string CUSTOM_SAVING_PROVIDER = "Arcanum.Core.CoreSystems.SavingSystem.AGS.SavingActionProvider";

   public static Dictionary<string, EnumAnalysisResult> EnumAnalysisCache = new();

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

      var saveAsProps = new List<SaveAsMetadata>();

      GetValidProperties(context, nexusProperties, saveAsProps, classSymbol);
      var (saverHintName, saverSource) =
         GenerateSaverClass(classSymbol, objectSaveAsAttr, saveAsProps, context);

      AnalyzeAndFilterProperties(nexusProperties, context);

      context.AddSource(saverHintName, saverSource);
   }

   private static void GetValidProperties(SourceProductionContext context,
                                          List<IPropertySymbol> nexusProperties,
                                          List<SaveAsMetadata> saveAsProps,
                                          INamedTypeSymbol classSymbol)
   {
      foreach (var member in nexusProperties)
      {
         if (member is null)
            continue;

         var (suppressAttr, saveAsAttr) = FindEffectiveAttributes(classSymbol, member.Name);

         // If [SuppressAgs] is present, silently skip this property.
         if (suppressAttr != null)
            continue;

         if (saveAsAttr == null)
         {
            context.ReportDiagnostic(Diagnostic.Create(DefinedDiagnostics.MissingSaveAsAttributeWarning,
                                                       member.Locations.FirstOrDefault(),
                                                       member.Name));
            continue;
         }

         try
         {
            saveAsProps.Add(new(member, saveAsAttr));
         }
         catch (Exception e)
         {
            context.ReportDiagnostic(Diagnostic.Create(DefinedDiagnostics.InvalidSaveAsAttribute,
                                                       member.Locations.FirstOrDefault(),
                                                       member.Name,
                                                       e.Message,
                                                       e.GetType().Name,
                                                       e.StackTrace));
         }
      }
   }

   /// <summary>
   /// For a given member name, walks up the type hierarchy of a class (including interfaces)
   /// to find the most-derived declaration and its [IgnoreModifiable] or [AddModifiable] attributes.
   /// </summary>
   private static (AttributeData? Ignore, AttributeData? Add) FindEffectiveAttributes(
      INamedTypeSymbol classSymbol,
      string memberName)
   {
      AttributeData? suppressAttr = null;
      AttributeData? saveAsAttribute = null;

      // Search the class and its base classes first (most-derived wins)
      var currentType = classSymbol;
      while (currentType != null && currentType.SpecialType != SpecialType.System_Object)
      {
         var memberOnType = currentType.GetMembers(memberName).FirstOrDefault();
         if (memberOnType != null)
         {
            // If we haven't found an attribute yet, check this level.
            // This correctly gives precedence to attributes on derived classes.
            suppressAttr ??= memberOnType.GetAttributes()
                                         .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                               SUPPRESS_AGS_ATTRIBUTE);
            saveAsAttribute ??= memberOnType.GetAttributes()
                                            .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                                  SAVE_AS_ATTRIBUTE);
         }

         currentType = currentType.BaseType;
      }

      // Only if we haven't found attributes yet, check the interfaces.
      // This ensures class attributes always override interface attributes.
      if (suppressAttr == null && saveAsAttribute == null)
         foreach (var iface in classSymbol.AllInterfaces)
         {
            var memberOnIface = iface.GetMembers(memberName).FirstOrDefault();
            if (memberOnIface != null)
            {
               suppressAttr ??= memberOnIface.GetAttributes()
                                             .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                                   SUPPRESS_AGS_ATTRIBUTE);
               saveAsAttribute ??= memberOnIface.GetAttributes()
                                                .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                                      SAVE_AS_ATTRIBUTE);
            }
         }

      return (suppressAttr, saveAsAttribute);
   }

   private static (string HintName, string Source) GenerateSaverClass(INamedTypeSymbol agsClassSymbol,
                                                                      AttributeData objectSaveAsAttr,
                                                                      List<SaveAsMetadata> nexusProperties,
                                                                      SourceProductionContext context)
   {
      var className = agsClassSymbol.Name;
      var hintName = $"{agsClassSymbol.ContainingNamespace}.{className}Ags.g.cs";
      var namespaceName = agsClassSymbol.ToDisplayString();

      var sb = new StringBuilder();
      sb.AppendLine("// <auto-generated/>");
      sb.AppendLine("#nullable enable");
      sb.AppendLine("using System;");
      sb.AppendLine("using System.Collections.Generic;");
      sb.AppendLine("using System.Linq;");
      sb.AppendLine("using System.Collections;");
      sb.AppendLine("using System.ComponentModel;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.SavingSystem.AGS;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;");
      sb.AppendLine("using Arcanum.Core.CoreSystems.Common;");
      sb.AppendLine($"using {agsClassSymbol.ContainingNamespace.ToDisplayString()};");
      sb.AppendLine();
      sb.AppendLine($"namespace {agsClassSymbol.ContainingNamespace.ToDisplayString()}");
      sb.AppendLine("{");
      sb.AppendLine($"    public partial class {className}");
      sb.AppendLine("    {");
      sb.AppendLine("        private static readonly IReadOnlyList<PropertySavingMetadata> _allProperties;");
      sb.AppendLine();
      sb.AppendLine($"        static {className}()");
      sb.AppendLine("        {");
      sb.AppendLine("            _allProperties = new List<PropertySavingMetadata>");
      sb.AppendLine("            {");

      foreach (var prop in nexusProperties)
         GenerateMetadataEntry(sb, prop, context, namespaceName);

      sb.AppendLine("            };");
      sb.AppendLine("        }");

      // --- The private static fields ---
      sb.AppendLine("        // Pre-built metadata for the class itself.");
      sb.AppendLine("        private static readonly ClassSavingMetadata _classMetadata = new(");
      sb.AppendLine($"            TokenType.{Helpers.GetEnumMemberName(objectSaveAsAttr.ConstructorArguments[0])},");
      sb.AppendLine($"            TokenType.{Helpers.GetEnumMemberName(objectSaveAsAttr.ConstructorArguments[1])},");
      sb.AppendLine($"            TokenType.{Helpers.GetEnumMemberName(objectSaveAsAttr.ConstructorArguments[2])},");
      sb.AppendLine($"            {GetNullOrString(objectSaveAsAttr.ConstructorArguments[4], SAVING_COMMENT_PROVIDER)},");
      sb.AppendLine($"            {GetNullOrString(objectSaveAsAttr.ConstructorArguments[3], CUSTOM_SAVING_PROVIDER)}");
      sb.AppendLine($"        );");
      sb.AppendLine();

      sb.AppendLine("        public ClassSavingMetadata ClassMetadata => _classMetadata;");
      sb.AppendLine();

      // --- The public accessor to the list ---
      sb.AppendLine();
      sb.AppendLine("        public IReadOnlyList<PropertySavingMetadata> SaveableProps => _allProperties;");
      sb.AppendLine("    }");
      sb.AppendLine("}");

      return (hintName, sb.ToString());
   }

   private static void GenerateMetadataEntry(StringBuilder sb,
                                             SaveAsMetadata prop,
                                             SourceProductionContext context,
                                             string namespaceName)
   {
      var keyword = GetPropertyMetadata(prop.Prop, context);

      if (prop.DefaultValueAttribute == null &&
          !prop.Prop.Type.AllInterfaces.Any(i => i.ToDisplayString() == UniGen.IAGS_INTERFACE) &&
          !prop.IsCollection)
         context.ReportDiagnostic(Diagnostic.Create(DefinedDiagnostics.MissingDefaultValueAttributeWarning,
                                                    prop.Prop.Locations.FirstOrDefault(),
                                                    prop.Prop.Name,
                                                    prop.Prop.ContainingType.Name));

      var defaultValueLiteral = "null";
      if (prop.DefaultValueAttribute != null && prop.DefaultValueAttribute.ConstructorArguments.Any())
         defaultValueLiteral =
            Helpers.FormatDefaultValueLiteral(prop.DefaultValueAttribute.ConstructorArguments[0]);

      sb.AppendLine("                new()");
      sb.AppendLine("                {");
      sb.AppendLine($"                    NxProp = {namespaceName}.Field.{prop.Prop.Name},");
      sb.AppendLine($"                    Keyword = \"{keyword}\",");
      sb.AppendLine($"                    CommentProvider = {prop.CommentMethod},");
      sb.AppendLine($"                    SavingMethod = {prop.SavingMethod},");
      sb.AppendLine($"                    ValueType = SavingValueType.{prop.ValueType},");
      sb.AppendLine($"                    DefaultValue = {defaultValueLiteral},");
      sb.AppendLine($"                    IsShattered = {prop.IsShattered.ToString().ToLowerInvariant()},");
      sb.AppendLine($"                    Separator = TokenType.{prop.Separator},");
      sb.AppendLine($"                    CollectionItemKeyProvider = {prop.CollectionKeyMethod},");
      sb.AppendLine($"                    IsCollection = {prop.IsCollection.ToString().ToLowerInvariant()},");
      sb.AppendLine($"                    CollectionSeparator = {prop.CollectionSeparator},");
      sb.AppendLine($"                    SaveEmbeddedAsIdentifier = {prop.SaveEmbeddedAsIdentifier.ToString().ToLowerInvariant()},");
      sb.AppendLine($"                    CollectionAsPureIdentifierList = {prop.CollectionAsPureIdentifierList.ToString().ToLowerInvariant()},");
      sb.AppendLine($"                    IsEmbeddedObject = {prop.IsEmbeddedObject.ToString().ToLowerInvariant()},");
      sb.AppendLine($"                    NumOfDecimalPlaces = {prop.NumOfDecimalPlaces.ToString()},");
      sb.AppendLine("                },");
   }

   private static string GetNullOrString(TypedConstant value, string providerName)
   {
      var bv = value.Value as string;
      return string.IsNullOrEmpty(bv) ? "null" : $"{providerName}.{bv}";
   }

   private static string GetPropertyMetadata(IPropertySymbol prop, SourceProductionContext context)
   {
      var parseAsAttr = prop.GetAttributes()
                            .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == PARSE_AS_ATTRIBUTE);
      string keyword;
      if (parseAsAttr == null)
      {
         var parseEmbeddedAttr = prop.GetAttributes()
                                     .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                           PARSE_AS_EMBEDDED_ATTRIBUTE);
         if (parseEmbeddedAttr == null)
         {
            context.ReportDiagnostic(Diagnostic.Create(DefinedDiagnostics.MissingParseAsAttribute,
                                                       prop.Locations.FirstOrDefault(),
                                                       prop.Name,
                                                       prop.ContainingType.Name));
            return "undefined_keyword";
         }

         keyword = parseEmbeddedAttr.ConstructorArguments[0].Value as string ?? "undefined_keyword";
      }
      else
         keyword = (parseAsAttr.ConstructorArguments[0].Value as string)!;

      // Report diagnostics if keyword is null or empty
      if (string.IsNullOrEmpty(keyword))
      {
         context.ReportDiagnostic(Diagnostic.Create(DefinedDiagnostics.InvalidDefineKeywordAttribute,
                                                    prop.Locations.FirstOrDefault(),
                                                    prop.Name,
                                                    prop.ContainingType.Name));
         return "undefined_keyword";
      }

      return keyword;
   }

   // This method is now called PER-CLASS, and it manages its own state.
   private static void AnalyzeAndFilterProperties(List<IPropertySymbol> allProperties, SourceProductionContext context)
   {
      // Create a temporary cache that ONLY lives for the analysis of this one class.
      var analysisCacheForThisClass = new Dictionary<string, EnumAnalysisResult>();

      foreach (var prop in allProperties)
      {
         if (prop is null || prop.Type.TypeKind != TypeKind.Enum)
            continue;

         var enumTypeSymbol = (INamedTypeSymbol)prop.Type;
         var enumFullName = enumTypeSymbol.ToDisplayString();

         // Use the temporary, per-class cache.
         if (analysisCacheForThisClass.ContainsKey(enumFullName))
            continue;

         // --- This is a new enum for this class, analyze it fully. ---
         var fieldResults = new List<EnumFieldAnalysisResult>();
         var isEnumOverallValid = true;

         var enumFields = enumTypeSymbol.GetMembers().OfType<IFieldSymbol>().ToList();

         foreach (var fieldSymbol in enumFields)
         {
            var enumAgsDataAttr = fieldSymbol.GetAttributeForKey(ENUM_AGS_DATA_ATTRIBUTE);

            if (enumAgsDataAttr == null)
            {
               isEnumOverallValid = false; // Mark the whole enum as invalid.
               fieldResults.Add(new(fieldSymbol, false, "INVALID", false));
            }
            else
            {
               var key = enumAgsDataAttr.ConstructorArguments[0].Value as string ?? fieldSymbol.Name.ToSnakeCase();
               var isIgnored = (bool)(enumAgsDataAttr.ConstructorArguments[1].Value ?? false);
               fieldResults.Add(new(fieldSymbol, true, key, isIgnored));
            }
         }

         var analysisResult = new EnumAnalysisResult(enumTypeSymbol, isEnumOverallValid, fieldResults);
         analysisCacheForThisClass[enumFullName] = analysisResult;

         // --- After a full analysis, if the enum is invalid, report ALL its errors at once. ---
         if (!analysisResult.IsValid)
         {
            foreach (var fieldResult in analysisResult.FieldResults.Where(fr => !fr.IsValid))
               context.ReportDiagnostic(Diagnostic.Create(DefinedDiagnostics.MissingEnumAgsDataAttribute,
                                                          fieldResult.FieldSymbol.Locations.FirstOrDefault(),
                                                          enumTypeSymbol.Name,
                                                          fieldResult.FieldSymbol.Name));
         }
         else
         {
            if (!EnumAnalysisCache.ContainsKey(enumFullName))
               EnumAnalysisCache[enumFullName] = analysisResult;
         }
      }
   }
}