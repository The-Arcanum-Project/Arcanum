using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public abstract class DiscoverThenParseLoadingService<T> : ParserValidationLoadingService<T>
   where T : IEu5Object<T>, new()
{
   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj<T> fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
      DiscoverObjects(rn, ctx, fileObj, actionStack, source, ref validation, lockObject);
      LoadSingleFileProperties(rn, ctx, fileObj, actionStack, source, ref validation, lockObject);
   }

   /// <summary>
   /// In this method only discover the objects and initialize the global collections.
   /// The actual parsing of the properties is done in <see cref="LoadSingleFileProperties"/>.
   /// </summary>
   /// <param name="rn"></param>
   /// <param name="ctx"></param>
   /// <param name="fileObj"></param>
   /// <param name="actionStack"></param>
   /// <param name="source"></param>
   /// <param name="validation"></param>
   /// <param name="lockObject"></param>
   protected virtual void DiscoverObjects(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj<T> fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
      if (!RemoveAllGroupingNodes(rn, ctx, actionStack, source, ref validation, out var sns))
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
   /// <param name="rn"></param>
   /// <param name="ctx"></param>
   /// <param name="fileObj"></param>
   /// <param name="actionStack"></param>
   /// <param name="source"></param>
   /// <param name="validation"></param>
   /// <param name="lockObject"></param>
   protected abstract void LoadSingleFileProperties(RootNode rn,
                                                    LocationContext ctx,
                                                    Eu5FileObj<T> fileObj,
                                                    string actionStack,
                                                    string source,
                                                    ref bool validation,
                                                    object? lockObject);
}