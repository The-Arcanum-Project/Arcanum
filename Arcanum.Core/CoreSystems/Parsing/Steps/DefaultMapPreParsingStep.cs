using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps;

/// <summary>
/// This class loads the header of the default.map file and provides fields to get the parsed data.
/// If this fails we abort the entire loading process as everything depends on this file.
/// </summary>
public class DefaultMapPreParsingStep : FileLoadingService
{
   public override List<Type> ParsedObjects => [typeof(DefaultMapDefinition)];

   public override string GetFileDataDebugInfo()
   {
      return $"IsValid:{Globals.DefaultMapDefinition.IsValid()}\n" +
             $"\tProvinceFileName:\t{Globals.DefaultMapDefinition.ProvinceFileName}\n" +
             $"\tRivers:\t\t\t{Globals.DefaultMapDefinition.Rivers}\n" +
             $"\tHeightMap:\t\t{Globals.DefaultMapDefinition.HeightMap}\n" +
             $"\tAdjacencies:\t\t{Globals.DefaultMapDefinition.Adjacencies}\n" +
             $"\tSetup:\t\t\t{Globals.DefaultMapDefinition.Setup}\n" +
             $"\tPorts:\t\t\t{Globals.DefaultMapDefinition.Ports}\n" +
             $"\tLocationsTemplates:\t{Globals.DefaultMapDefinition.LocationsTemplates}\n" +
             $"\tWrapX:\t\t\t{Globals.DefaultMapDefinition.WrapX}\n" +
             $"\tEquatorY:\t\t{Globals.DefaultMapDefinition.EquatorY}\n";
   }

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      var (_, elements) = ElementParser.GetElements(fileObj.Path);
      var dmd = new DefaultMapDefinition();

      ParseDefaultMapElements(elements, fileObj.Path, dmd, GetActionName());
      Globals.DefaultMapDefinition = dmd;

      return dmd.IsValid();
   }

   public static void ParseDefaultMapElements(List<Content> contents,
                                              PathObj po,
                                              DefaultMapDefinition dmd,
                                              string actionName)
   {
      if (contents.Count == 0)
      {
         DiagnosticException.LogWarning(LocationContext.Empty,
                                        ParsingError.Instance.InvalidContentElementCount,
                                        actionName,
                                        "at least one",
                                        contents.Count,
                                        string.Empty);
         return;
      }

      foreach (var kvp in contents.SelectMany(content => content.GetLineKvpEnumerator(po)))
         switch (kvp.Key)
         {
            case "provinces":
               dmd.ProvinceFileName = kvp.Value;
               break;
            case "rivers":
               dmd.Rivers = kvp.Value;
               break;
            case "topology":
               dmd.HeightMap = kvp.Value;
               break;
            case "adjacencies":
               dmd.Adjacencies = kvp.Value;
               break;
            case "setup":
               dmd.Setup = kvp.Value;
               break;
            case "ports":
               dmd.Ports = kvp.Value;
               break;
            case "location_templates":
               dmd.LocationsTemplates = kvp.Value;
               break;
            case "wrap_x":
               ValuesParsing.ParseBool(kvp.Value, LocationContext.Empty, actionName, out var wrapX);
               dmd.WrapX = wrapX;
               break;
            case "equator_y":
               if (int.TryParse(kvp.Value, out var equatorY))
                  dmd.EquatorY = equatorY;
               else
                  DiagnosticException.LogWarning(LocationContext.Empty,
                                                 ParsingError.Instance.InvalidIntMarkup,
                                                 actionName,
                                                 kvp.Value);
               break;
            default:
               DiagnosticException.LogWarning(LocationContext.Empty,
                                              ParsingError.Instance.UnknownKeyInDefinition,
                                              actionName,
                                              kvp.Key);
               break;
         }

      if (!dmd.IsValid())
      {
         DiagnosticException.LogWarning(LocationContext.Empty,
                                        ParsingError.Instance.InvalidDefaultMapDefinition,
                                        actionName);
         dmd.SetDefaultValues();
      }

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

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
   {
      Globals.DefaultMapDefinition.Adjacencies = string.Empty;
      Globals.DefaultMapDefinition.HeightMap = string.Empty;
      Globals.DefaultMapDefinition.ProvinceFileName = string.Empty;
      Globals.DefaultMapDefinition.Rivers = string.Empty;
      Globals.DefaultMapDefinition.Setup = string.Empty;
      Globals.DefaultMapDefinition.Ports = string.Empty;
      Globals.DefaultMapDefinition.LocationsTemplates = string.Empty;
      Globals.DefaultMapDefinition.WrapX = false;
      Globals.DefaultMapDefinition.EquatorY = -1;
      return true;
   }
}