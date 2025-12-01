using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.LocationCollections.SubObjects;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Setup;

[ParserFor(typeof(CountryDefinition))]
public partial class CountryDefinitionParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<CountryDefinition>(dependencies)
{
   public override void LoadSingleFile(RootNode rn,
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       object? lockObject)
   {
      SimpleObjectParser.Parse(fileObj,
                               rn,
                               ref pc,
                               ParseProperties,
                               GetGlobals(),
                               lockObject);
   }

   public override bool AfterLoadingStep(FileDescriptor descriptor)
   {
      // We create the dictionary with a small overhead to avoid rehashing.
      var countries = new Dictionary<string, Country>((int)(Globals.CountryDefinitions.Count * 1.005f));
      // We create the countries here.
      foreach (var def in Globals.CountryDefinitions.Values)
      {
         if (countries.TryAdd(def.UniqueId,
                              new()
                              {
                                 UniqueId = def.UniqueId, Definition = def,
                              }))
            continue;

         De.Warning(def.FileLocation.ToLocationContext(def.Source),
                    ParsingError.Instance.DuplicateObjectDefinition,
                    $"{nameof(CountryDefinitionParsing)}.AssignDefinitions",
                    def.UniqueId,
                    nameof(Country));
      }

      Globals.Countries = countries;
      return true;
   }

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, object? lockObject)
   {
      var result = base.UnloadSingleFileContent(fileObj, lockObject);
      foreach (var obj in fileObj.ObjectsInFile)
         if (Globals.Countries.TryGetValue(obj.UniqueId, out var country))
            country.Definition = CountryDefinition.Empty;
      return result;
   }

   public override void ReloadSingleFile(Eu5FileObj fileObj,
                                         object? lockObject)
   {
      base.ReloadSingleFile(fileObj, lockObject);
      foreach (var obj in fileObj.ObjectsInFile)
         if (Globals.Countries.TryGetValue(obj.UniqueId, out var country) &&
             country.Definition == CountryDefinition.Empty)
         {
            // Re-linking the country definition failed. Abort.
            return;
         }
   }

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   CountryDefinition target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes) => ParseProperties(block, target, ref pc, allowUnknownNodes);
}