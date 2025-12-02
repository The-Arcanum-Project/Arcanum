using System.Text;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.Setup;

public class SetupParsingStep(IEnumerable<IDependencyNode<string>> dependencies) : FileLoadingService(dependencies)
{
   public override List<Type> ParsedObjects => [];

   public override string GetFileDataDebugInfo()
   {
      var sb = new StringBuilder();
      sb.AppendLine("Parsed the following files:");
      foreach (var file in SetupParsingManager.LoadedFiles)
         sb.AppendLine($"- {file.Path.LocalPath} ({file.ObjectsInFile.Count} objects)");
      return sb.ToString();
   }

   public override void ReloadSingleFile(Eu5FileObj fileObj,
                                         object? lockObject,
                                         string actionStack,
                                         ref bool validation)
   {
      // TODO: we have to reload all.
   }

   public override bool LoadSingleFile(Eu5FileObj fileObj, object? lockObject)
   {
      return SetupParsingManager.LoadFile(fileObj, lockObject);
   }

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      // Todo: unload all.
      return true;
   }
}