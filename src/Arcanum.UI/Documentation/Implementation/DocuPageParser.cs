#region

using System.IO;
using System.Text.RegularExpressions;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.UI.Documentation.Renderers;
using Common.Logger;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

#endregion

namespace Arcanum.UI.Documentation.Implementation;

public static class DocuPageParser
{
   private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
                                                           .WithNamingConvention(CamelCaseNamingConvention.Instance) // Matches targetId to TargetId
                                                           .IgnoreUnmatchedProperties() // Prevents crash if YAML has extra fields
                                                           .WithTypeConverter(new FeatureIdYamlConverter()) // Custom converter for FeatureId
                                                           .Build();

   private static readonly MarkdownPipeline Pipeline;

   static DocuPageParser()
   {
      var builder = new MarkdownPipelineBuilder()
                   .UseYamlFrontMatter()
                   .UseAdvancedExtensions()
                   .UseAlertBlocks();

      builder.Extensions.Add(new WpfAlertExtension());
      Pipeline = builder.Build();
   }

   // Parse from file
   public static FeatureDoc? Parse(string path)
   {
      var content = IO.ReadAllTextUtf8(path);
      return content != null ? ParseInternal(content) : null;
   }

   // Parse page from stream
   public static FeatureDoc? Parse(StreamReader reader) => ParseInternal(reader.ReadToEnd());

   private static FeatureDoc? ParseInternal(string content)
   {
      try
      {
         var document = Markdown.Parse(content, Pipeline);
         var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
         if (yamlBlock == null)
            return null;

         var yamlRaw = content.Substring(yamlBlock.Span.Start, yamlBlock.Span.Length).Trim('-', '\r', '\n');
         var page = YamlDeserializer.Deserialize<FeatureDoc>(yamlRaw);

         // Get the whole body after YAML
         var body = content[yamlBlock.Span.End..].Trim();

         // Split by the section marker: ---section:name---
         var sectionParts = Regex.Split(body, @"---section:(\w+)---");

         // The first part is always the main 'Content'
         page.Content = sectionParts[0].Trim('-', '\r', '\n');

         // The following parts come in pairs: [SectionName, SectionContent]
         var sections = new List<DocuSection>();
         for (var i = 1; i < sectionParts.Length; i += 2)
            if (Enum.TryParse<FeatureSection>(sectionParts[i], true, out var sectionType))
               sections.Add(new()
               {
                  Section = sectionType, Content = sectionParts[i + 1].Trim('-', '\r', '\n'),
               });

         page.Sections = sections.ToArray();
         return page;
      }
      catch (Exception e)
      {
         ArcLog.Error("DPP", $"Failed to parse documentation page. Error: {e.Message}", e);
         return null;
      }
   }
}