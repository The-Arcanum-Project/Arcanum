using System.Diagnostics;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;
using Region = Arcanum.Core.GameObjects.LocationCollections.Region;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;

public class DefinitionFileLoading : FileLoadingService
{
   public override List<Type> ParsedObjects =>
   [
      typeof(Continent), typeof(SuperRegion), typeof(Region), typeof(Area), typeof(Province),
   ];

   public override string GetFileDataDebugInfo()
   {
      return "Definition File Loading: \n" +
             $"\t\tContinents: {Globals.Continents.Count}\n" +
             $"\t\tSuperRegions: {Globals.SuperRegions.Count}\n" +
             $"\t\tRegions: {Globals.Regions.Count}\n" +
             $"\t\tAreas: {Globals.Areas.Count}\n" +
             $"\t\tProvinces: {Globals.Provinces.Count}";
   }

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      var (blocks, content) = ElementParser.GetElements(fileObj.Path);
      var ctx = new LocationContext(0, 0, fileObj.Path.FullPath);

      if (content.Count != 0)
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = content[0].StartLine;
         DiagnosticException.LogWarning(ctxInstance,
                                        ParsingError.Instance.ForbiddenElement,
                                        GetActionName());
      }

      var fileInformation = new FileInformation(fileObj.Path.Filename, false, descriptor);

      foreach (var block in blocks)
         ParseContinent(block, ctx, fileInformation);

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
         DiagnosticException.LogWarning(ctxInstance,
                                        ParsingError.Instance.InvalidContentElementCount,
                                        GetActionName(),
                                        0,
                                        block.ContentElements.Count,
                                        block.ContentElements[0].ToString());
      }

      Continent continent = new(fileInformation, block.Name);
      if (Globals.Continents.ContainsKey(continent.Name))
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = block.StartLine;
         DiagnosticException.LogWarning(ctxInstance,
                                        ParsingError.Instance.DuplicateContinentDefinition,
                                        GetActionName(),
                                        block.Name);

         return;
      }

      foreach (var subBlock in block.SubBlocks)
         if (ParseSuperRegion(subBlock, ctx, fileInformation, out var superRegion))
         {
            Debug.Assert(superRegion is not null, "SuperRegion should not be null after parsing.");
            continent.Add(superRegion);
         }

      Globals.Continents.Add(continent.Name, continent);
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
         DiagnosticException.LogWarning(ctxInstance,
                                        ParsingError.Instance.InvalidContentElementCount,
                                        GetActionName(),
                                        0,
                                        block.ContentElements.Count,
                                        block.ContentElements[0].ToString());
      }

      superRegion = new(fileInformation, block.Name);
      if (Globals.SuperRegions.ContainsKey(superRegion.Name))
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = block.StartLine;
         DiagnosticException.LogWarning(ctxInstance,
                                        ParsingError.Instance.DuplicateSuperRegionDefinition,
                                        GetActionName(),
                                        block.Name);
         return false;
      }

      foreach (var subBlock in block.SubBlocks)
         if (ParseRegion(subBlock, ctx, fileInformation, out var region))
         {
            Debug.Assert(region is not null, "Region should not be null after parsing.");
            superRegion.Add(region);
         }

      Globals.SuperRegions.Add(superRegion.Name, superRegion);
      return true;
   }

   private bool ParseRegion(Block block, LocationContext ctx, FileInformation fileInformation, out Region region)
   {
      if (block.ContentElements.Count != 0)
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = block.ContentElements[0].StartLine;
         DiagnosticException.LogWarning(ctxInstance,
                                        ParsingError.Instance.InvalidContentElementCount,
                                        GetActionName(),
                                        0,
                                        block.ContentElements.Count,
                                        block.ContentElements[0].ToString());
      }

      region = new(fileInformation, block.Name);
      if (Globals.Regions.ContainsKey(region.Name))
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = block.StartLine;
         DiagnosticException.LogWarning(ctxInstance,
                                        ParsingError.Instance.DuplicateRegionDefinition,
                                        GetActionName(),
                                        block.Name);
         return false;
      }

      foreach (var subBlock in block.SubBlocks)
         if (ParseArea(subBlock, ctx, fileInformation, out var area))
         {
            Debug.Assert(area is not null, "Area should not be null after parsing.");
            region.Add(area);
         }

      Globals.Regions.Add(region.Name, region);
      return true;
   }

   private bool ParseArea(Block block, LocationContext ctx, FileInformation fileInformation, out Area area)
   {
      if (block.ContentElements.Count != 0)
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = block.ContentElements[0].StartLine;
         DiagnosticException.LogWarning(ctxInstance,
                                        ParsingError.Instance.InvalidContentElementCount,
                                        GetActionName(),
                                        0,
                                        block.ContentElements.Count,
                                        block.ContentElements[0].ToString());
      }

      area = new(fileInformation, block.Name);
      if (Globals.Areas.ContainsKey(area.Name))
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = block.StartLine;
         DiagnosticException.LogWarning(ctxInstance,
                                        ParsingError.Instance.DuplicateAreaDefinition,
                                        GetActionName(),
                                        block.Name);
         return false;
      }

      foreach (var subBlock in block.SubBlocks)
         if (ParseProvince(subBlock, ctx, fileInformation, out var province))
         {
            Debug.Assert(province is not null, "Province should not be null after parsing.");
            area.Add(province);
         }

      Globals.Areas.Add(area.Name, area);
      return true;
   }

   private bool ParseProvince(Block block, LocationContext ctx, FileInformation fileInformation, out Province? province)
   {
      if (block.SubBlocks.Count != 0)
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = block.SubBlocks[0].StartLine;
         DiagnosticException.LogWarning(ctxInstance,
                                        ParsingError.Instance.ForbiddenBlock,
                                        GetActionName());
      }

      if (block.ContentElements.Count != 1)
      {
         var tmpCtx = ctx.GetInstance();
         if (block.ContentElements.Count > 1)
            tmpCtx.LineNumber = block.ContentElements[1].StartLine;
         DiagnosticException.LogWarning(tmpCtx,
                                        ParsingError.Instance.InvalidContentElementCount,
                                        GetActionName(),
                                        0,
                                        block.ContentElements.Count,
                                        block.ContentElements.Count.ToString());
      }

      province = new(fileInformation, block.Name);
      if (Globals.Provinces.ContainsKey(province.Name))
      {
         var ctxInstance = ctx.GetInstance();
         ctxInstance.LineNumber = block.StartLine;
         DiagnosticException.LogWarning(ctxInstance,
                                        ParsingError.Instance.DuplicateProvinceDefinition,
                                        GetActionName(),
                                        block.Name);
         return false;
      }

      foreach (var (location, lineNum) in block.ContentElements[0].GetStringListEnumerator())
      {
         if (string.IsNullOrWhiteSpace(location) ||
             !Globals.Locations.TryGetValue(location, out var existingLocation))
         {
            var ctxInstance = ctx.GetInstance();
            ctxInstance.LineNumber = lineNum;
            DiagnosticException.LogWarning(ctxInstance,
                                           ParsingError.Instance.InvalidLocationKey,
                                           GetActionName(),
                                           location);
            continue;
         }

         province.Add(existingLocation);
      }

      Globals.Provinces.Add(province.Name, province);
      return true;
   }

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
   {
      Globals.Areas.Clear();
      Globals.Continents.Clear();
      Globals.Provinces.Clear();
      Globals.Regions.Clear();
      Globals.SuperRegions.Clear();
      return true;
   }
}