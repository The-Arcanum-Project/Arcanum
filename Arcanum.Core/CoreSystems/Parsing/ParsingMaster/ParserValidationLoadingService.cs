using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public abstract class ParserValidationLoadingService<T> : FileLoadingService where T : IEu5Object<T>, new()
{
   private const string ACTION_STACK = nameof(ParserValidationLoadingService<T>);

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

   protected abstract bool UnloadSingleFileContent(Eu5FileObj<T> fileObj, object? lockObject);

   protected abstract void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj<T> fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject);
}