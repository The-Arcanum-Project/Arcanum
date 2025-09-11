using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GlobalStates;
using LocationRank = Arcanum.Core.GameObjects.LocationCollections.LocationRank;

namespace Arcanum.Core.CoreSystems.Parsing.Steps;

[ParserFor(typeof(LocationRank))]
public partial class LocationRankParsing : FileLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(LocationRank)];

   public override string GetFileDataDebugInfo()
   {
      var outStr = $"Location Ranks: ({Globals.LocationRanks.Count})\n";
      return Globals.LocationRanks.Aggregate(outStr,
                                             (current, rank)
                                                => current + $"- {rank.Name} ({rank.ColorKey}, {rank.IsMaxRank})\n");
   }

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      const string actionStack = nameof(LocationRankParsing);
      var rn = Parser.Parse(fileObj, out var source, out var ctx);
      var validation = true;
      var order = 0;

      foreach (var sn in rn.Statements)
      {
         if (!sn.IsBlockNode(ctx, source, actionStack, ref validation, out var bn))
            continue;

         var rankKey = bn.KeyNode.GetLexeme(source);
         var rank = new LocationRank(rankKey, order);
         var unresolvedNodes = ParseProperties(bn, rank, ctx, source, ref validation);

         foreach (var node in unresolvedNodes)
            // We only care if we have unparsed content nodes
            node.IsBlockNode(ctx, source, actionStack, ref validation, out _);

         if (Globals.LocationRanks.Contains(rank))
         {
            ctx.SetPosition(bn.KeyNode);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.DuplicateObjectDefinition,
                                           actionStack,
                                           rankKey,
                                           typeof(LocationRank),
                                           LocationRank.Field.Name);
         }
         else
            Globals.LocationRanks.Add(rank);

         order++;
      }

      return validation;
   }

   /// <summary>
   /// There is only one file allowed so we clear the list on unload
   /// </summary>
   /// <param name="fileObj"></param>
   /// <param name="descriptor"></param>
   /// <returns></returns>
   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
   {
      Globals.LocationRanks.Clear();
      return true;
   }
}