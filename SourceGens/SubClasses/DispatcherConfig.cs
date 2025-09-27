using Microsoft.CodeAnalysis;

namespace ParserGenerator.SubClasses;

/// <summary>
/// Configuration for the DispatcherGeneratorHelper to generate a cache/dispatcher class.
/// </summary>
public class DispatcherConfig
{
   /// <summary>
   /// The namespace for the generated class.
   /// E.g., "Arcanum.Core.Registry"
   /// </summary>
   public string Namespace { get; set; } = "MISSING_GeneratedNamespace";

   /// <summary>
   /// The name of the static class to generate.
   /// E.g., "EmptyRegistry" or "MapInferrableDispatcher"
   /// </summary>
   public string ClassName { get; set; } = "MISSING_GeneratedClass";

   /// <summary>
   /// The C# code that defines the internal delegate for the dictionary.
   /// E.g., "private delegate object Accessor();"
   /// E.g., "private delegate IEnumerable Accessor(IEnumerable<Location> sLocs);"
   /// </summary>
   public string DelegateDefinition { get; set; } = "MISSING_DelegateDefinition";

   /// <summary>
   /// The name of the delegate type defined above.
   /// E.g., "Accessor"
   /// </summary>
   public string DelegateName { get; set; } = "Accessor";

   /// <summary>
   /// A function that takes a Roslyn type symbol and returns the C# code
   /// for the value to be stored in the dictionary at compile time.
   /// E.g., symbol => $"() => {symbol.ToDisplayString...}.Empty"
   /// E.g., symbol => $"(sLocs) => {symbol.ToDisplayString...}.GetInferredList(sLocs)"
   /// </summary>
   public Func<INamedTypeSymbol, string> CompileTimeValueFactory { get; set; } =
      symbol => $"MISSING_CompileTimeValueFactory_for_{symbol.Name}";

   /// <summary>
   /// The name of the static method or property to look for at runtime via reflection.
   /// For properties, use "get_YourProperty".
   /// E.g., "get_Empty" or "GetInferredList"
   /// </summary>
   public string RuntimeMethodName { get; set; } = "MISSING_RuntimeMethodName";

   /// <summary>
   /// The full C# source code for all the public "getter" methods.
   /// This gives you full control over the public API (e.g., creating TryGet and TryGet<T>).
   /// </summary>
   public string PublicApiMethods { get; set; } = "MISSING_PublicApiMethods";

   /// <summary>
   /// Additional using directives to include at the top of the generated file.
   /// E.g., new[] { "using System.Collections;", "using Arcanum.Core.GameObjects.LocationCollections;" }
   /// </summary>
   public string[] Usings { get; set; } = [];
}