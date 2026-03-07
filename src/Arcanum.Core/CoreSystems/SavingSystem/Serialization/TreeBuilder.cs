// #define DO_SKIP_CHECK_IN_VISUALIZER

using System.Collections;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Nexus;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.Serialization.Nodes;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.SavingSystem.Serialization;

public static class TreeBuilder
{
   public static void ConstructAndWrite(IEu5Object target,
                                        IndentedStringBuilder sb,
                                        bool isOneLine,
                                        bool isArray,
                                        PropertySavingMetadata? psm,
                                        bool ignoreCustomSavingMethod,
                                        bool writeDefaults)
   {
      var node = Construct(target, isArray, psm, ignoreCustomSavingMethod);
      var commentChar = target.Source.Descriptor.FileType.CommentPrefix;
      node.Write(sb, ref commentChar, isOneLine, writeDefaults);
   }

   public static void ConstructAndWriteInjRep(IEu5Object target,
                                              IndentedStringBuilder sb,
                                              bool isOneLine,
                                              bool isArray,
                                              HashSet<PropertySavingMetadata> psms)
   {
      var node = Construct(target, isArray, null);
      var commentChar = target.Source.Descriptor.FileType.CommentPrefix;
      if (node is not BlockSerializationNode blockNode)
         throw new InvalidOperationException("InjRep serialization requires a BlockSerializationNode at the root.");

      blockNode.Write(sb, ref commentChar, isOneLine, psms, target.InjRepType, true);
   }

   public static SerializationNode Construct(IEu5Object gameObj, bool isArray, PropertySavingMetadata? psm, bool ignoreCustomSavingMethod = false)
   {
      if (gameObj.ClassMetadata.SavingMethod != null && !ignoreCustomSavingMethod)
         // Custom Saving Method Defined
         return new ManualSerializationNode(gameObj, [.. gameObj.SaveableProps], psm);

      var root = new BlockSerializationNode(FormattingService.FormatBlockNameWithInjection(gameObj, isArray),
                                            gameObj.AgsSettings.WriteEmptyCollectionHeader,
                                            gameObj,
                                            psm)
      {
         IsCompact = gameObj.AgsSettings.AsOneLine,
         InlineComment = gameObj.InlineComment,
         LeadingComment = gameObj.LeadingComment,
         ClosingComment = gameObj.ClosingComment,
      };
      var writeEmptyBlocks = gameObj.AgsSettings.WriteEmptyCollectionHeader;

      foreach (var meta in gameObj.SaveableProps)
      {
         if (meta.MustNotBeWritten?.Invoke(gameObj) == true)
            continue;

         // A. Check Defaults / AlwaysWrite
         var rawValue = Nx.ForceGetAs<object>(gameObj, meta.NxProp);
         if (!meta.AlwaysWrite && !Config.Settings.SavingConfig.WriteAllDefaultValues && IsDefault(rawValue, meta.DefaultValue))
            continue;

         var lead = gameObj.GetLeadingComment(meta.NxProp);
         var inline = gameObj.GetInlineComment(meta.NxProp);
         var closing = gameObj.GetClosingComment(meta.NxProp);

         // C. Create Nodes
         if (meta.IsCollection)
         {
            var block = new BlockSerializationNode(meta.Keyword, gameObj.AgsSettings.WriteEmptyCollectionHeader, gameObj, meta, meta.Separator)
            {
               LeadingComment = lead,
               InlineComment = inline,
               ClosingComment = closing,
            };

            // Populate Children
            var collection = (IEnumerable)rawValue;
            if (meta.IsEmbeddedObject)
               foreach (var item in collection)
               {
                  // Serialize Item Logic
                  // If item is IEu5Object -> Recursive BuildTree
                  // If item is Simple -> ValueSerializationNode
                  var childNode = CreateNodeForItem(item, meta);
                  block.Children.Add(childNode);
               }
            else
               block.Children.Add(new BulkValueSerializationNode(collection, meta, gameObj));

            if (writeEmptyBlocks || block.Children.Count > 0)
               root.Children.Add(block);
         }
         else
         {
            var prop = new PropertySerializationNode(meta, rawValue, gameObj)
            {
               LeadingComment = lead, InlineComment = inline,
            };

            root.Children.Add(prop);
         }
      }

      // 2. Add Body Comments
      // Again, assuming dynamic/interface access
      var bodyComments = gameObj.GetStandaloneComments();
      if (bodyComments is not null)
         foreach (var c in bodyComments)
            root.Children.Add(new CommentSerializationNode(c));

      return root;
   }

