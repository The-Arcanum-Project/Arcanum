using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.CountryLevel;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps;

public class CountryRankLoading : FileLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(CountryRank)];
   public override bool IsFullyParsed => false;

   public override string GetFileDataDebugInfo() => $"Country Ranks Loaded: {Globals.CountryRanks.Count}";

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      var rns = Parser.Parse(fileObj, out var source);
      var ctx = new LocationContext(0, 0, fileObj.Path.FullPath);

      foreach (var rn in rns.Statements)
      {
         if (!rn.IsBlockNode(ctx, source, GetActionName(), out var bNode))
            continue;

         var crlName = bNode!.KeyNode.GetLexeme(source);
         var crl = new CountryRank(crlName);

         foreach (var cn in bNode.Children)
         {
            if (cn is ContentNode { Value: LiteralValueNode lvn } cnn)
            {
               var lvnKey = cn.KeyNode.GetLexeme(source);
               if (lvnKey.Equals("color"))
               {
                  var validation = true;
                  cnn.SetColorIfValid(ctx, GetActionName(), source, ref validation, crl, CountryRank.Field.Color);
               }
               else if (lvnKey.Equals("level"))
               {
                  if (int.TryParse(lvn.Value.GetLexeme(source), out var level))
                     crl.Level = level;
                  else
                     DiagnosticException.LogWarning(ctx.GetInstance(),
                                                    ParsingError.Instance.InvalidIntegerValue,
                                                    "CountryRankLoading",
                                                    lvn.Value.GetLexeme(source));
               }
            }
         }

         Globals.CountryRanks.Add(crl);
      }

      return true;
   }

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
   {
      Globals.CountryRanks.Clear();
      return true;
   }
}