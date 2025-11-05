using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
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
   : FileLoadingService(dependencies) where T : IEu5Object<T>, new()
{
   private const string ACTION_STACK = nameof(ParserValidationLoadingService<T>);

   public virtual string[] GroupingNodeNames => [];
   public virtual Dictionary<string, T> GetGlobals() => T.GetGlobalItems();

   public override List<Type> ParsedObjects => [typeof(T)];

   public override string GetFileDataDebugInfo()
   {
      var str = "Parsed File Data Summary:\n";

      foreach (var type in ParsedObjects)
      {
         var getGlobalItemsMethod = type.GetMethod("GetGlobalItems",
                                                   BindingFlags.Public | BindingFlags.Static);

         string countStr;

         if (getGlobalItemsMethod != null)
         {
            var collectionAsObject = getGlobalItemsMethod.Invoke(null, null);
            countStr = collectionAsObject switch
            {
               ICollection collection => collection.Count.ToString(),
               IEnumerable enumerable => enumerable.Cast<object>().Count().ToString(),
               _ => "[Returned null or not a collection]",
            };
         }
         else
            countStr = "[Method Not Found]";

         str += $"\t\t{type.Name}: {countStr}\n";
      }

      return str;
   }

   public override bool LoadSingleFile(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      var rn = Parser.Parse(fileObj, out var source, out var ctx);
      var validation = true;

      LoadSingleFile(rn, ctx, fileObj, ACTION_STACK, source, ref validation, lockObject);

      return validation;
   }

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      return UnloadSingleFileContent(fileObj, lockObject);
   }

   protected virtual bool UnloadSingleFileContent(Eu5FileObj fileObj, object? lockObject)
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

   protected abstract void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject);

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

   protected virtual void ResetObjectProperties(IEu5Object eu5Obj)
   {
      foreach (var prop in eu5Obj.GetAllProperties())
      {
         if (prop.ToString() == "UniqueId" || prop.ToString() == "Source")
            continue;

         var defaultVal = eu5Obj.GetDefaultValue(prop);
         eu5Obj._setValue(prop, defaultVal);
      }
   }
}