using System.Text;
using Microsoft.CodeAnalysis;
using ParserGenerator.ParserGen;

namespace ParserGenerator;

public static class DispatchGenerator
{
   public static void GenerateMainDispatcher(StringBuilder sb,
                                             List<PropertyData> data,
                                             ITypeSymbol targetType)
   {
      data.RemoveAll(x => x.PropertyMetadata.IEu5KeyType != null || x.PropertyMetadata.Ignore);
      GenerateInlineParsingLoop(sb, targetType);

      sb.Append("    #region Dispatcher");
      sb.AppendLine();
      AppendDispatcherHeader(sb, targetType);

      AppendMainSwitchBody(sb, data);

      sb.AppendLine("        return true;");
      sb.AppendLine("    }");
      sb.AppendLine();

      sb.AppendLine("    #endregion");
   }

   public static void GenerateInlineParsingLoop(StringBuilder sb, ITypeSymbol targetType)
   {
      sb.AppendLine("    #region Inline Parsing Loop");
      sb.AppendLine();
      sb.AppendLine("    internal static void ParseProperties(");
      sb.AppendLine("        BlockNode blockNode,");
      sb.AppendLine($"        {targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} target,");
      sb.AppendLine("        ref ParsingContext pc,");
      sb.AppendLine("        bool allowUnknownNodes)");
      sb.AppendLine("    {");

      sb.AppendLine("        using var scope = pc.PushScope();");

      sb.AppendLine("        foreach (var node in blockNode.Children)");
      sb.AppendLine("        {");

      // Static Parsers (Dispatch)
      sb.AppendLine("            // Dispatch the node to the appropriate parser");
      sb.AppendLine("            if (Dispatch(node, target, ref pc))");
      sb.AppendLine("                continue;");
      sb.AppendLine();

      // Dynamic Parsers
      sb.AppendLine("            // Dynamic parsers");
      sb.AppendLine("            var wasHandled = false;");
      sb.AppendLine("            switch (node)");
      ;
      sb.AppendLine("            {");
      sb.AppendLine("                case ContentNode:");
      sb.AppendLine("                    foreach (var dcp in _dynamicContentParsers)");
      sb.AppendLine("                        if (dcp((ContentNode)node, target, ref pc))");
      sb.AppendLine("                        {");
      sb.AppendLine("                            wasHandled = true;");
      sb.AppendLine("                            break;");
      sb.AppendLine("                        }");
      sb.AppendLine("                    break;");
      sb.AppendLine();
      sb.AppendLine("                case BlockNode:");
      sb.AppendLine("                    foreach (var dbp in _dynamicBlockParsers)");
      sb.AppendLine("                        if (dbp((BlockNode)node, target, ref pc))");
      sb.AppendLine("                        {");
      sb.AppendLine("                            wasHandled = true;");
      sb.AppendLine("                            break;");
      sb.AppendLine("                        }");
      sb.AppendLine("                    break;");
      sb.AppendLine("            }");
      sb.AppendLine();
      sb.AppendLine("            if (wasHandled)");
      sb.AppendLine("                continue;");
      sb.AppendLine();

      // Ignored Nodes Check
      sb.AppendLine("            // Check if the node is ignored");
      sb.AppendLine("            if (IsIgnoredNode(node, ref pc))");
      sb.AppendLine("                continue;");
      sb.AppendLine();
      sb.AppendLine("            // Handle unknown nodes");
      sb.AppendLine("            if (!allowUnknownNodes)");
      sb.AppendLine("            {");
      sb.AppendLine("                pc.Fail();");
      sb.AppendLine("                pc.SetContext(node);");
      sb.AppendLine("                 DiagnosticException.LogWarning(ref pc,");
      sb.AppendLine("                    ParsingError.Instance.InvalidNodeType,");
      sb.AppendLine("                    node.GetType().Name,");
      sb.AppendLine("                    \"ContentNode or BlockNode or is node type is correct no parse in the dictionaries was found.\",");
      sb.AppendLine("                    pc.SliceString(node.KeyNode));");
      sb.AppendLine("            }");
      sb.AppendLine("        }");
      sb.AppendLine("    }");
      sb.AppendLine();
      sb.AppendLine("    #endregion");
   }

