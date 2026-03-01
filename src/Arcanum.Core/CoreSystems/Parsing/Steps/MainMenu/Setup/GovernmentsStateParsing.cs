using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.GameObjects.InGame.Court.State;
using GovernmentState = Arcanum.Core.GameObjects.InGame.Court.State.GovernmentState;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

[ParserFor(typeof(GovernmentState))]
public static partial class GovernmentsStateParsing
{
   private static partial bool ArcParse_RulerTerms_PartList(BlockNode node,
                                                            GovernmentState target,
                                                            ref ParsingContext pc)
   {
      var mainTerm = Eu5Activator.CreateInstance<RulerTerm>("", pc.Context.FileObj, node);
      var currentContext = mainTerm;
      var firstCharacterFound = false;

      foreach (var sn in node.Children)
      {
         if (!sn.IsContentNode(ref pc, out var cn))
            continue;

         if (pc.IsSliceEqual(cn, "character"))
         {
            if (ParsingToolBox.ArcTryParse_Character(cn, ref pc, out var curChar))
            {
               if (!firstCharacterFound)
               {
                  currentContext.CharacterId = curChar;
                  firstCharacterFound = true;
               }
               else
               {
                  var coRuler = Eu5Activator.CreateInstance<RulerTerm>("", pc.Context.FileObj, cn);
                  coRuler.CharacterId = curChar;

                  mainTerm.CoRulers.Add(coRuler);
                  currentContext = coRuler;
               }
            }

            continue;
         }

         var key = pc.SliceString(cn);
         switch (key)
         {
            case "start_date":
               if (ParsingToolBox.ArcTryParse_JominiDate(cn, ref pc, out var sDate))
                  currentContext.StartDate = sDate;
               break;

            case "end_date":
               if (ParsingToolBox.ArcTryParse_JominiDate(cn, ref pc, out var eDate))
                  currentContext.EndDate = eDate;
               break;

            case "regnal_number":
               if (cn.Value.IsLiteralValueNode(ref pc, out var rNode) &&
                   NumberParsing.TryParseInt(pc.SliceString(rNode.Value), ref pc, out var regNum))
                  currentContext.RegnalNumber = regNum;
               break;
         }
      }

      target.RulerTerms.Add(mainTerm);
      return true;
   }
}