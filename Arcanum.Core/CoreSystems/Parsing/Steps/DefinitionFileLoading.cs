using System.Diagnostics;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;
using Region = Arcanum.Core.GameObjects.LocationCollections.Region;

namespace Arcanum.Core.CoreSystems.Parsing.Steps;

public class DefinitionFileLoading : FileLoadingService
{
   private Dictionary<string, Location> _locationCache = null!;
   
   public override string GetFileDataDebugInfo()
   {
      return "Definition File Loading: \n" +
             $"Continents: {Globals.Continents.Count}\n" +
             $"SuperRegions: {Globals.SuperRegions.Count}\n" +
             $"Regions: {Globals.Regions.Count}\n" +
             $"Areas: {Globals.Areas.Count}\n" +
             $"Provinces: {Globals.Provinces.Count}";
   }

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      _locationCache = Globals.Locations.ToDictionary(loc => loc.Name, loc => loc);
      
      var (blocks, content) = ElementParser.GetElements(fileObj.Path);
      var ctx = new LocationContext(0, 0, fileObj.Path.FullPath);

      if (content.Count != 0)
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = content[0].StartLine;
         ErrorManager.AddToLog(new Diagnostic(ParsingError.Instance.ForbiddenElement,
                                              ctx,
                                              DiagnosticSeverity.Error,
                                              GetActionName(),
                                              "Failed to parse definition file content.",
                                              "Please check the file content for errors."));
      }

      var fileInformation = new FileInformation(fileObj.Path.Filename, false, descriptor);

      foreach (var block in blocks)
         ParseContinent(block, ctx, fileInformation);

      _locationCache = null!; 
      return true;
   }

   private void ParseContinent(Block block,
                               LocationContext ctx,
                               FileInformation fileInformation)
   {
      if (block.ContentElements.Count != 0)
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = block.ContentElements[0].StartLine;
         ErrorManager.AddToLog(new Diagnostic(ParsingError.Instance.InvalidContentElementCount,
                                              ctxInstance,
                                              DiagnosticSeverity.Error,
                                              GetActionName(),
                                              "Illegal content element(s); A continent cannot have content elements.",
                                              "Continents cannot have content elements."));
      }

      Continent continent = new(fileInformation, block.Name);
      if (Globals.Continents.Contains(continent))
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = block.StartLine;
         ErrorManager.AddToLog(new Diagnostic(ParsingError.Instance.DuplicateContinentDefinition,
                                              ctxInstance,
                                              DiagnosticSeverity.Error,
                                              GetActionName(),
                                              $"Duplicate continent definition found for '{block.Name}'.",
                                              "Continents must have unique names."));
         return;
      }

      foreach (var subBlock in block.SubBlocks)
         if (ParseSuperRegion(subBlock, ctx, fileInformation, out var superRegion))
         {
            Debug.Assert(superRegion is not null, "SuperRegion should not be null after parsing.");
            continent.Add(superRegion);
         }

      Globals.Continents.Add(continent);
   }

   private bool ParseSuperRegion(Block block,
                                 LocationContext ctx,
                                 FileInformation fileInformation,
                                 out SuperRegion superRegion)
   {
      if (block.ContentElements.Count != 0)
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = block.ContentElements[0].StartLine;
         ErrorManager.AddToLog(new Diagnostic(ParsingError.Instance.InvalidContentElementCount,
                                              ctxInstance,
                                              DiagnosticSeverity.Error,
                                              GetActionName(),
                                              "Illegal content element(s); A super region cannot have content elements.",
                                              "Super regions cannot have content elements."));
      }

      superRegion = new(fileInformation, block.Name);
      if (Globals.SuperRegions.Contains(superRegion))
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = block.StartLine;
         ErrorManager.AddToLog(new Diagnostic(ParsingError.Instance.DuplicateSuperRegionDefinition,
                                              ctxInstance,
                                              DiagnosticSeverity.Error,
                                              GetActionName(),
                                              $"Duplicate super region definition found for '{block.Name}'.",
                                              "Super regions must have unique names."));
         return false;
      }

      foreach (var subBlock in block.SubBlocks)
         if (ParseRegion(subBlock, ctx, fileInformation, out var region))
         {
            Debug.Assert(region is not null, "Region should not be null after parsing.");
            superRegion.Add(region);
         }

      Globals.SuperRegions.Add(superRegion);
      return true;
   }

   private bool ParseRegion(Block block, LocationContext ctx, FileInformation fileInformation, out Region region)
   {
      if (block.ContentElements.Count != 0)
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = block.ContentElements[0].StartLine;
         ErrorManager.AddToLog(new Diagnostic(ParsingError.Instance.InvalidContentElementCount,
                                              ctxInstance,
                                              DiagnosticSeverity.Error,
                                              GetActionName(),
                                              "Illegal content element(s); An area cannot have content elements.",
                                              "Areas cannot have content elements."));
      }

      region = new(fileInformation, block.Name);
      if (Globals.Regions.Contains(region))
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = block.StartLine;
         ErrorManager.AddToLog(new Diagnostic(ParsingError.Instance.DuplicateRegionDefinition,
                                              ctxInstance,
                                              DiagnosticSeverity.Error,
                                              GetActionName(),
                                              $"Duplicate region definition found for '{block.Name}'.",
                                              "Regions must have unique names."));
         return false;
      }

      foreach (var subBlock in block.SubBlocks)
         if (ParseArea(subBlock, ctx, fileInformation, out var area))
         {
            Debug.Assert(area is not null, "Area should not be null after parsing.");
            region.Add(area);
         }

      Globals.Regions.Add(region);
      return true;
   }

   private bool ParseArea(Block block, LocationContext ctx, FileInformation fileInformation, out Area area)
   {
      if (block.ContentElements.Count != 0)
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = block.ContentElements[0].StartLine;
         ErrorManager.AddToLog(new Diagnostic(ParsingError.Instance.InvalidContentElementCount,
                                              ctxInstance,
                                              DiagnosticSeverity.Error,
                                              GetActionName(),
                                              "Illegal content element(s); An area cannot have content elements.",
                                              "Areas cannot have content elements."));
      }

      area = new(fileInformation, block.Name);
      if (Globals.Areas.Contains(area))
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = block.StartLine;
         ErrorManager.AddToLog(new Diagnostic(ParsingError.Instance.DuplicateAreaDefinition,
                                              ctxInstance,
                                              DiagnosticSeverity.Error,
                                              GetActionName(),
                                              $"Duplicate area definition found for '{block.Name}'.",
                                              "Areas must have unique names."));
         return false;
      }

      foreach (var subBlock in block.SubBlocks)
         if (ParseProvince(subBlock, ctx, fileInformation, out var province))
         {
            Debug.Assert(province is not null, "Province should not be null after parsing.");
            area.Add(province);
         }

      Globals.Areas.Add(area);
      return true;
   }

   private bool ParseProvince(Block block, LocationContext ctx, FileInformation fileInformation, out Province? province)
   {
      if (block.SubBlocks.Count != 0)
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = block.SubBlocks[0].StartLine;
         ErrorManager.AddToLog(new Diagnostic(ParsingError.Instance.ForbiddenBlock,
                                              ctxInstance,
                                              DiagnosticSeverity.Error,
                                              GetActionName(),
                                              "Illegal sub-block(s)",
                                              "Provinces cannot have sub-blocks."));
      }

      if (block.ContentElements.Count != 1)
      {
         var tmpCtx = ctx.GetInstance();
         if (block.ContentElements.Count > 1)
            tmpCtx.LineNumber = block.ContentElements[1].StartLine;
         ErrorManager.AddToLog(new Diagnostic(ParsingError.Instance.InvalidContentElementCount,
                                              tmpCtx,
                                              DiagnosticSeverity.Error,
                                              GetActionName(),
                                              "Illegal content element(s)",
                                              "Provinces cannot have content elements."));
      }

      province = new(fileInformation, block.Name);
      if (Globals.Provinces.Contains(province))
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = block.StartLine;
         ErrorManager.AddToLog(new Diagnostic(ParsingError.Instance.DuplicateProvinceDefinition,
                                              ctxInstance,
                                              DiagnosticSeverity.Error,
                                              GetActionName(),
                                              $"Duplicate province definition found for '{block.Name}'.",
                                              "Provinces must have unique names."));
         return false;
      }

      foreach (var (location, lineNum) in block.ContentElements[0].GetStringListEnumerator())
      {
         if (string.IsNullOrWhiteSpace(location) ||
             !_locationCache.TryGetValue(location, out var existingLocation))
         {
            var ctxInstance = ctx.GetInstance();
            ctxInstance.LineNumber = lineNum;
            ErrorManager.AddToLog(new Diagnostic(ParsingError.Instance.InvalidLocationKey,
                                                 ctxInstance,
                                                 DiagnosticSeverity.Error,
                                                 GetActionName(),
                                                 $"Invalid location name '{location}'in the province definition: '{province.Name}'.",
                                                 "Location names must be unique and not empty."));
            continue;
         }

         province.Add(existingLocation);
      }

      Globals.Provinces.Add(province);
      return true;
   }

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
   {
      return true;
   }
}