using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public abstract class ParserValidationLoadingService<T> : FileLoadingService where T : IEu5Object<T>, new()
{
   private const string ACTION_STACK = nameof(ParserValidationLoadingService<T>);

   public override List<Type> ParsedObjects => [typeof(T)];
   public virtual string[] GroupingNodeNames => [];
   public virtual Dictionary<string, T> GetGlobals() => T.GetGlobalItems();

   public override string GetFileDataDebugInfo() => $"Parsed Climates: {GetGlobals().Count}";

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      var rn = Parser.Parse(fileObj, out var source, out var ctx);
      var validation = true;

      LoadSingleFile(rn, ctx, new(fileObj.Path, fileObj.Descriptor), ACTION_STACK, source, ref validation, lockObject);

      return validation;
   }

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      return UnloadSingleFileContent(new(fileObj.Path, fileObj.Descriptor), lockObject);
   }

   protected virtual bool UnloadSingleFileContent(Eu5FileObj<T> fileObj, object? lockObject)
   {
      var globals = (Dictionary<string, T>)T.GetGlobalItems();
      if (lockObject != null)
         lock (lockObject)
            foreach (var obj in fileObj.GetEu5Objects())
               globals.Remove(obj.UniqueId);
      else
         foreach (var obj in fileObj.GetEu5Objects())
            globals.Remove(obj.UniqueId);
      return true;
   }

   protected abstract void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj<T> fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject);

   protected bool RemoveAllGroupingNodes(RootNode rn,
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
}