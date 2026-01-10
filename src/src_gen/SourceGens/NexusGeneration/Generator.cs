using System.Text;
using Microsoft.CodeAnalysis;
using ParserGenerator.HelperClasses;

namespace ParserGenerator.NexusGeneration;

public static class Generator
{
   private const string NX = "";

   public static void RunNexusGenerator(INamedTypeSymbol[] nexusClasses,
                                        SourceProductionContext context,
                                        Compilation compilation)
   {
      var enumerableSymbol = compilation.GetTypeByMetadataName("System.Collections.IEnumerable");
      var iListSymbol = compilation.GetTypeByMetadataName("System.Collections.IList");
      var ieu5ObjectSymbol = compilation.GetTypeByMetadataName("Arcanum.Core.GameObjects.BaseTypes.IEu5Object");
      if (enumerableSymbol is null || ieu5ObjectSymbol is null || iListSymbol is null)
         return;

      foreach (var cs in nexusClasses)
      {
         var code = Generate(cs, context, enumerableSymbol, ieu5ObjectSymbol, iListSymbol);
         context.AddSource($"{cs.ContainingNamespace}.{cs.Name}.Nexus.g.cs", code.ToString());
      }
   }

   private static StringBuilder Generate(INamedTypeSymbol cs,
                                         SourceProductionContext context,
                                         INamedTypeSymbol enumerableSymbol,
                                         INamedTypeSymbol ieu5ObjectSymbol,
                                         INamedTypeSymbol iListSymbol)
   {
      var npds = DataGatherer.CreateNexusPropertyDataList(cs,
                                                          context,
                                                          enumerableSymbol,
                                                          ieu5ObjectSymbol,
                                                          iListSymbol);

      var builder = new IndentBuilder();

      AppendFileUsing(builder, cs);
      using (builder.Indent())
      {
         using (builder.Region("Field Enum"))
            AppendEnumRegion(npds, builder);
         builder.AppendLine();

         using (builder.Region("Comment Storage"))
            AppendCommentStorage(builder, npds);
         builder.AppendLine();

         using (builder.Region("NuiConfig Bitmask Methods"))
            AppendIntBitsAsBools(builder, npds.Select(npd => npd.PropertyConfigData).ToList());
         builder.AppendLine();

         using (builder.Region("Type & ItemType"))
            AppendTypeData(builder, npds);
         builder.AppendLine();

         using (builder.Region("Descriptions"))
            AppendTypeDescriptions(builder, npds);
         builder.AppendLine();

         using (builder.Region("Default Values & Methods"))
            AppendDefaultValues(builder, npds);
         builder.AppendLine();

         using (builder.Region("Min / Max & Methods"))
            AppendMinMaxValues(builder, npds);
         builder.AppendLine();

         using (builder.Region("Setter / Getter"))
            AppendAccessorsValues(builder, npds);
         builder.AppendLine();

         using (builder.Region("Collection Methods"))
            AppendCollectionMethods(builder, npds);
         builder.AppendLine();

         using (builder.Region("Misc"))
            AppendMisc(builder, npds);
         builder.AppendLine();

         using (builder.Region("Deep Clone"))
            AppendDeepClone(builder, cs, npds);

         using (builder.Region("Property Changed & Indexer"))
            AppendPropertyChanged(builder);
         builder.AppendLine();

         using (builder.Region("Overrides"))
            AppendOverrides(builder, cs, ieu5ObjectSymbol);
         builder.AppendLine();
      }

      AppendFileFooter(builder);
      return builder.InnerBuilder;
   }

