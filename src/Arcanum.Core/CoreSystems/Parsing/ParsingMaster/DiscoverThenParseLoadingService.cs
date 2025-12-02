using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public abstract class DiscoverThenParseLoadingService<T>(bool isDiscoveryPhase,
                                                         IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<T>(dependencies)
   where T : IEu5Object<T>, new()
{
   private bool IsDiscoveryPhase { get; } = isDiscoveryPhase;

   public override void LoadSingleFile(RootNode rn,
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       object? lockObject)
   {
      if (IsDiscoveryPhase)
         DiscoverObjects(rn, ref pc, fileObj, lockObject);
      else
         LoadSingleFileProperties(rn, ref pc, fileObj, lockObject);
   }

   /// <summary>
   /// In this method only discover the objects and initialize the global collections.
   /// The actual parsing of the properties is done in <see cref="LoadSingleFileProperties"/>.
   /// </summary>
   protected virtual void DiscoverObjects(RootNode rn,
                                          ref ParsingContext pc,
                                          Eu5FileObj fileObj,
                                          object? lockObject)
   {
      if (!ParsingMaster.RemoveAllGroupingNodes(rn,
                                                ref pc,
                                                GroupingNodeNames,
                                                out var sns))
         return;

      SimpleObjectParser.DiscoverObjectDeclarations(sns,
                                                    fileObj,
                                                    ref pc,
                                                    GetGlobals(),
                                                    lockObject);
   }

   /// <summary>
   /// Do not create new objects here, only parse the properties of the objects discovered in <see cref="DiscoverObjects"/>.
   /// </summary>
   protected virtual void LoadSingleFileProperties(RootNode rn,
                                                   ref ParsingContext pc,
                                                   Eu5FileObj fileObj,
                                                   object? lockObject)
   {
   }
}