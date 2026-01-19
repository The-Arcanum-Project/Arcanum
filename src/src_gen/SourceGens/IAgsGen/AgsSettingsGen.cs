#region

using Microsoft.CodeAnalysis;
using ParserGenerator.NexusGeneration;
using ParserGenerator.SubClasses;

#endregion

namespace ParserGenerator.IAgsGen;

public static class AgsSettingsGen
{
   public const string AGS_SETTINGS_NAMESPACE = "Arcanum.Core.CoreSystems.SavingSystem.AGS.AgsSettings";
   public const string COLLECTION_FORMAT_NAMESPACE = "Arcanum.Core.CoreSystems.SavingSystem.Serialization.CollectionFormatProfile";
   public const string RESET_METHOD_NAME = "ResetCollectionToDefault";

   public static INamedTypeSymbol AgsSettingsSymbol = null!;

   public static void GenerateSettingsClasses(INamedTypeSymbol cs,
                                              SourceProductionContext context,
                                              List<IPropertySymbol> nexusProperties,
                                              List<SaveAsMetadata> saveAsProps,
                                              Compilation compilation)
   {
      if (ValidateAndRetrieveSymbols(cs, context, compilation))
         return;

      var builder = new IndentBuilder();

      AppendUsings(builder, cs);
      AppendClassHeader(builder, cs);

      using (builder.Indent())
      {
         AppendConstructorCall(builder, cs);
         var profiles = CalculateProfiles(saveAsProps);
         AppendCollectionProperties(builder, profiles);
         AppendResetMethod(builder, profiles);
         AppendOverrides(builder, profiles);
      }

      AppendClassFooter(builder);

      context.AddSource($"{cs.Name}Config.AgsSettings.g.cs", builder.ToString());
   }

   private static void AppendConstructorCall(IndentBuilder builder, INamedTypeSymbol cs)
   {
      builder.AppendLine();
      builder.AppendLine($"public {cs.Name}_Config()");
      builder.AppendLine("{");
      using (builder.Indent())
      {
         var attr = cs.GetAttributes()
                      .FirstOrDefault(ad => ad.AttributeClass?.Name == "ObjectSaveAsAttribute");

         if (attr is not null)
            builder.AppendLine($"AsOneLine = {attr.ConstructorArguments[5].Value!.ToString().ToLower()};");

         builder.AppendLine("InitializeDefaults();");
      }

      builder.AppendLine("}");
      builder.AppendLine();
      builder.AppendLine("partial void InitializeDefaults();");
   }

   private static bool ValidateAndRetrieveSymbols(INamedTypeSymbol cs, SourceProductionContext context, Compilation compilation)
   {
      var agsSettingsSymbol = compilation.GetTypeByMetadataName(AGS_SETTINGS_NAMESPACE);
      if (agsSettingsSymbol is null)
      {
         context.ReportDiagnostic(Diagnostic.Create(new("STG001",
                                                        "Missing AgsSettings Class",
                                                        "The AgsSettings class is missing in the namespace '{0}'. Please define it to enable AGS settings generation.",
                                                        "AgsSettingsGen",
                                                        DiagnosticSeverity.Error,
                                                        true),
                                                    Location.None,
                                                    cs.ContainingNamespace.ToDisplayString()));
         return true;
      }

      var collectionFormatSymbol = compilation.GetTypeByMetadataName(COLLECTION_FORMAT_NAMESPACE);
      if (collectionFormatSymbol is null)
      {
         context.ReportDiagnostic(Diagnostic.Create(new("STG002",
                                                        "Missing CollectionFormatProfile Class",
                                                        "The CollectionFormatProfile class is missing in the namespace '{0}'. Please define it to enable AGS settings generation.",
                                                        "AgsSettingsGen",
                                                        DiagnosticSeverity.Error,
                                                        true),
                                                    Location.None,
                                                    cs.ContainingNamespace.ToDisplayString()));
         return true;
      }

      AgsSettingsSymbol = agsSettingsSymbol;
      return false;
   }

