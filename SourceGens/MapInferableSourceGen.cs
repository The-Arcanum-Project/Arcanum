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
         DelegateDefinition = "private delegate IEnumerable Accessor(IEnumerable<Location> sLocs);",
         RuntimeMethodName = "GetInferredList",
         CompileTimeValueFactory = symbol =>
            $"(sLocs) => {symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.GetInferredList(sLocs)",
         PublicApiMethods = """

                                /// <summary>
                                /// This retrieves a list of items of type T based on the selection.
                                /// </summary>
                                public static IEnumerable? GetInferredList(Type type, IEnumerable<Location> sLocs)
                                {
                                    if (_dispatchers.TryGetValue(type, out var accessor))
                                        return accessor(sLocs);
                                    return null;
                                }
                            """,
         DelegateName = "Accessor",
         Usings = ["Arcanum.Core.GameObjects.LocationCollections"],
      };

      context.AddSource($"{config.ClassName}.g.cs", DispatcherGeneratorHelper.Generate(config, foundTypes));
   }
}