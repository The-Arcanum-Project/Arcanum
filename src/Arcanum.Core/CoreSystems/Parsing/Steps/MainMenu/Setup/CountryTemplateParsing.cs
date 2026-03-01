using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

[ParserFor(typeof(CountryTemplate))]
public partial class CountryTemplateParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<CountryTemplate>(dependencies)
{
   public override List<Type> ParsedObjects { get; } = [typeof(CountryTemplate),];

   public override void ReloadSingleFile(Eu5FileObj fileObj, object? lockObject)
   {
   }

   protected override void ParsePropertiesToObject(BlockNode block, CountryTemplate target, ref ParsingContext pc, bool allowUnknownNodes)
   {
      foreach (var sn in block.Children)
         CountryParsing.Dispatch(sn, target.TemplateData, ref pc);
   }

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, object? lockObject) => true;

   public override void LoadSingleFile(RootNode rn, ref ParsingContext pc, Eu5FileObj fileObj, object? lockObject)
   {
      DetectAll(fileObj.Descriptor, ref pc);

      // The files contain n StatementNodes containing any property of a country.
      // So we will treat the entire file a being a country which is the only thing a CountryTemplate wraps.

      var curCt = Globals.CountryTemplates[fileObj.Path.FilenameWithoutExtension];
      foreach (var sn in rn.Statements)
         CountryParsing.Dispatch(sn, curCt.TemplateData, ref pc);
   }

   private static void DetectAll(FileDescriptor descriptor, ref ParsingContext pc)
   {
      if (Globals.CountryTemplates.Count != 0)
         // We have already done this pre loading step.
         return;

      // We have to detect all templates beforehand as they are referencing each other.
      // The game simlpy loads them as strings, but we can do it directly and check for their existence immediately.

      var files = descriptor.Files;
      files.Sort(LUtil.Eu5FileObjFileNameComparer);

      for (var i = 0; i < files.Count; i++)
      {
         var file = descriptor.Files[i];
         var fileName = file.Path.FilenameWithoutExtension;
         var lct = new CountryTemplate
         {
            UniqueId = fileName,
            Source = file,
            Index = i,
            TemplateData = new()
         };
         LUtil.TryAddToGlobals(fileName, ref pc, lct, Globals.CountryTemplates);
      }
   }
}