   private static List<GeneratedProfileData> CalculateProfiles(List<SaveAsMetadata> psms)
   {
      var results = new List<GeneratedProfileData>();
      foreach (var psm in psms)
      {
         if (!psm.IsCollection)
            continue;

         var collectionFormatAttr = psm.Prop.GetAttributes()
                                       .FirstOrDefault(ad => ad.AttributeClass?.Name == "AgsCollectionFormatAttribute");

         var cfp = CollectionDataGatherer.ParseCollectionProfile(collectionFormatAttr);

         // Apply Heuristics (optional, if you want logic like 'Embedded = Vertical')
         if (psm.IsEmbeddedObject && cfp.LayoutMode == CollectionLayoutMode.Flow)
            cfp.LayoutMode = CollectionLayoutMode.Vertical;

         results.Add(new() { PropertyName = psm.Prop.Name, Profile = cfp });
      }

      return results;
   }

   private static void AppendCollectionProperties(IndentBuilder builder, List<GeneratedProfileData> profiles)
   {
      foreach (var data in profiles)
      {
         var cfp = data.Profile;

         // Point to the single shared reset method
         builder.AppendLine($"[CustomResetMethod(nameof({RESET_METHOD_NAME}))]");
         builder.AppendLine($"public CollectionFormatProfile {data.PropertyName} {{ get; set; }} = new ()");
         builder.AppendLine("{");
         using (builder.Indent())
            AppendProfileInitializers(builder, cfp);
         builder.AppendLine("};");
         builder.AppendLine();
      }
   }

   private static void AppendResetMethod(IndentBuilder builder, List<GeneratedProfileData> profiles)
   {
      // Signature matches your reflection requirement: object Method(PropertyInfo)
      builder.AppendLine($"public object {RESET_METHOD_NAME}(System.Reflection.PropertyInfo info)");
      builder.AppendLine("{");
      using (builder.Indent())
      {
         builder.AppendLine("return info.Name switch");
         builder.AppendLine("{");
         using (builder.Indent())
         {
            foreach (var data in profiles)
            {
               builder.AppendLine($"\"{data.PropertyName}\" => new CollectionFormatProfile");
               builder.AppendLine("{");
               using (builder.Indent())
                  AppendProfileInitializers(builder, data.Profile);
               builder.AppendLine("},");
            }

            // Default fallback
            builder.AppendLine("_ => new CollectionFormatProfile()");
         }

         builder.AppendLine("};");
      }

      builder.AppendLine("}");
      builder.AppendLine();
   }

   private static void AppendProfileInitializers(IndentBuilder builder, CollectionFormatProfile cfp)
   {
      builder.AppendLine($"LayoutMode = CollectionLayoutMode.{cfp.LayoutMode},");
      builder.AppendLine($"ItemsPerRow = {cfp.ItemsPerRow},");
      builder.AppendLine($"AlignColumns = {cfp.AlignColumns.ToString().ToLower()},");
      builder.AppendLine($"ColumnWidth = {cfp.ColumnWidth},");
      builder.AppendLine($"SortMode = CollectionSortMode.{cfp.SortMode},");
      builder.AppendLine($"WriteEmpty = {cfp.WriteEmpty.ToString().ToLower()},");
   }

   private static void AppendOverrides(IndentBuilder builder, List<GeneratedProfileData> profiles)
   {
      builder.AppendLine("public override CollectionFormatProfile GetCollectionProfile(Enum prop) => prop.ToString() switch");
      builder.AppendLine("{");
      using (builder.Indent())
      {
         foreach (var p in profiles)
            builder.AppendLine($"\"{p.PropertyName}\" => {p.PropertyName},");

         builder.AppendLine("_ => base.GetCollectionProfile(prop),");
      }

      builder.AppendLine("};");
   }

   private static void AppendClassFooter(IndentBuilder builder)
   {
      builder.AppendLine("}");
   }

   private static void AppendClassHeader(IndentBuilder builder, INamedTypeSymbol cs)
   {
      builder.AppendLine();
      builder.AppendLine($"public partial class {cs.Name}_Config : {AgsSettingsSymbol.ToDisplayString()}");
      builder.AppendLine("{");
   }

   private static void AppendUsings(IndentBuilder builder, INamedTypeSymbol cs)
   {
      var namespaceName = cs.ContainingNamespace.ToDisplayString();

      builder.AppendLine("// <auto-generated/>");
      builder.AppendLine("#nullable enable");
      builder.AppendLine();
      builder.AppendLine("using Arcanum.Core.CoreSystems.SavingSystem.Serialization;");
      builder.AppendLine("using Arcanum.API.Attributes;");

      builder.AppendLine();
      builder.AppendLine($"namespace {namespaceName};");
   }

   private struct GeneratedProfileData
   {
      public string PropertyName;
      public CollectionFormatProfile Profile;
   }
}