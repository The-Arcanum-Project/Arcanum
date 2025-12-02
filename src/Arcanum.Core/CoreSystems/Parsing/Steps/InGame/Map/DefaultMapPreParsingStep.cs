using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.Utils.Sorting;
using Common;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;

/// <summary>
/// This class loads the header of the default.map file and provides fields to get the parsed data.
/// If this fails we abort the entire loading process as everything depends on this file.
/// </summary>
public class DefaultMapPreParsingStep(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<DefaultMapDefinition>(dependencies)
{
   public override bool CanBeReloaded => false;

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   DefaultMapDefinition target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes)
      => throw new NotSupportedException("DefaultMapPreParsingStep should not parse to object directly.");

   public override void LoadSingleFile(RootNode rn,
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       object? lockObject)
   {
      var dmd = new DefaultMapDefinition { Source = fileObj };
      ParseProperties(rn, ref pc, fileObj, dmd);

      Globals.DefaultMapDefinition = dmd;
   }

   private static void ParseProperties(RootNode rn,
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       DefaultMapDefinition dmd)
   {
      FileStateManager.RegisterPath(fileObj.Path);

      foreach (var sn in rn.Statements)
         DefaultMapParsing.Dispatch(sn, dmd, ref pc);

      // Set the values for the file names to the matching DescriptorDefinitions
      //DescriptorDefinitions.MapDescriptor.LocalPath[^1] = dmd.ProvinceFileName.TrimQuotes();
      //DescriptorDefinitions.RiverDescriptor.LocalPath[^1] = dmd.Rivers.TrimQuotes();
      //DescriptorDefinitions.HeightMapDescriptor.LocalPath[^1] = dmd.HeightMap.TrimQuotes();
      DescriptorDefinitions.AdjacenciesDescriptor.SetPathFileName(dmd.Adjacencies.TrimQuotes());
      DescriptorDefinitions.DefinitionsDescriptor.SetPathFileName(dmd.Setup.TrimQuotes());
      DescriptorDefinitions.MapTracingDescriptor.SetPathFileName(dmd.ProvinceFileName.TrimQuotes());
      //DescriptorDefinitions.PortsDescriptor.LocalPath[^1] = dmd.Ports.TrimQuotes();
      //DescriptorDefinitions.LocationsTemplatesDescriptor.LocalPath[^1] = dmd.LocationsTemplates.TrimQuotes();
   }
}