   private static SerializationNode CreateNodeForItem(object item, PropertySavingMetadata parentMeta)
   {
      if (item is IEu5Object nestedObj)
         // Recursion for complex objects
         // Note: We might need a generic non-typed version of BuildTree
         // Or use the item's own saving logic
         return Construct(nestedObj, true, parentMeta);

      // Simple Value
      return new ValueSerializationNode(item); // Use proper serializer here
   }

   private static bool IsDefault(object? val, object? def)
   {
      if (val == null && def == null)
         return true;
      if (val == null || def == null)
         return false;

      return val.Equals(def);
   }

   public static string PrintTreeStructure(SerializationNode node)
   {
      var sb = new IndentedStringBuilder();
      PrintRecursive(node, sb, 0);
      return sb.ToString();
   }

   private static void PrintRecursive(SerializationNode node, IndentedStringBuilder sb, int indentLevel)
   {
      var indent = new string(' ', indentLevel * 4); // 4 spaces per level
      var typeName = node.GetType().Name.Replace("SerializationNode", ""); // Shorten name

      // Visual indicators for comments attached to the node
      var commentFlags = new List<string>();
      if (!string.IsNullOrEmpty(node.LeadingComment))
         commentFlags.Add("Lead");
      if (!string.IsNullOrEmpty(node.InlineComment))
         commentFlags.Add("Inline");

      var flagsStr = commentFlags.Count > 0 ? $" [Comments: {string.Join(",", commentFlags)}]" : "";

      switch (node)
      {
         case BlockSerializationNode block:
         {
            var keyStr = string.IsNullOrEmpty(block.Key) ? "<Anonymous>" : $"Key: '{block.Key}'";
            var compactStr = block.IsCompact ? " (Compact)" : "";
            var closingCmt = !string.IsNullOrEmpty(block.ClosingComment) ? " [Has Closing Cmt]" : "";

            var count = 0;
            foreach (var c in block.Children)
               if (c is BulkValueSerializationNode bulk)
                  foreach (var _ in bulk.Collection)
                     count++;
               else
                  count++;

            sb.AppendLine($"{indent}📦 {typeName} -> [{count}]{keyStr}{compactStr}{flagsStr}{closingCmt}");

            foreach (var child in block.Children)
               PrintRecursive(child, sb, indentLevel + 1);

            break;
         }

         case PropertySerializationNode prop:
         {
#if DO_SKIP_CHECK_IN_VISUALIZER
            var shouldSkip = FormattingService.ShouldSkipCheck(prop.Psm, prop.Target, prop.Value, false);
            if (!shouldSkip)
               sb.AppendLine($"{indent}🔑 {typeName} -> Key: '{prop.Psm.Keyword}' Type: {prop.Value?.GetType().Name ?? "null"}{flagsStr}");
#else
            var shouldSkip = FormattingService.ShouldSkipCheck(prop.Psm, prop.Target, prop.Value, false, false);
            var sss = shouldSkip ? "[Skipped]" : "";
            sb.AppendLine($"{indent}🔑 {sss}{typeName} -> Key: '{prop.Psm.Keyword}' Type: {prop.Value.GetType().Name}{flagsStr}");
#endif
            break;
         }

         case ValueSerializationNode val:
         {
            sb.AppendLine($"{indent}📄 {typeName} -> Value: {val.Value.ToString() ?? "null"}{flagsStr}");
            break;
         }

         case BulkValueSerializationNode:
         {
            // We iterate just to check if empty, usually safe for collections
            sb.AppendLine($"{indent}📚 {typeName} -> (Delegate to FormattingService){flagsStr}");
            break;
         }

         case ManualSerializationNode:
         {
            sb.AppendLine($"{indent}⚠️ {typeName} -> (Black Box / Custom Delegate){flagsStr}");
            break;
         }

         case CommentSerializationNode comment:
         {
            // Truncate long comments for display
            var text = comment.Text.Length > 20 ? comment.Text.Substring(0, 20) + "..." : comment.Text;
            sb.AppendLine($"{indent}💬 {typeName} -> \"{text}\"");
            break;
         }

         default:
         {
            sb.AppendLine($"{indent}❓ {typeName} -> Unknown Node Type");
            break;
         }
      }
   }
}