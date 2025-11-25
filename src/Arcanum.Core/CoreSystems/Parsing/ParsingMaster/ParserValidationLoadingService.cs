using System.Diagnostics;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public abstract class
   ParserValidationLoadingService<T>(IEnumerable<IDependencyNode<string>> dependencies)
   : GeneralFileLoadingService(dependencies) where T : IEu5Object<T>, new()
{
   public virtual string[] GroupingNodeNames => [];
   public virtual Dictionary<string, T> GetGlobals() => T.GetGlobalItems();

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, object? lockObject)
   {
      var globals = GetGlobals();
      if (lockObject != null)
         lock (lockObject)
            foreach (var obj in fileObj.ObjectsInFile)
               globals.Remove(obj.UniqueId);
      else
         foreach (var obj in fileObj.ObjectsInFile)
            globals.Remove(obj.UniqueId);
      return true;
   }

   public override List<Type> ParsedObjects => [typeof(T)];

   public override void ReloadSingleFile(Eu5FileObj fileObj,
                                         object? lockObject,
                                         string actionStack,
                                         ref bool validation)
   {
      if (!CanBeReloaded)
         return;

      var objectsInFile = new HashSet<IEu5Object>(fileObj.ObjectsInFile);
      var globals = GetGlobals();

      foreach (var obj in objectsInFile)
         ResetObjectProperties(obj);

      var rn = Parser.Parse(fileObj, out var source, out var ctx);

      if (!ParsingMaster.RemoveAllGroupingNodes(rn,
                                                ctx,
                                                actionStack,
                                                source,
                                                ref validation,
                                                GroupingNodeNames,
                                                out var sns))
         return;

      foreach (var sn in sns)
      {
         if (!sn.IsBlockNode(ctx, source, actionStack, ref validation, out var bn))
            continue;

         var key = bn.KeyNode.GetLexeme(source);
         if (!globals.TryGetValue(key, out var target))
         {
            var instance = Eu5Activator.CreateInstance<T>(bn.KeyNode.GetLexeme(source), fileObj, bn);
            Debug.Assert(instance.Source != null, "instance.Source != null");

            if (lockObject != null)
               lock (lockObject)
                  globals.Add(instance.UniqueId, instance);
            else
               globals.Add(instance.UniqueId, instance);
         }
         else
         {
            ParsePropertiesToObject(bn, target, ctx, source, ref validation, allowUnknownNodes: false);
            objectsInFile.Remove(target);
         }
      }

      // We check if all objects we had before are still present if not we throw an error as this can cause corruption.
      foreach (var obj in objectsInFile)
         De.Warning(ctx,
                    ParsingError.Instance.MissingObjectAfterReload,
                    actionStack,
                    obj.UniqueId,
                    typeof(T).Name);
   }

   protected abstract void ParsePropertiesToObject(BlockNode block,
                                                   T target,
                                                   LocationContext ctx,
                                                   string source,
                                                   ref bool validation,
                                                   bool allowUnknownNodes);
}