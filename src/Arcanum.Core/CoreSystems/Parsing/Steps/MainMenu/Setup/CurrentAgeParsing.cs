using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;
using Age = Arcanum.Core.GameObjects.InGame.AbstractMechanics.Age;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

public class CurrentAgeParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : SetupFileLoadingService(dependencies)
{
   public override List<Type> ParsedObjects { get; } = [typeof(Age)];

   public override void ReloadSingleFile(Eu5FileObj fileObj, object? lockObject)
   {
   }

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, object? lockObject)
   {
      Globals.SetupContentNodes.CurrentAge = Age.Empty;
      return true;
   }

   public override void LoadSetupFile(StatementNode sn, ref ParsingContext pc, Eu5FileObj fileObj, object? lockObject)
   {
      if (!sn.IsContentNode(ref pc, out var cn))
         return;

      var ageKey = pc.SliceString(cn);
      if (!Globals.Ages.TryGetValue(ageKey, out var age))
      {
         pc.SetContext(cn);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.UnknownKey,
                                        ageKey,
                                        cn);
         return;
      }

      Globals.SetupContentNodes.CurrentAge = age;
   }
}