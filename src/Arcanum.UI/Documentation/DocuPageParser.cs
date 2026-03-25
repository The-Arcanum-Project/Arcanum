using System.IO;
using Arcanum.Core.CoreSystems.IO;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Arcanum.UI.Documentation;

public static class DocuPageParser
{
   private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
                                                           .WithNamingConvention(CamelCaseNamingConvention.Instance) // Matches targetId to TargetId
                                                           .IgnoreUnmatchedProperties() // Prevents crash if YAML has extra fields
                                                           .WithTypeConverter(new FeatureIdYamlConverter()) // Custom converter for FeatureId
                                                           .Build();

   private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
                                                      .UseYamlFrontMatter()
                                                      .UseAlertBlocks() // Supports [!tip]
                                                      .Build();

   // Parse from file
   public static DocuPage? Parse(string path)
   {
      var content = IO.ReadAllTextUtf8(path);
      return content != null ? ParseInternal(content) : null;
   }

   // Parse page from stream
   public static DocuPage? Parse(StreamReader reader) => ParseInternal(reader.ReadToEnd());

   private static DocuPage? ParseInternal(string content)
   {
      if (string.IsNullOrWhiteSpace(content))
         return null;

      // Parse the Markdown Document structure
      var document = Markdown.Parse(content, Pipeline);

      // Extract and Deserialize YAML
      var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
      if (yamlBlock == null)
         return null;

      // Get the raw YAML string from the block
      var yamlRaw = content.Substring(yamlBlock.Span.Start, yamlBlock.Span.Length)
                           .Trim('-', '\r', '\n'); // Remove the "---" markers

      var page = YamlDeserializer.Deserialize<DocuPage>(yamlRaw);

      // Handle Content and Section Splitting
      // We calculate where the YAML ends to get the body
      var body = content.Substring(yamlBlock.Span.End).Trim('-', '\r', '\n');

      // TODO section parsing
      page.Content = body;

      return page;
   }
}