   private static void AppendMisc(IndentBuilder builder, List<NexusPropertyData> npds)
   {
      builder.AppendLine();
      builder.AppendLine("// Lazy array of all properties");
      builder.AppendLine($"private static Enum[]? {NX}_allProps;");
      builder.AppendLine($"public Enum[] {NX}GetAllProperties()");
      builder.AppendLine("{");
      using (builder.Indent())
      {
         builder.AppendLine($"if ({NX}_allProps == null)");
         using (builder.Indent())
            builder.AppendLine($"{NX}_allProps = Enum.GetValues(typeof({NX}Field)).Cast<Enum>().ToArray();");
         builder.AppendLine($"return {NX}_allProps!;");
      }

      builder.AppendLine("}");

      // IsCollection method
      builder.AppendLine();
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Returns whether the given property is a collection.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine("public bool IsCollection(Enum property)");
      builder.AppendLine("{");
      using (builder.Indent())
      {
         builder.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         builder.AppendLine($"return ({NX}Field)property switch");
         builder.AppendLine("{");
         using (builder.Indent())
         {
            foreach (var npd in npds.Where(npd => npd.IsCollection))
               builder.AppendLine($"{NX}Field.{npd.PropertyName} => true,");

            builder.AppendLine("_ => false");
         }

         builder.AppendLine("};");
      }

      builder.AppendLine("}");

      // byte array containg all AggregateLinkType values and getters for them
      builder.AppendLine();
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Stores the AggregateLinkType for each property.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine($"private static ReadOnlySpan<byte> {NX}aggregateLinkTypes  => new byte[] ");
      builder.AppendLine("{");
      var numStrings = npds
                      .Select(npd => ((byte)npd.PropertyConfigData.AggregateLinkType).ToString())
                      .ToArray();
      Formatter.FormatStringArray(builder, numStrings, 25);
      builder.AppendLine();
      builder.AppendLine("};");
      builder.AppendLine();
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Gets the AggregateLinkType of a property.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine("public AggregateLinkType GetNxPropAggregateLinkType(Enum property)");
      builder.AppendLine("{");
      using (builder.Indent())
      {
         builder.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         builder.AppendLine($"return (AggregateLinkType){NX}aggregateLinkTypes[Nx.GetEnumIndex(property)];");
      }

      builder.AppendLine("}");
      builder.AppendLine();

      // Getter Method to check if a property is any AggregateLinkType
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Checks if the property is of any AggregateLinkType.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine("public bool IsAggregateLink(Enum property)");
      builder.AppendLine("{");
      using (builder.Indent())
      {
         builder.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         builder.AppendLine($"return {NX}aggregateLinkTypes[Nx.GetEnumIndex(property)] != 0;");
      }

      builder.AppendLine("}");

      // Append switch to return AggregateLinkEnum if any is defined else null
      builder.AppendLine();
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Gets the AggregateLinkEnum of a property, if defined.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine("public Enum? GetCorrespondingEnum(Enum property)");
      builder.AppendLine("{");
      using (builder.Indent())
      {
         builder.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         builder.AppendLine($"return ({NX}Field)property switch");
         builder.AppendLine("{");
         using (builder.Indent())
         {
            foreach (var npd in npds)
               if (npd.PropertyConfigData.AggregateLinkParent is not null)
               {
                  var fullPropertyType = npd.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                  builder.AppendLine($"{NX}Field.{npd.PropertyName} => {fullPropertyType}.Field.{npd.PropertyConfigData.AggregateLinkParent},");
               }

            builder.AppendLine("_ => null");
         }

         builder.AppendLine("};");
      }

      builder.AppendLine("}");
   }

   private static void AppendDeepClone(IndentBuilder builder, INamedTypeSymbol cs, List<NexusPropertyData> npds)
   {
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Creates a deep copy of the object structure.");
      builder.AppendLine("/// Note: Collections are recreated, but items inside reference collections are not cloned unless they are value types.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine($"public INexus {NX}DeepClone()");
      builder.Block(m =>
      {
         m.AppendLine($"var {NX}clone = new {cs.Name}();");
         m.AppendLine();

         foreach (var npd in npds)
         {
            var propName = npd.PropertyName;

            // Handle Collections
            if (npd.IsCollection)
            {
               // Arrays
               if (npd.PropertyType.TypeKind == TypeKind.Array)
               {
                  var typeName = npd.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                  m.AppendLine($"if ({propName} != null)");
                  using (m.Indent())
                     m.AppendLine($"{NX}clone.{propName} = ({typeName}){propName}.Clone();");
               }
               // Standard Collections (List, HashSet, ObservableCollection)
               else if (npd.PropertyType is { } namedType &&
                        (namedType.Name.Contains("List") ||
                         namedType.Name.Contains("HashSet") ||
                         namedType.Name.Contains("Observable")))
               {
                  var typeName = npd.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                  m.AppendLine($"if ({propName} != null)");
                  using (m.Indent())
                     m.AppendLine($"{NX}clone.{propName} = new {typeName}({propName});");
               }
               // Fallback for interfaces or unknown collections (Shallow Copy)
               else
                  m.AppendLine($"{NX}clone.{propName} = {propName};");
            }
            // Handle Primitives, Strings, and Value Types (Direct Copy)
            else
               // Logic check: If it's a reference type that implements ICloneable, we could call .Clone()
               // For now, standard assignment is safest for a general generator.
               m.AppendLine($"{NX}clone.{propName} = {propName};");
         }

         m.AppendLine();
         m.AppendLine("return clone;");
      });
   }

   private static void AppendOverrides(IndentBuilder builder, INamedTypeSymbol cs, INamedTypeSymbol ieu5ObjectSymbol)
   {
      var inheritsOrImplements = SymbolEqualityComparer.Default.Equals(cs.BaseType, ieu5ObjectSymbol) ||
                                 cs.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, ieu5ObjectSymbol));

      if (!inheritsOrImplements)
         return;

      // Check if the user has already manually overridden ToString() in this specific class
      // We check 'cs.GetMembers()' to find members declared in THIS class.
      var hasToStringOverride = cs.GetMembers("ToString")
                                  .OfType<IMethodSymbol>()
                                  .Any(m => m.Parameters.Length == 0 && m.IsOverride);

      if (hasToStringOverride)
         // User implemented it manually, do not generate.
         return;

