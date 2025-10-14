using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ParserGenerator.HelperClasses;
using ParserGenerator.SubClasses;

namespace ParserGenerator;

[Generator]
public class MapInferableSourceGen : IIncrementalGenerator
{
   public const string MAP_INFERABLE_GENERIC_INTERFACE = "Arcanum.Core.CoreSystems.NUI.IMapInferable`1";

   public void Initialize(IncrementalGeneratorInitializationContext context)
   {
      // Find all class declarations in the compilation.
      var classProvider = Helpers.CreateClassSyntaxProvider(context);

      context.RegisterSourceOutput(context.CompilationProvider.Combine(classProvider),
                                   (spc, source) => { Generate(source.Left, source.Right, spc); });
   }

   private static void Generate(Compilation compilation,
                                ImmutableArray<ClassDeclarationSyntax> classes,
                                SourceProductionContext context)
   {
      if (classes.IsDefaultOrEmpty)
         return;

      var foundTypes = Helpers.FindTypesImplementingInterface(compilation, classes, MAP_INFERABLE_GENERIC_INTERFACE);
      if (foundTypes.Count == 0)
         return;

      var config = new DispatcherConfig
      {
         Namespace = "Arcanum.Core.Registry",
         ClassName = "MapInferrableRegistry",
         DelegateDefinition = "private delegate IList Accessor(List<Location> sLocs);",
         RuntimeMethodName = "GetInferredList",
         CompileTimeValueFactory = symbol =>
            $"(sLocs) => {symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.GetInferredList(sLocs)",
         Delegate2Definition = "private delegate List<Location> ObjToMapAccessor(IList items);",
         DelegateName2 = "ObjToMapAccessor",
         CompileTimeValueFactoryForSecondMethod = symbol =>
            $"(items) => {symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.GetRelevantLocations(items)",
         PublicApiMethods = """

                                /// <summary>
                                /// This retrieves a list of items of type T based on the selection.
                                /// </summary>
                                public static IList? GetInferredList(Type type, List<Location> sLocs)
                                {
                                    if (_dispatchers.TryGetValue(type, out var accessor))
                                        return accessor(sLocs);
                                    return null;
                                }
                                
                                /// <summary>
                                /// Returns a list of locations relevant to the provided items of type T.
                                /// </summary>
                                public static List<Location> GetRelevantLocations(Type type, IList items)
                                {
                                    if (_dispatchers2.TryGetValue(type, out var accessor))
                                        return accessor(items);
                                    return [];
                                }
                            """,
         DelegateName = "Accessor",
         Usings = ["Arcanum.Core.GameObjects.LocationCollections"],
      };

      context.AddSource($"{config.ClassName}.g.cs", DispatcherGeneratorHelper.Generate(config, foundTypes));
   }
}