using System.Collections;
using System.Reflection;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public abstract class GeneralFileLoadingService(IEnumerable<IDependencyNode<string>> dependencies)
   : FileLoadingService(dependencies)
{
   private const string ACTION_STACK = nameof(GeneralFileLoadingService);

   public override bool LoadSingleFile(Eu5FileObj fileObj, object? lockObject)
   {
      var rn = Parser.Parse(fileObj, out var source, out var ctx);
      var validation = true;
      var pc = new ParsingContext(ctx, source, ACTION_STACK, ref validation);

      LoadSingleFile(rn, ref pc, fileObj, lockObject);

      return validation;
   }

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      return UnloadSingleFileContent(fileObj, lockObject);
   }

   public abstract bool UnloadSingleFileContent(Eu5FileObj fileObj, object? lockObject);

   protected static void ResetObjectProperties(IEu5Object eu5Obj)
   {
      foreach (var prop in eu5Obj.GetAllProperties())
      {
         if (prop.ToString() == "UniqueId")
            continue;

         // TODO: Make this an internal method in each object so we can disable the resetting via a flag in the PropertyConfig attribute
         eu5Obj._setValue(prop, eu5Obj.GetDefaultValue(prop));
      }
   }

   public override string GetFileDataDebugInfo()
   {
      var str = "Parsed File Data Summary:\n";

      foreach (var type in ParsedObjects)
      {
         // TODO this can use the GetGlobalItemsNonGeneric method from IEu5Object
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

   public abstract void LoadSingleFile(RootNode rn,
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       object? lockObject);
}