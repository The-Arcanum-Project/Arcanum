using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public abstract class DiscoverThenParseLoadingService<T> : ParserValidationLoadingService<T>
   where T : IEu5Object<T>, new()
{
   public override List<Type> ParsedObjects => [typeof(T)];
   public abstract Dictionary<string, T> GetGlobals();
   public virtual string[] GroupingNodeNames => [];

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

   private bool RemoveAllGroupingNodes(RootNode rn,
                                       LocationContext ctx,
                                       string actionStack,
                                       string source,
                                       ref bool validation,
                                       out List<StatementNode> sns)
   {
      if (GroupingNodeNames.Length == 0)
      {
         sns = rn.Statements;
         return true;
      }

      if (!SimpleObjectParser.StripGroupingNodes(rn,
                                                 ctx,
                                                 actionStack,
                                                 source,
                                                 ref validation,
                                                 GroupingNodeNames[0],
                                                 out sns))
         return false;

      for (var i = 1; i < GroupingNodeNames.Length; i++)
      {
         if (sns.Count != 1 || !sns[0].IsBlockNode(ctx, source, actionStack, out var bn))
            continue;

         if (!SimpleObjectParser.StripGroupingNodes(bn!,
                                                    ctx,
                                                    actionStack,
                                                    source,
                                                    ref validation,
                                                    GroupingNodeNames[i],
                                                    out sns))
            return false;
      }

      return true;
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

   public override string GetFileDataDebugInfo() => $"Parsed {nameof(T)}: {GetGlobals().Count}";
}