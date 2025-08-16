using System.ComponentModel;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Utils.PropertyHelpers;

namespace Arcanum.Core.CoreSystems.Parsing.Steps;

public class DefaultMapParsing : FileLoadingService
{
   public override string GetFileDataDebugInfo() => throw new NotImplementedException();

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      var (blocks, contents) = ElementParser.GetElements(fileObj.Path);
      var ctx = new LocationContext(0, 0, fileObj.Path.FullPath);

      if (blocks.Count != 7)
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidBlockCount,
                                        GetActionName(),
                                        fileObj.Path.FullPath,
                                        7,
                                        blocks.Count);

      DefaultMapDefinition dmd = new();

      ParseDefaultMapElements(contents, fileObj.Path, dmd);

      foreach (var block in blocks)
      {
         if (block.SubBlocks.Count != 0)
         {
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.InvalidBlockCount,
                                           GetActionName(),
                                           block.Name,
                                           0,
                                           block.SubBlocks.Count);
            continue;
         }

         ctx.LineNumber = block.StartLine;
         dmd.SetCollection(block.Name, ParsingUtil.ParseLocationList(block.ContentElements[0], ctx), ctx);
      }

      return true;
   }

   private void ParseDefaultMapElements(List<Content> contents, PathObj po, DefaultMapDefinition dmd)
   {
      if (contents.Count == 0)
      {
         DiagnosticException.LogWarning(LocationContext.Empty,
                                        ParsingError.Instance.InvalidContentElementCount,
                                        GetActionName(),
                                        "at least one",
                                        0);
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
               if (ParsingUtil.TryParseBool(kvp.Value, out var wrapX))
                  dmd.WrapX = wrapX;
               else
                  DiagnosticException.LogWarning(LocationContext.Empty,
                                                 ParsingError.Instance.BoolParsingError,
                                                 GetActionName(),
                                                 nameof(DefaultMapDefinition.WrapX),
                                                 kvp.Value);
               break;
            case "equator_y":
               if (int.TryParse(kvp.Value, out var equatorY))
                  dmd.EquatorY = equatorY;
               else
                  DiagnosticException.LogWarning(LocationContext.Empty,
                                                 ParsingError.Instance.IntParsingError,
                                                 GetActionName(),
                                                 nameof(DefaultMapDefinition.EquatorY),
                                                 kvp.Value);
               break;
            default:
               DiagnosticException.LogWarning(LocationContext.Empty,
                                              ParsingError.Instance.UnknownKeyInDefinition,
                                              GetActionName(),
                                              kvp.Key,
                                              po.FullPath);
               break;
         }

      if (!dmd.IsValid())
      {
         DiagnosticException.LogWarning(LocationContext.Empty,
                                        ParsingError.Instance.InvalidDefaultMapDefinition,
                                        GetActionName(),
                                        po.FullPath);
         dmd.SetDefaultValues();
      }
      Globals.DefaultMapDefinition = dmd;
   }

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
   {
      Globals.DefaultMapDefinition = new ();
      return true;
   }
}

public class DefaultMapDefinition
{
   [DefaultValue("locations.png")]
   public string ProvinceFileName { get; set; } = string.Empty;
   [DefaultValue("rivers.png")]
   public string Rivers { get; set; } = string.Empty;
   [DefaultValue("heightmap.heightmap")]
   public string HeightMap { get; set; } = string.Empty;
   [DefaultValue("adjacencies.csv")]
   public string Adjacencies { get; set; } = string.Empty;
   [DefaultValue("definitions.txt")]
   public string Setup { get; set; } = string.Empty;
   [DefaultValue("ports.csv")]
   public string Ports { get; set; } = string.Empty;
   [DefaultValue("locations_templates.csv")]
   public string LocationsTemplates { get; set; } = string.Empty;

   public bool WrapX { get; set; } = true;
   public int EquatorY { get; set; } = -1;

   public HashSet<Location> SoundTolls { get; set; } = [];
   public HashSet<Location> Volcanoes { get; set; } = [];
   public HashSet<Location> Earthquakes { get; set; } = [];
   public HashSet<Location> SeaZones { get; set; } = [];
   public HashSet<Location> Lakes { get; set; } = [];
   public HashSet<Location> NotOwnable { get; set; } = [];
   public HashSet<Location> ImpassableMountains { get; set; } = [];

   public void SetCollection(string collectionName, List<Location> locations, LocationContext ctx)
   {
      switch (collectionName.ToLowerInvariant())
      {
         case "sound_toll":
            SoundTolls = new(locations);
            break;
         case "volcanoes":
            Volcanoes = new(locations);
            break;
         case "earthquakes":
            Earthquakes = new(locations);
            break;
         case "sea_zones":
            SeaZones = new(locations);
            break;
         case "lakes":
            Lakes = new(locations);
            break;
         case "impassable_mountains":
            ImpassableMountains = new(locations);
            break;
         case "non_ownable":
            NotOwnable = new(locations);
            break;
         default:
            DiagnosticException.LogWarning(ctx,
                                           ParsingError.Instance.UnknownDefaultMapCollectionName,
                                           nameof(DefaultMapDefinition),
                                           collectionName);
            break;
      }
   }

   public bool IsValid()
   {
      return !string.IsNullOrWhiteSpace(ProvinceFileName) &&
             !string.IsNullOrWhiteSpace(Rivers) &&
             !string.IsNullOrWhiteSpace(HeightMap) &&
             !string.IsNullOrWhiteSpace(Adjacencies) &&
             !string.IsNullOrWhiteSpace(Setup) &&
             !string.IsNullOrWhiteSpace(Ports) &&
             !string.IsNullOrWhiteSpace(LocationsTemplates) &&
             EquatorY >= 0;
   }

   public void SetDefaultValues()
   {
      this.SetAllPropsToDefault();
   }
}