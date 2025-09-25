using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Map;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;

/// <summary>
/// This class loads the header of the default.map file and provides fields to get the parsed data.
/// If this fails we abort the entire loading process as everything depends on this file.
/// </summary>
public class DefaultMapPreParsingStep : ParserValidationLoadingService<DefaultMapDefinition>
{
   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
      var dmd = new DefaultMapDefinition { Source = fileObj, };

      foreach (var sn in rn.Statements)
         if (sn is ContentNode cn)
            Pdh.DispatchContentNode(cn,
                                    dmd,
                                    ctx,
                                    source,
                                    actionStack,
                                    DefaultMapParsing._contentParsers,
                                    ref validation);

      // Set the values for the file names to the matching DescriptorDefinitions
      //DescriptorDefinitions.MapDescriptor.LocalPath[^1] = dmd.ProvinceFileName.TrimQuotes();
      //DescriptorDefinitions.RiverDescriptor.LocalPath[^1] = dmd.Rivers.TrimQuotes();
      //DescriptorDefinitions.HeightMapDescriptor.LocalPath[^1] = dmd.HeightMap.TrimQuotes();
      DescriptorDefinitions.AdjacenciesDescriptor.SetPathFileName(dmd.Adjacencies.TrimQuotes());
      DescriptorDefinitions.DefinitionsDescriptor.SetPathFileName(dmd.Setup.TrimQuotes());
      //DescriptorDefinitions.PortsDescriptor.LocalPath[^1] = dmd.Ports.TrimQuotes();
      //DescriptorDefinitions.LocationsTemplatesDescriptor.LocalPath[^1] = dmd.LocationsTemplates.TrimQuotes();

      Globals.DefaultMapDefinition = dmd;
   }
}