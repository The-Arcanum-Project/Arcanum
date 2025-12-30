using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ParserGenerator.NexusGeneration;

public static class DataGatherer
{
   private const string INEXUS_INTERFACE = "Nexus.Core.INexus";
   private const string AGGREGATE_LINK_CLASS_NAME = "Arcanum.Core.Utils.DataStructures.AggregateLink`1";

   private static INamedTypeSymbol? _aggregateLinkSymbol;

   public static List<NexusPropertyData> CreateNexusPropertyDataList(INamedTypeSymbol cs,
                                                                     SourceProductionContext context,
                                                                     INamedTypeSymbol enumerableSymbol,
                                                                     INamedTypeSymbol ieu5ObjectSymbol,
                                                                     INamedTypeSymbol iListSymbol)
   {
      var npds = new List<NexusPropertyData>();

      foreach (var member in Helpers.FindModifiableMembers(cs, context))
      {
         if (member is not IPropertySymbol propertySymbol)
            continue;

         npds.Add(new(member, propertySymbol, cs, context, ieu5ObjectSymbol));
      }

      return npds;
   }

   /// <summary>
   /// This transform finds any class that implements INexus.
   /// It's the gatekeeper for both generation steps.
   /// </summary>
   public static INamedTypeSymbol? GetNexusClassSymbol(GeneratorSyntaxContext context, CancellationToken token)
   {
      var classDeclaration = (ClassDeclarationSyntax)context.Node;

      if (context.SemanticModel.GetDeclaredSymbol(classDeclaration, token) is INamedTypeSymbol
          classSymbol)
      {
         var nexusInterface = context.SemanticModel.Compilation.GetTypeByMetadataName(INEXUS_INTERFACE);
         if (nexusInterface != null &&
             classSymbol.AllInterfaces.Contains(nexusInterface, SymbolEqualityComparer.Default))
            return classSymbol;
      }

      _aggregateLinkSymbol ??= context.SemanticModel.Compilation.GetTypeByMetadataName(AGGREGATE_LINK_CLASS_NAME);

      return null;
   }
}