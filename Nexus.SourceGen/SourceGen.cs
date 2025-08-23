using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Nexus.SourceGen;

[Generator]
public class PropertyModifierGenerator : IIncrementalGenerator
{
   public void Initialize(IncrementalGeneratorInitializationContext context)
   {
      const string targetInterfaceName = "Nexus.Core.INexus";

      var classDeclarations = context.SyntaxProvider
                                     .CreateSyntaxProvider(predicate: (node, _)
                                                              => node is ClassDeclarationSyntax cds &&
                                                                 cds.Modifiers.Any(m => m.IsKind(SyntaxKind
                                                                           .PartialKeyword)),
                                                           transform: (ctx, cancellationToken) =>
                                                           {
                                                              var classDeclaration = (ClassDeclarationSyntax)ctx.Node;
                                                              var semanticModel = ctx.SemanticModel;

                                                              // Get the symbol for the interface we're looking for
                                                              var interfaceSymbol =
                                                                 semanticModel.Compilation
                                                                              .GetTypeByMetadataName(targetInterfaceName);
                                                              if (interfaceSymbol is null)
                                                              {
                                                                 // The interface isn't defined in this compilation, so no classes can implement it.
                                                                 return null;
                                                              }

                                                              // Get the symbol for the class we're inspecting. 
                                                              // GetDeclaredSymbol returns ISymbol, so we must safely cast it to INamedTypeSymbol.

                                                              // Check if the class implements the interface, using the SymbolEqualityComparer for a robust check
                                                              if (ctx.SemanticModel.GetDeclaredSymbol(classDeclaration,
                                                                      cancellationToken) is { } classSymbol &&
                                                                  classSymbol.AllInterfaces.Contains(interfaceSymbol,
                                                                      SymbolEqualityComparer.Default))
                                                              {
                                                                 return classDeclaration;
                                                              }

                                                              return null;
                                                           })
                                     .Where(x => x is not null); // Filter out the nulls

      // Combine them with the compilation
      var compilationAndClasses
         = context.CompilationProvider.Combine(classDeclarations.Collect());

      // Generate the source
      context.RegisterSourceOutput(compilationAndClasses,
                                   (spc, source) => Execute(source.Left, source.Right!, spc));
   }

   private void Execute(Compilation compilation,
                        ImmutableArray<ClassDeclarationSyntax> classes,
                        SourceProductionContext context)
   {
      if (classes.IsDefaultOrEmpty)
         return;

      var distinctClasses = classes.Distinct();

      foreach (var classSyntax in distinctClasses)
      {
         context.CancellationToken.ThrowIfCancellationRequested();

         var semanticModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);

         if (ModelExtensions.GetDeclaredSymbol(semanticModel, classSyntax) is not INamedTypeSymbol classSymbol)
            continue;

         var attributeData = classSymbol.GetAttributes()
                                        .FirstOrDefault(ad =>
                                                           ad.AttributeClass?.ToDisplayString() ==
                                                           Helpers.ExplicitPropertiesAttributeString);

         // Find all eligible properties and fields
         var members = Helpers.FindModifiableMembers(classSymbol, attributeData is null, context);

         // Generate the code for this class
         var sourceCode = GeneratePartialClass(classSymbol, members);

         // Add the generated source file to the compilation
         var hintName = $"{classSymbol.ContainingNamespace}.{classSymbol.Name}.PropertyModifier.g.cs";
         context.AddSource(hintName, SourceText.From(sourceCode, Encoding.UTF8));
      }
   }

   private string GeneratePartialClass(INamedTypeSymbol classSymbol, List<ISymbol> members)
   {
      var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
      var className = classSymbol.Name;

      var builder = new StringBuilder();
      builder.AppendLine("// <auto-generated/>");
      builder.AppendLine("#nullable enable");
      builder.AppendLine("using Nexus.Core;");
      builder.AppendLine("using System.Runtime.CompilerServices;");
      builder.AppendLine("using System.ComponentModel;");
#if DEBUG
      builder.AppendLine("using System.Diagnostics;");
#endif
      builder.AppendLine($"namespace {namespaceName};");
      builder.AppendLine();
      builder.AppendLine($"public partial class {className}");
      builder.AppendLine("{");

      // 1. Generate the Enum
      builder.AppendLine("    public enum Field");
      builder.AppendLine("    {");
      foreach (var member in members)
      {
         ITypeSymbol memberType = member switch
         {
            IPropertySymbol p => p.Type,
            IFieldSymbol f => f.Type,
            _ => throw new ArgumentOutOfRangeException()
         };

         // 2. Get a fully qualified name for the type
         var memberTypeName = memberType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
         builder.AppendLine($"        [ExpectedType(typeof({memberTypeName}))]");
         builder.AppendLine($"        {member.Name},");
      }

      builder.AppendLine("    }");
      builder.AppendLine();

      // 2. Generate the SetValue method
      builder.AppendLine("    public void _setValue(Enum property, object value)");
      builder.AppendLine("    {");
      builder.AppendLine("        switch (property)");
      builder.AppendLine("        {");
      foreach (var member in members)
      {
         var memberType = (member is IPropertySymbol p) ? p.Type : ((IFieldSymbol)member).Type;
         var typeName = memberType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
         builder.AppendLine($"            case Field.{member.Name}:");
#if DEBUG
         builder.AppendLine($"                Debug.Assert(value is {typeName}, \"{member.Name} needs to be a {typeName}\");");
#endif
         builder.AppendLine($"                this.{member.Name} = ({typeName})value;");
         builder.AppendLine("                break;");
      }

      builder.AppendLine("        }");
      builder.AppendLine("    }");
      builder.AppendLine();

      // 3. Generate the GetValue method
      //builder.AppendLine("    [PropertyGetter]");
      builder.AppendLine("    public object _getValue(Enum property)");
      builder.AppendLine("    {");
      builder.AppendLine("        switch (property)");
      builder.AppendLine("        {");
      foreach (var member in members)
      {
         builder.AppendLine($"            case Field.{member.Name}:");
         builder.AppendLine($"                return this.{member.Name};");
      }

      builder.AppendLine("            default:");
      builder.AppendLine("                throw new ArgumentOutOfRangeException(nameof(property));");
      builder.AppendLine("        }");
      builder.AppendLine("    }");
      
      // 4. Generate the indexer
      builder.AppendLine();
      builder.AppendLine("    public object this[Enum key]");
      builder.AppendLine("    {");
      builder.AppendLine("        get => _getValue(key);");
      builder.AppendLine("        set => _setValue(key, value);");
      builder.AppendLine("    }");
      
      // 5. Generate the INotifyPropertyChanged implementation
      builder.AppendLine("    public event PropertyChangedEventHandler? PropertyChanged;");
      builder.AppendLine();
      builder.AppendLine("    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)");
      builder.AppendLine("    {");
      builder.AppendLine("        PropertyChanged?.Invoke(this, new(propertyName));");
      builder.AppendLine("    }");
      builder.AppendLine();
      builder.AppendLine("    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)");
      builder.AppendLine("    {");
      builder.AppendLine("        if (EqualityComparer<T>.Default.Equals(field, value))");
      builder.AppendLine("            return false;");
      builder.AppendLine();
      builder.AppendLine("        field = value;");
      builder.AppendLine("        OnPropertyChanged(propertyName);");
      builder.AppendLine("        return true;");
      builder.AppendLine("    }");
      builder.AppendLine();

      builder.AppendLine("}"); // Close class

      return builder.ToString();
   }
}