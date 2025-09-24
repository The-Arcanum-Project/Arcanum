using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public abstract class DiscoverThenParseLoadingService<T>(bool isDiscoveryPhase, IEnumerable<IDependencyNode<string>> dependencies) : ParserValidationLoadingService<T>(dependencies)
   where T : IEu5Object<T>, new()
{
   private bool IsDiscoveryPhase { get; } = isDiscoveryPhase;

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
      if (IsDiscoveryPhase)
         DiscoverObjects(rn, ctx, fileObj, actionStack, source, ref validation, lockObject);
      else
         LoadSingleFileProperties(rn, ctx, fileObj, actionStack, source, ref validation, lockObject);
   }

   /// <summary>
   /// In this method only discover the objects and initialize the global collections.
   /// The actual parsing of the properties is done in <see cref="LoadSingleFileProperties"/>.
   /// </summary>
   protected virtual void DiscoverObjects(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
      if (!ParsingMaster.RemoveAllGroupingNodes(rn,
                                                ctx,
                                                actionStack,
                                                source,
                                                ref validation,
                                                GroupingNodeNames,
                                                out var sns))
         return;

      SimpleObjectParser.DiscoverObjectDeclarations(sns,
                                                    ctx,
                                                    fileObj,
                                                    actionStack,
                                                    source,
                                                    ref validation,
                                                    GetGlobals(),
                                                    lockObject);
   }

   /// <summary>
   /// Do not create new objects here, only parse the properties of the objects discovered in <see cref="DiscoverObjects"/>.
   /// </summary>
   protected virtual void LoadSingleFileProperties(RootNode rn,
                                                   LocationContext ctx,
                                                   Eu5FileObj fileObj,
                                                   string actionStack,
                                                   string source,
                                                   ref bool validation,
                                                   object? lockObject)
   {
   }
}