      builder.AppendLine("public override string ToString()");
      builder.Block(b => { b.AppendLine($"return {NX}_getValue({NX}Field.UniqueId)?.ToString() ?? $\"{cs.Name}(no UniqueId)\";"); });
      builder.AppendLine();
   }

   private static void AppendPropertyChanged(IndentBuilder builder)
   {
      // Indexer
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Indexer to get/set property values by enum.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine("public object? this[Enum property]");
      builder.Block(b =>
      {
         b.AppendLine($"get => {NX}_getValue(property);");
         b.AppendLine($"set => {NX}_setValue(property, value!);");
      });

      // PropertyChanged event
      builder.AppendLine();
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Event raised when a property value changes.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine("public event PropertyChangedEventHandler? PropertyChanged;");

      // OnPropertyChanged method
      builder.AppendLine();
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Raises the PropertyChanged event for the specified property name.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine("protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)");
      builder.Block(b => { b.AppendLine("PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));"); });
      builder.AppendLine();
   }

   private static void AppendCollectionMethods(IndentBuilder builder, List<NexusPropertyData> npds)
   {
      // _addToCollection
      builder.AppendLine($"public void {NX}_addToCollection(Enum property, object item)");
      builder.Block(m =>
      {
         m.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         if (npds.All(npd => !npd.IsCollection))
         {
            m.AppendLine("// No collection properties available to remove from.");
            m.AppendLine("return;");
            return;
         }

         m.AppendLine($"switch (({NX}Field)property)");
         m.Block(sw =>
         {
            foreach (var npd in npds.Where(npd => npd.IsCollection))
            {
               sw.AppendLine($"case {NX}Field.{npd.PropertyName}:");
               using (sw.Indent())
               {
                  var itemType = npd.CollectionItemType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                  sw.AppendLine($"{npd.PropertyName}.Add(({itemType})item);");
                  sw.AppendLine($"OnPropertyChanged(nameof({npd.PropertyName}));");
                  sw.AppendLine("break;");
               }
            }
         });
      });

      // _removeFromCollection
      builder.AppendLine();
      builder.AppendLine($"public void {NX}_removeFromCollection(Enum property, object item)");
      builder.Block(m =>
      {
         m.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         if (npds.All(npd => !npd.IsCollection))
         {
            m.AppendLine("// No collection properties available to remove from.");
            m.AppendLine("return;");
            return;
         }

         m.AppendLine($"switch (({NX}Field)property)");
         m.Block(sw =>
         {
            foreach (var npd in npds.Where(npd => npd.IsCollection))
            {
               sw.AppendLine($"case {NX}Field.{npd.PropertyName}:");
               using (sw.Indent())
               {
                  var itemType = npd.CollectionItemType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                  sw.AppendLine($"{npd.PropertyName}.Remove(({itemType})item);");
                  sw.AppendLine($"OnPropertyChanged(nameof({npd.PropertyName}));");
                  sw.AppendLine("break;");
               }
            }
         });
      });

      // _addRangeToCollection
      builder.AppendLine();
      builder.AppendLine($"public void {NX}_addRangeToCollection(Enum property, System.Collections.IEnumerable items)");
      builder.Block(m =>
      {
         m.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         if (npds.All(npd => !npd.IsCollection))
         {
            m.AppendLine("// No collection properties available to remove from.");
            m.AppendLine("return;");
            return;
         }

         m.AppendLine($"switch (({NX}Field)property)");
         m.Block(sw =>
         {
            foreach (var npd in npds.Where(npd => npd.IsCollection))
            {
               sw.AppendLine($"case {NX}Field.{npd.PropertyName}:");
               using (sw.Indent())
               {
                  var itemType = npd.CollectionItemType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                  sw.AppendLine($"{npd.PropertyName}.AddRange(items.Cast<{itemType}>());");
                  sw.AppendLine($"OnPropertyChanged(nameof({npd.PropertyName}));");
                  sw.AppendLine("break;");
               }
            }
         });
      });

      // _removeRangeFromCollection
      builder.AppendLine();
      builder.AppendLine($"public void {NX}_removeRangeFromCollection(Enum property, System.Collections.IEnumerable items)");
      builder.Block(m =>
      {
         m.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         if (npds.All(npd => !npd.IsCollection))
         {
            m.AppendLine("// No collection properties available to remove from.");
            m.AppendLine("return;");
            return;
         }

         m.AppendLine($"switch (({NX}Field)property)");
         m.Block(sw =>
         {
            foreach (var npd in npds.Where(npd => npd.IsCollection))
            {
               sw.AppendLine($"case {NX}Field.{npd.PropertyName}:");
               using (sw.Indent())
               {
                  var itemType = npd.CollectionItemType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                  sw.AppendLine($"{npd.PropertyName}.RemoveRange(items.Cast<{itemType}>());");
                  sw.AppendLine($"OnPropertyChanged(nameof({npd.PropertyName}));");
                  sw.AppendLine("break;");
               }
            }
         });
      });

      // _insertIntoCollection
      builder.AppendLine();
      builder.AppendLine($"public void {NX}_insertIntoCollection(Enum property, int index, object item)");
      builder.Block(m =>
      {
         m.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         if (npds.All(npd => !npd.IsCollection))
         {
            m.AppendLine("// No collection properties available to remove from.");
            m.AppendLine("return;");
            return;
         }

         m.AppendLine($"switch (({NX}Field)property)");
         m.Block(sw =>
         {
            foreach (var npd in npds.Where(npd => npd.IsCollection))
            {
               sw.AppendLine($"case {NX}Field.{npd.PropertyName}:");
               using (sw.Indent())
               {
                  var itemType = npd.CollectionItemType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                  sw.AppendLine($"{npd.PropertyName}.Insert(index, ({itemType})item);");
                  sw.AppendLine($"OnPropertyChanged(nameof({npd.PropertyName}));");
                  sw.AppendLine("break;");
               }
            }
         });
      });

      // _removeFromCollectionAt
      builder.AppendLine();
      builder.AppendLine($"public void {NX}_removeFromCollectionAt(Enum property, int index)");
      builder.Block(m =>
      {
         m.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         if (npds.All(npd => !npd.IsCollection))
         {
            m.AppendLine("// No collection properties available to remove from.");
            m.AppendLine("return;");
            return;
         }

         m.AppendLine($"switch (({NX}Field)property)");
         m.Block(sw =>
         {
            foreach (var npd in npds.Where(npd => npd.IsCollection))
            {
               sw.AppendLine($"case {NX}Field.{npd.PropertyName}:");
               using (sw.Indent())
               {
                  sw.AppendLine($"{npd.PropertyName}.RemoveAt(index);");
                  sw.AppendLine($"OnPropertyChanged(nameof({npd.PropertyName}));");
                  sw.AppendLine("break;");
               }
            }
         });
      });

      // _clearCollection
      builder.AppendLine();
      builder.AppendLine($"public void {NX}_clearCollection(Enum property)");
      builder.Block(m =>
      {
         m.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         if (npds.All(npd => !npd.IsCollection))
         {
            m.AppendLine("// No collection properties available to remove from.");
            m.AppendLine("return;");
            return;
         }

         m.AppendLine($"switch (({NX}Field)property)");
         m.Block(sw =>
         {
            foreach (var npd in npds.Where(npd => npd.IsCollection))
            {
               sw.AppendLine($"case {NX}Field.{npd.PropertyName}:");
               using (sw.Indent())
               {
                  sw.AppendLine($"{npd.PropertyName}.Clear();");
                  sw.AppendLine($"OnPropertyChanged(nameof({npd.PropertyName}));");
                  sw.AppendLine("break;");
               }
            }
         });
      });
   }

   private static void AppendAccessorsValues(IndentBuilder builder, List<NexusPropertyData> npds)
   {
      builder.AppendLine($"public void {NX}_setValue(Enum property, object value)");
      builder.Block(m =>
      {
         m.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         m.AppendLine("switch (Nx.GetEnumIndex(property))");
         m.Block(sw =>
         {
            for (var i = 0; i < npds.Count; i++)
            {
               var npd = npds[i];

               sw.AppendLine($"case {i}: // {NX}Field.{npd.PropertyName}");
               using (sw.Indent())
               {
                  var typeSymbol = npd.PropertyType;
                  var fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                  var valName = $"{npd.PropertyName}_val";

                  if (npd.IsCollection &&
                      typeSymbol.Name is "ObservableRangeCollection" or "List" or "HashSet")
                  {
                     var itemType = npd.CollectionItemType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                     sw.AppendLine($"{fullTypeName} {valName};");

                     // Exact match
                     sw.AppendLine($"if (value is {fullTypeName} {npd.PropertyName}_exact)");
                     using (sw.Indent())
                        sw.AppendLine($"{valName} = {npd.PropertyName}_exact;");

                     // IEnumerable conversion
                     sw.AppendLine($"else if (value is System.Collections.Generic.IEnumerable<{itemType}> {npd.PropertyName}_iter)");
                     using (sw.Indent())
                        sw.AppendLine($"{valName} = new {fullTypeName}({npd.PropertyName}_iter);");

                     // Fallback cast
                     sw.AppendLine("else");
                     using (sw.Indent())
                        sw.AppendLine($" {valName} = ({fullTypeName})value;");
                  }
                  else
                     // Standard Cast
                     sw.AppendLine($"var {valName} = ({fullTypeName})value;");

                  var canUseOperator = typeSymbol.IsReferenceType ||
                                       typeSymbol.TypeKind == TypeKind.Enum ||
                                       typeSymbol.SpecialType != SpecialType.None;

                  sw.AppendLine(canUseOperator
                                   ? $"if ({npd.PropertyName} == {valName})"
                                   : $"if ({npd.PropertyName}.Equals({valName}))");
                  using (sw.Indent())
                     sw.AppendLine("return;");

                  // Assign and Notify
                  sw.AppendLine($"{npd.PropertyName} = {valName};");
                  sw.AppendLine($"OnPropertyChanged(nameof({npd.PropertyName}));");
                  sw.AppendLine("break;");
               }
            }
         });
      });

      builder.AppendLine();
      builder.AppendLine($"public object {NX}_getValue(Enum property)");
      builder.Block(m =>
      {
         m.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         m.AppendLine("switch (Nx.GetEnumIndex(property))");
         m.Block(sw =>
         {
            for (var i = 0; i < npds.Count; i++)
            {
               sw.AppendLine($"case {i}:");
               sw.AppendLine($"   return {npds[i].PropertyName};");
            }

            sw.AppendLine("default: throw new ArgumentOutOfRangeException(\"Invalid Enum Index\");");
         });
      });

      builder.AppendLine();
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Gets the value of a property, strongly typed. Optimized to avoid boxing.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine($"public T {NX}GetValue<T>(Enum property)");
      builder.Block(m =>
      {
         m.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property));");

         m.AppendLine("switch (Nx.GetEnumIndex(property))");
         m.Block(sw =>
         {
            for (var i = 0; i < npds.Count; i++)
            {
               var npd = npds[i];
               var propTypeName = npd.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

               sw.AppendLine($"case {i}: // {npd.PropertyName}");
               using (sw.Indent())
               {
                  sw.AppendLine($"{propTypeName} val_{npd.PropertyName} = {npd.PropertyName};");
                  sw.AppendLine($"return Unsafe.As<{propTypeName}, T>(ref val_{npd.PropertyName});");
               }
            }

            sw.AppendLine("default: throw new ArgumentOutOfRangeException(\"Invalid Enum Index\");");
         });
      });
   }

   private static void AppendMinMaxValues(IndentBuilder builder, List<NexusPropertyData> npds)
   {
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Gets the minimum value of a property, if applicable.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine($"public object? {NX}GetMinValue(Enum property)");

      builder.Block(b =>
      {
         builder.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         b.AppendLine($"return ({NX}Field)property switch");

         b.AppendLine("{");
         using (b.Indent())
         {
            foreach (var npd in npds)
            {
               if (npd.PropertyConfigData.MinValue is "null" or null)
                  continue;

               b.AppendLine($"{NX}Field.{npd.PropertyName} => {npd.PropertyConfigData.MinValue},");
            }

            // Default Case: Handle invalid enum values safely
            b.AppendLine("_ => null");
         }

         b.AppendLine("};");
      });

      // public T? NX_GetMinValue<T>(Enum property)
      builder.AppendLine();
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Gets the minimum value of a property, strongly typed, if applicable.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine($"public T? {NX}GetMinValue<T>(Enum property)");

      builder.Block(_ =>
      {
         builder.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         builder.AppendLine($"var minValue = {NX}GetMinValue(property);");
         builder.AppendLine($"Debug.Assert({NX}GetNxPropType(property) == typeof(T), \"Requested type does not match property type\");");
         builder.AppendLine("return (T)minValue!;");
      });

      builder.AppendLine();
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Gets the maximum value of a property, if applicable.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine($"public object? {NX}GetMaxValue(Enum property)");

      builder.Block(b =>
      {
         builder.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         b.AppendLine($"return ({NX}Field)property switch");

         b.AppendLine("{");
         using (b.Indent())
         {
            foreach (var npd in npds)
            {
               if (npd.PropertyConfigData.MaxValue is "null" or null)
                  continue;

               b.AppendLine($"{NX}Field.{npd.PropertyName} => {npd.PropertyConfigData.MaxValue},");
            }

            // Default Case: Handle invalid enum values safely
            b.AppendLine("_ => null");
         }

         b.AppendLine("};");
      });

      // public T? NX_GetMaxValue<T>(Enum property)
      builder.AppendLine();
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Gets the maximum value of a property, strongly typed, if applicable.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine($"public T? {NX}GetMaxValue<T>(Enum property)");
      builder.Block(_ =>
      {
         builder.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         builder.AppendLine($"var maxValue = {NX}GetMaxValue(property);");
         builder.AppendLine($"Debug.Assert({NX}GetNxPropType(property) == typeof(T), \"Requested type does not match property type\");");
         builder.AppendLine("return (T)maxValue!;");
      });
   }

   private static void AppendDefaultValues(IndentBuilder builder, List<NexusPropertyData> npds)
   {
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Gets the default value of a property.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine($"public object {NX}GetDefaultValue(Enum property)");
      builder.Block(b =>
      {
         builder.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         // Start the switch expression
         b.AppendLine($"return ({NX}Field)property switch");
         b.AppendLine("{");

         using (b.Indent())
         {
            foreach (var npd in npds)
               // Case: Field.Name => Value,
               b.AppendLine($"{NX}Field.{npd.PropertyName} => {npd.DefaultValue},");

            // Default Case: Handle invalid enum values safely
            b.AppendLine("_ => throw new ArgumentOutOfRangeException(nameof(property), property, \"Unknown property enum value\")");
         }

         b.AppendLine("};");
      });

      // public T GetDefaultValue<T>(Enum property)
      builder.AppendLine();
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Gets the default value of a property, strongly typed.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine($"public T {NX}GetDefaultValue<T>(Enum property)");
      builder.Block(_ =>
      {
         builder.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         builder.AppendLine($"Debug.Assert({NX}GetNxPropType(property) == typeof(T), \"Requested type does not match property type\");");
         builder.AppendLine($"Debug.Assert(!{NX}GetNxPropType(property).IsValueType || Nullable.GetUnderlyingType({NX}GetNxPropType(property)) != null || ");
         builder.AppendLine($"             {NX}GetDefaultValue(property) != null, \"Property default value is null but requested type is non-nullable value type\");");
         builder.AppendLine($"return (T){NX}GetDefaultValue(property)!;");
      });
   }

   private static void AppendTypeDescriptions(IndentBuilder builder, List<NexusPropertyData> npds)
   {
      var descriptionStrings = npds.Select(npd => $"\"{Formatter.EscapeStringForCode(npd.Description)}\"").ToArray();

      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Array of property descriptions for each property. <br/>");
      builder.AppendLine("/// </summary>");
      builder.AppendLine($"public static readonly string[] {NX}PropertyDescriptions = {{ ");
      Formatter.FormatStringArray(builder, descriptionStrings, 1);
      builder.AppendLine();
      builder.AppendLine(" };");
      builder.AppendLine();

      // Getter for Description
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Gets the description of a property.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine($"public string {NX}GetDescription(Enum property)");
      builder.AppendLine("{");
      using (builder.Indent())
      {
         builder.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         builder.AppendLine($"return {NX}PropertyDescriptions[Nx.GetEnumIndex(property)];");
      }

      builder.AppendLine("}");
   }

   private static void AppendTypeData(IndentBuilder builder, List<NexusPropertyData> npds)
   {
      // We generate two arrays: one for the property types, and one for the collection item types (if applicable)
      var typeStrings = npds.Select(npd => $"typeof({npd.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})").ToArray();
      var itemTypeStrings = npds.Select(npd =>
                                 {
                                    if (npd is { IsCollection: true, CollectionItemType: not null })
                                       return $"typeof({npd.CollectionItemType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})";

                                    return "null";
                                 })
                                .ToArray();

      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Array of property types for each property. <br/>");
      builder.AppendLine("/// </summary>");
      builder.AppendLine($"public static readonly System.Type[] {NX}PropertyTypes = {{ ");
      Formatter.FormatStringArray(builder, typeStrings, 1);
      builder.AppendLine();
      builder.AppendLine(" };");

      builder.AppendLine();

      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Array of collection item types for each property, or null if the property is not a collection. <br/>");
      builder.AppendLine("/// </summary>");
      builder.AppendLine($"public static readonly System.Type?[] {NX}CollectionItemTypes = {{ ");
      Formatter.FormatStringArray(builder, itemTypeStrings, 1);
      builder.AppendLine();
      builder.AppendLine(" };");

      // Getter and Setters for Type and ItemType

      builder.AppendLine();
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Gets the Type of a property.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine($"public Type {NX}GetNxPropType(Enum property)");
      builder.AppendLine("{");
      using (builder.Indent())
      {
         builder.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         builder.AppendLine($"return {NX}PropertyTypes[Nx.GetEnumIndex(property)];");
      }

      builder.AppendLine("}");
      builder.AppendLine();

      // --- Public accessor method for the Collection Item Type ---
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Gets the item Type for a collection property, or null if the property is not a collection.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine($"public Type? {NX}GetNxItemType(Enum property)");
      builder.AppendLine("{");
      using (builder.Indent())
      {
         builder.AppendLine($"Debug.Assert(Enum.IsDefined(typeof({NX}Field), property), $\"Invalid property enum value for:'{{property}}'\");");
         builder.AppendLine($"return {NX}CollectionItemTypes[Nx.GetEnumIndex(property)];");
      }

      builder.AppendLine("}");
      builder.AppendLine();
   }

   private static void AppendIntBitsAsBools(IndentBuilder sb, List<PropertyConfigData> nuiConfig)
   {
      var flags = new (string ConstName, string MethodName, string Comment, Func<PropertyConfigData, bool> Selector)[]
      {
         ($"{NX}IS_INLINED", $"{NX}IsPropertyInlined", "Checks if the property is marked as inlined.", d => d.IsInlined),
         ($"{NX}IS_READONLY", $"{NX}IsPropertyReadOnly", "Checks if the property is marked as read-only.", d => d.IsReadonly),
         ($"{NX}ALLOW_EMPTY", $"{NX}AllowsEmptyValue", "Checks if the property allows empty values.", d => d.AllowEmpty), ($"{NX}DISABLE_MAP_INFER_BUTTONS",
                  $"{NX}IsMapInferButtonsDisabled", "Checks if the property has map infer buttons disabled.",
                  d => d.DisableMapInferButtons),
         ($"{NX}IS_REQUIRED", $"{NX}IsRequired", "Checks if the property is marked as required.", d => d.IsRequired),
         ($"{NX}IGNORE_COMMAND", $"{NX}IgnoreCommand", "Checks if the property should ignore command behavior.", d => d.IgnoreCommand),
      };

      var bitArrays = new int[nuiConfig.Count];
      for (var i = 0; i < nuiConfig.Count; i++)
      {
         var data = nuiConfig[i];
         if (data == null)
            continue;

         for (var bitIndex = 0; bitIndex < flags.Length; bitIndex++)
            if (flags[bitIndex].Selector(data))
               bitArrays[i] |= 1 << bitIndex;
      }

      sb.AppendLine("/// <summary>");
      sb.AppendLine("/// Encoded NuiConfig data as an integer for efficient access. <br/>");
      for (var i = 0; i < flags.Length; i++)
         sb.AppendLine($"/// Bit {i}: {flags[i].ConstName} <br/>");

      sb.AppendLine("/// </summary>");

      var numStrings = bitArrays.Select(b => "0b" + Convert.ToString(b, 2).PadLeft(8, '0')).ToArray();

      sb.AppendLine($"private static ReadOnlySpan<int> {NX}ConfigBits => new int[] ");
      sb.AppendLine("{ ");
      Formatter.FormatStringArray(sb, numStrings);
      sb.AppendLine();
      sb.AppendLine(" };");
      sb.AppendLine();

      for (var i = 0; i < flags.Length; i++)
         sb.AppendLine($"private const int {flags[i].ConstName} = 1 << {i};");

      sb.AppendLine();

      for (var i = 0; i < flags.Length; i++)
      {
         var flag = flags[i];
         sb.AppendLine("/// <summary>");
         sb.AppendLine($"/// {flag.Comment}");
         sb.AppendLine("/// </summary>");
         sb.AppendLine($"public bool {flag.MethodName}(Enum property)");
         using (sb.Indent())
            sb.AppendLine($"=> ({NX}ConfigBits[(int)(({NX}Field)property)] & {flag.ConstName}) != 0;");

         if (i < flags.Length - 1)
            sb.AppendLine();
      }
   }

   private static void AppendEnumRegion(List<NexusPropertyData> npds, IndentBuilder builder)
   {
      // We generate an enum for each property
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Enum containing all properties of this Class.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine($"public enum {NX}Field");
      builder.AppendLine("{");
      using (builder.Indent())
         for (var i = 0; i < npds.Count; i++)
         {
            var npd = npds[i];
            builder.AppendLine("/// <summary>");
            if (!string.IsNullOrEmpty(npd.Description))
               if (!string.IsNullOrEmpty(npd.Description))
               {
                  // Normalize newlines and split
                  var lines = npd.Description!.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

                  foreach (var line in lines)
                     // Append the comment prefix to every line
                     builder.AppendLine($"/// {line}");
               }

            builder.AppendLine("/// </summary>");
            builder.AppendLine($"{npd.PropertyName} = {i},");
         }

      builder.AppendLine("}");
   }

   private static void AppendCommentStorage(IndentBuilder builder, List<NexusPropertyData> npds)
   {
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Array of property comments for each property. <br/>");
      builder.AppendLine("/// </summary>");
      builder.AppendLine("private string?[]? _leadingComments;");
      builder.AppendLine("private string?[]? _inlineComments;");
      builder.AppendLine("private string?[]? _closingComments;");
      builder.AppendLine("private string[]? _blockBodyComment;");
      builder.AppendLine();

      builder.AppendLine("private void EnsureCommentStore(ref string?[]? store)");
      builder.Block(_ =>
      {
         builder.AppendLine("if (store == null)");
         using (builder.Indent())
            builder.AppendLine($"store = new string?[{npds.Count}];");
      });

      // GetLeadingComment
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Sets the leading comment for a property.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine("public void SetLeadingComment(Enum property, string? val)");
      builder.Block(_ =>
      {
         builder.AppendLine("if (val == null && _leadingComments == null)");
         using (builder.Indent())
            builder.AppendLine("return;");
         builder.AppendLine("EnsureCommentStore(ref _leadingComments);");
         builder.AppendLine("_leadingComments![Nx.GetEnumIndex(property)] = val;");
      });
      builder.AppendLine();

      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Gets the leading comment for a property.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine("public string? GetLeadingComment(Enum property)");
      using (builder.Indent())
         builder.AppendLine("=> _leadingComments?[Nx.GetEnumIndex(property)];");
      builder.AppendLine();

      // SetInlineComment
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Sets the inline comment for a property.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine("public void SetInlineComment(Enum property, string? val)");
      builder.Block(_ =>
      {
         builder.AppendLine("if (val == null && _inlineComments == null)");
         using (builder.Indent())
            builder.AppendLine("return;");
         builder.AppendLine("EnsureCommentStore(ref _inlineComments);");
         builder.AppendLine("_inlineComments![Nx.GetEnumIndex(property)] = val;");
      });
      builder.AppendLine();

      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Gets the inline comment for a property.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine("public string? GetInlineComment(Enum property)");
      using (builder.Indent())
         builder.AppendLine("=> _inlineComments?[Nx.GetEnumIndex(property)];");
      builder.AppendLine();

      // SetClosingComment
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Sets the closing comment for a property.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine("public void SetClosingComment(Enum property, string? val)");
      builder.Block(_ =>
      {
         builder.AppendLine("if (val == null && _closingComments == null)");
         using (builder.Indent())
            builder.AppendLine("return;");
         builder.AppendLine("EnsureCommentStore(ref _closingComments);");
         builder.AppendLine("_closingComments![Nx.GetEnumIndex(property)] = val;");
      });
      builder.AppendLine();

      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Gets the closing comment for a property.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine("public string? GetClosingComment(Enum property)");
      using (builder.Indent())
         builder.AppendLine("=> _closingComments?[Nx.GetEnumIndex(property)];");
      builder.AppendLine();

      // Standalone block body comments
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Add a standalone block comment.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine("public void AddStandaloneComment(string comment)");
      builder.Block(_ =>
      {
         builder.AppendLine("if (_blockBodyComment == null)");
         using (builder.Indent())
            builder.AppendLine("_blockBodyComment = new string[] { comment };");
         builder.AppendLine("else");
         using (builder.Indent())
            // Resize and add
            builder.AppendLine("_blockBodyComment = _blockBodyComment.Append(comment).ToArray();");
      });
      builder.AppendLine();

      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Gets the standalone block comments.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine("public string[]? GetStandaloneComments()");
      using (builder.Indent())
         builder.AppendLine("=> _blockBodyComment;");
      builder.AppendLine();

      // _assignComments
      builder.AppendLine("/// <summary>");
      builder.AppendLine("/// Assigns comments from an AstNode to a property, handling shattered/collection merging.");
      builder.AppendLine("/// </summary>");
      builder.AppendLine("internal void _assignComments(Enum property, AstNode node, bool isShatteredOrCollection)");
      builder.Block(_ =>
      {
         builder.AppendLine("string? lead = node.LeadingComment;");
         builder.AppendLine("string? inline = node.InlineComment;");
         builder.AppendLine("string? closing = (node as BlockNode)?.ClosingComment;");
         builder.AppendLine();

         builder.AppendLine("if (isShatteredOrCollection)");
         builder.Block(__ =>
         {
            builder.AppendLine("var existingLead = GetLeadingComment(property);");
            builder.AppendLine("if (!string.IsNullOrEmpty(existingLead) && !string.IsNullOrEmpty(lead))");
            using (builder.Indent())
               builder.AppendLine("lead = existingLead + \"\\n\" + lead;");
            builder.AppendLine("else if (!string.IsNullOrEmpty(existingLead))");
            using (builder.Indent())
               builder.AppendLine("lead = existingLead;");
            builder.AppendLine();

            builder.AppendLine("var existingClose = GetClosingComment(property);");
            builder.AppendLine("if (!string.IsNullOrEmpty(existingClose) && !string.IsNullOrEmpty(closing))");
            using (builder.Indent())
               builder.AppendLine("closing = existingClose + \"\\n\" + closing;");
            builder.AppendLine("else if (!string.IsNullOrEmpty(existingClose))");
            using (builder.Indent())
               builder.AppendLine("closing = existingClose;");
            builder.AppendLine();

            builder.AppendLine("var existingInline = GetInlineComment(property);");
            builder.AppendLine("if (!string.IsNullOrEmpty(existingInline) && !string.IsNullOrEmpty(inline))");
            using (builder.Indent())
               builder.AppendLine("inline = existingInline + \" \" + inline;");
            builder.AppendLine("else if (!string.IsNullOrEmpty(existingInline))");
            using (builder.Indent())
               builder.AppendLine("inline = existingInline;");
         });
         builder.AppendLine();

         builder.AppendLine("if (lead != null)");
         using (builder.Indent())
            builder.AppendLine("SetLeadingComment(property, lead);");
         builder.AppendLine("if (inline != null)");
         using (builder.Indent())
            builder.AppendLine("SetInlineComment(property, inline);");
         builder.AppendLine("if (closing != null)");
         using (builder.Indent())
            builder.AppendLine("SetClosingComment(property, closing);");
      });
   }

   private static void AppendFileUsing(IndentBuilder builder, INamedTypeSymbol cs)
   {
      var namespaceName = cs.ContainingNamespace.ToDisplayString();

      builder.AppendLine("// <auto-generated/>");
      builder.AppendLine("#nullable enable");

      builder.AppendLine("using Arcanum.Core.CoreSystems.History.Commands;");
      builder.AppendLine("using Arcanum.Core.CoreSystems.Nexus;");
      builder.AppendLine("using Arcanum.Core.CoreSystems.NUI.Attributes;");
      builder.AppendLine("using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;");
      builder.AppendLine("using Arcanum.Core.Registry;");
      builder.AppendLine("using Nexus.Core;");
      builder.AppendLine("using System.Runtime.CompilerServices;");
      builder.AppendLine("using System.ComponentModel;");
      builder.AppendLine("using System.Diagnostics;");

      builder.AppendLine($"namespace {namespaceName};");

      builder.AppendLine($"public partial class {cs.Name}");
      builder.AppendLine("{");
   }

   private static void AppendFileFooter(IndentBuilder builder)
   {
      builder.AppendLine("}");
   }
}