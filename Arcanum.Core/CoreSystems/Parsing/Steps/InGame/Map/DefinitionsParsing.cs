using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;
using Region = Arcanum.Core.GameObjects.LocationCollections.Region;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;

[ParserFor(typeof(Continent))]
public partial class DefinitionsParsing : ParserValidationLoadingService<Continent>
{
   public override List<Type> ParsedObjects
      => [typeof(Continent), typeof(SuperRegion), typeof(Region), typeof(Area), typeof(Province)];

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      Globals.Continents.Clear();
      Globals.SuperRegions.Clear();
      Globals.Regions.Clear();
      Globals.Areas.Clear();
      Globals.Provinces.Clear();
      return true;
   }

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
      var continentGlobals = Continent.GetGlobalItems();
      var superRegionGlobals = SuperRegion.GetGlobalItems();
      var regionGlobals = Region.GetGlobalItems();
      var areaGlobals = Area.GetGlobalItems();
      var provinceGlobals = Province.GetGlobalItems();
      var locationGlobals = Location.GetGlobalItems();

      foreach (var sn in rn.Statements)
      {
         // Continent level locations
         if (!sn.IsBlockNode(ctx, source, actionStack, ref validation, out var contBn))
            continue;

         var continentKey = contBn.KeyNode.GetLexeme(source);
         var continent = IEu5Object<Continent>.CreateInstance(continentKey, fileObj);
         LUtil.TryAddToGlobals(ctx,
                               contBn.KeyNode,
                               continentKey,
                               actionStack,
                               ref validation,
                               continent,
                               continentGlobals);

         foreach (var superRegionSn in contBn.Children)
         {
            // SuperRegion level locations
            if (!superRegionSn.IsBlockNode(ctx, source, actionStack, ref validation, out var srBn))
               continue;

            var superRegionKey = srBn.KeyNode.GetLexeme(source);
            var superRegion = IEu5Object<SuperRegion>.CreateInstance(superRegionKey, fileObj);
            LUtil.TryAddToGlobals(ctx,
                                  srBn.KeyNode,
                                  superRegionKey,
                                  actionStack,
                                  ref validation,
                                  superRegion,
                                  superRegionGlobals);
            continent.LocationChildren.Add(superRegion);
            superRegion.Parents.Add(continent);

            foreach (var regionSn in srBn.Children)
            {
               // Region level locations
               if (!regionSn.IsBlockNode(ctx, source, actionStack, ref validation, out var rBn))
                  continue;

               var regionKey = rBn.KeyNode.GetLexeme(source);
               var region = IEu5Object<Region>.CreateInstance(regionKey, fileObj);
               LUtil.TryAddToGlobals(ctx, rBn.KeyNode, regionKey, actionStack, ref validation, region, regionGlobals);
               superRegion.LocationChildren.Add(region);
               region.Parents.Add(superRegion);

               foreach (var areaSn in rBn.Children)
               {
                  // Area level locations
                  if (!areaSn.IsBlockNode(ctx, source, actionStack, ref validation, out var aBn))
                     continue;

                  var areaKey = aBn.KeyNode.GetLexeme(source);
                  var area = IEu5Object<Area>.CreateInstance(areaKey, fileObj);
                  LUtil.TryAddToGlobals(ctx, aBn.KeyNode, areaKey, actionStack, ref validation, area, areaGlobals);
                  region.LocationChildren.Add(area);
                  area.Parents.Add(region);

                  foreach (var provinceSn in aBn.Children)
                  {
                     // Province level locations
                     if (!provinceSn.IsBlockNode(ctx, source, actionStack, ref validation, out var pBn))
                        continue;

                     var provinceKey = pBn.KeyNode.GetLexeme(source);
                     var province = IEu5Object<Province>.CreateInstance(provinceKey, fileObj);
                     LUtil.TryAddToGlobals(ctx,
                                           pBn.KeyNode,
                                           provinceKey,
                                           actionStack,
                                           ref validation,
                                           province,
                                           provinceGlobals);
                     area.LocationChildren.Add(province);
                     province.Parents.Add(area);

                     foreach (var locationSn in pBn.Children)
                     {
                        // Actual locations
                        if (!locationSn.IsKeyOnlyNode(ctx, source, actionStack, ref validation, out var kNode))
                           continue;

                        if (!LUtil.ParseLocation(kNode, ctx, actionStack, source, out var location))
                           continue;

                        province.LocationChildren.Add(location);
                        location.Parents.Add(province);
                     }
                  }
               }
            }
         }
      }
   }
}