   public static void AppendIsIgnoredCheck(StringBuilder sb, string[] ignoredCns, string[] ignoredBns)
   {
      sb.AppendLine("    #region IsIgnoredCheck");
      sb.AppendLine("    private static bool IsIgnoredNode(StatementNode node, ref ParsingContext pc)");
      sb.AppendLine("    {");
      sb.AppendLine("        ref byte ptr = ref pc.GetKeyStartReference(node);");
      sb.AppendLine("        switch (node)");

      sb.AppendLine("        {");
      // Content Nodes
      sb.AppendLine("            case ContentNode:");
      sb.AppendLine("                switch (node.KeyNode.Length) {");

      var cnGroups = ignoredCns.GroupBy(k => k.Length);
      foreach (var g in cnGroups)
      {
         sb.AppendLine($"                    case {g.Key}:");
         foreach (var key in g)
         {
            sb.AppendLine($"                        if ({IntegerCheckGenerator.Generate(key)}) return true;");
         }

         sb.AppendLine("                        break;");
      }

      sb.AppendLine("                }");
      sb.AppendLine("                break;");

      // Block Nodes
      sb.AppendLine("            case BlockNode:");
      sb.AppendLine("                switch (node.KeyNode.Length) {");
      var bnGroups = ignoredBns.GroupBy(k => k.Length);
      foreach (var g in bnGroups)
      {
         sb.AppendLine($"                    case {g.Key}:");
         foreach (var key in g)
         {
            sb.AppendLine($"                        if ({IntegerCheckGenerator.Generate(key)}) return true;");
         }

         sb.AppendLine("                        break;");
      }

      sb.AppendLine("                }");
      sb.AppendLine("                break;");
      sb.AppendLine("        }");
      sb.AppendLine("        return false;");
      sb.AppendLine("    }");
      sb.AppendLine("    #endregion");
   }

   private static void AppendMainSwitchBody(StringBuilder sb, List<PropertyData> data)
   {
      var groups = DispatchDataHandler.GroupByLength(data);

      sb.AppendLine($"        // Length-based dispatch for {data.Count} properties");
      sb.AppendLine("        switch (node.KeyNode.Length)");
      sb.AppendLine("        {");

      foreach (var group in groups)
      {
         var propertyKeys = group.Properties.Select(p => p.PropertyMetadata.Keyword);
         sb.AppendLine($"            case {group.Length}: // [{group.Properties.Count,2}] {string.Join(", ", propertyKeys)}");
         sb.AppendLine("            {");

         foreach (var prop in group.Properties)
         {
            var md = prop.PropertyMetadata;
            var integerCheck = IntegerCheckGenerator.Generate(md.Keyword);

            sb.AppendLine($"                if ({integerCheck})");
            sb.AppendLine("                {");
            sb.AppendLine($"                    if (node is {md.AstNodeType} {md.Keyword}_node)");
            sb.AppendLine($"                        return {prop.MethodCall}({md.Keyword}_node, target, ref pc);");
            sb.AppendLine("                }");
         }

         sb.AppendLine("                break;");
         sb.AppendLine("            }");
      }

      sb.AppendLine("        }");
   }

   private static void AppendDispatcherHeader(StringBuilder sb, ITypeSymbol targetType)
   {
      sb.AppendLine("    internal static bool Dispatch(");
      sb.AppendLine("        StatementNode node,");
      sb.AppendLine($"        {targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} target,");
      sb.AppendLine("        ref ParsingContext pc)");
      sb.AppendLine("    {");

      sb.AppendLine("        ref byte ptr = ref pc.GetKeyStartReference(node);");
      sb.AppendLine();
   }
}