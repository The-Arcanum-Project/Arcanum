using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;
using Language = Arcanum.Core.GameObjects.Cultural.Language;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(Language))]
public partial class LanguageParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<Language>(dependencies)
{
   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
      SimpleObjectParser.Parse(fileObj,
                               rn,
                               ctx,
                               actionStack,
                               source,
                               ref validation,
                               ParseProperties,
                               Globals.Languages,
                               lockObject);
   }

   public override bool AfterLoadingStep(FileDescriptor descriptor)
   {
      foreach (var lang in GetGlobals().Values)
         foreach (var dialect in lang.Dialects)
            if (!Globals.Languages.TryGetValue(dialect.UniqueId, out _))
               Globals.Dialects.Add(dialect.UniqueId, dialect);
      return true;
   }

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      Globals.Dialects.Clear();
      var returnVal = base.UnloadSingleFileContent(fileObj, descriptor, lockObject);
      AfterLoadingStep(descriptor);
      return returnVal;
   }
}