using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
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
}