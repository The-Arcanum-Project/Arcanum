using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common.KeyWordClasses;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Culture;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

public class CultureParsing : FileLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(Culture)];
   public override string GetFileDataDebugInfo() => $"Loaded '{Globals.Cultures.Count}' cultures.";
   public static readonly Dictionary<FileObj, List<ContentNodeGroup>> Opinions2NdParse = [];

   private const string STEP_NAME = nameof(CultureParsing);

   private delegate void ContentParser(ContentNode cn,
                                       Culture culture,
                                       LocationContext ctx,
                                       string source,
                                       ref bool validation);

   private delegate void BlockParser(BlockNode bn,
                                     Culture c,
                                     LocationContext ctx,
                                     string src,
                                     ref bool v,
                                     FileObj f);

   private static readonly Dictionary<string, ContentParser> ContentParsers = new()
   {
      { CultureKeywords.LANGUAGE, ParseLanguage },
      { CultureKeywords.COLOR, ParseColor },
      { CultureKeywords.DYNASTY_NAME_TYPE, SetDynastyNameTypeKey },
      { CultureKeywords.USE_PATRONYM, ParsePatronym },
   };

   private static readonly Dictionary<string, BlockParser> BlockParsers = new()
   {
      { CultureKeywords.TAGS, ParseGfxTags },
      { CultureKeywords.OPINIONS, ParseOpinions },
      { CultureKeywords.CULTURE_GROUPS, ParseCultureGroups },
      { CultureKeywords.NOUN_KEYS, ParseNounKeys },
      { CultureKeywords.ADJECTIVE_KEYS, ParseAdjectiveKeys },
      { CultureKeywords.COUNTRY_MODIFIER, UnsupportedModifier },
      { CultureKeywords.LOCATION_MODIFIER, UnsupportedModifier },
      { CultureKeywords.CHARACTER_MODIFIER, UnsupportedModifier },
   };

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      const string actionName = $"{nameof(CultureParsing)}.ParseSingleCultureFile";
      var validation = true;
      var rns = Parser.Parse(fileObj, out var source, out var ctx);

      foreach (var sn in rns.Statements)
      {
         if (!sn.IsBlockNode(ctx, nameof(CultureParsing), source, ref validation, out var cbn))
            continue;

         var culture = new Culture(cbn.KeyNode.GetLexeme(source));

         foreach (var innerSn in cbn.Children)
         {
            if (innerSn is ContentNode cn)
            {
               ParseCultureContentNode(cn, culture, ctx, source, actionName, ref validation);
            }
            else if (innerSn is BlockNode ibn)
            {
               ParseCultureBlockNode(ibn, culture, ctx, source, actionName, ref validation, fileObj);
            }
            else
            {
               ctx.SetPosition(innerSn.KeyNode);
               DiagnosticException.LogWarning(ctx.GetInstance(),
                                              ParsingError.Instance.InvalidNodeType,
                                              actionName,
                                              innerSn.GetType().Name,
                                              $"{nameof(ContentNode)}' or '{nameof(BlockNode)}",
                                              innerSn.KeyNode.GetLexeme(source));
               validation = false;
            }
         }

         if (!Globals.Cultures.TryAdd(culture.Name, culture))
         {
            ctx.SetPosition(cbn.KeyNode);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.DuplicateObjectDefinition,
                                           actionName,
                                           culture.Name,
                                           nameof(Globals.Cultures),
                                           Culture.Field.Name);
            validation = false;
         }
      }

      return validation;
   }

   private static void ParseCultureBlockNode(BlockNode bn,
                                             Culture culture,
                                             LocationContext ctx,
                                             string source,
                                             string actionName,
                                             ref bool validation,
                                             FileObj fileObj)
   {
      var key = bn.KeyNode.GetLexeme(source);
      if (BlockParsers.TryGetValue(key, out var parser))
         parser(bn, culture, ctx, source, ref validation, fileObj);
      else
         De.LogInvalidBlockName(ctx, bn.KeyNode, source, actionName, BlockParsers.Keys.ToArray(), ref validation);
   }

   private static void ParseCultureContentNode(ContentNode cn,
                                               Culture culture,
                                               LocationContext ctx,
                                               string source,
                                               string actionName,
                                               ref bool validation)
   {
      var key = cn.KeyNode.GetLexeme(source);
      if (ContentParsers.TryGetValue(key, out var parser))
         parser(cn, culture, ctx, source, ref validation);
      else
         De.LogInvalidContentKeyOrNode(ctx,
                                       cn.KeyNode,
                                       source,
                                       actionName,
                                       string.Join(", ", ContentParsers.Keys),
                                       ref validation);
   }

   #region Content Parsers

   private static void ParsePatronym(ContentNode cn,
                                     Culture culture,
                                     LocationContext ctx,
                                     string source,
                                     ref bool validation)
   {
      cn.SetBoolIfValid(ctx,
                        $"{STEP_NAME}.{nameof(ParsePatronym)}",
                        source,
                        culture,
                        Culture.Field.UsePatronym,
                        ref validation);
   }

   private static void SetDynastyNameTypeKey(ContentNode cn,
                                             Culture culture,
                                             LocationContext ctx,
                                             string source,
                                             ref bool validation)
   {
      cn.SetIdentifierIfValid(ctx,
                              $"{STEP_NAME}.{nameof(SetDynastyNameTypeKey)}",
                              source,
                              ref validation,
                              culture,
                              Culture.Field.DynastyNameType);
   }

   private static void ParseColor(ContentNode cn,
                                  Culture culture,
                                  LocationContext ctx,
                                  string source,
                                  ref bool validation)
   {
      cn.SetColorIfValid(ctx,
                         $"{STEP_NAME}.{nameof(ParseColor)}",
                         source,
                         ref validation,
                         culture,
                         Culture.Field.Color);
   }

   private static void ParseLanguage(ContentNode cn,
                                     Culture culture,
                                     LocationContext ctx,
                                     string source,
                                     ref bool validation)
   {
      cn.SetIdentifierIfValid(ctx,
                              $"{STEP_NAME}.{nameof(ParseLanguage)}",
                              source,
                              ref validation,
                              culture,
                              Culture.Field.Language);
   }

   #endregion

   #region Block Parsers

   private static void UnsupportedModifier(BlockNode bn,
                                           Culture c,
                                           LocationContext ctx,
                                           string src,
                                           ref bool v,
                                           FileObj f)
   {
   }

   private static void ParseAdjectiveKeys(BlockNode bn,
                                          Culture c,
                                          LocationContext ctx,
                                          string src,
                                          ref bool v,
                                          FileObj f)
   {
      bn.SetIdentifierList(ctx,
                           $"{STEP_NAME}.{nameof(ParseAdjectiveKeys)}",
                           src,
                           ref v,
                           c,
                           Culture.Field.AdjectiveKeys);
   }

   private static void ParseNounKeys(BlockNode bn, Culture c, LocationContext ctx, string src, ref bool v, FileObj f)
   {
      bn.SetIdentifierList(ctx, $"{STEP_NAME}.{nameof(ParseNounKeys)}", src, ref v, c, Culture.Field.NounKeys);
   }

   private static void ParseCultureGroups(BlockNode bn,
                                          Culture c,
                                          LocationContext ctx,
                                          string src,
                                          ref bool v,
                                          FileObj f)
   {
      bn.SetIdentifierList(ctx,
                           $"{STEP_NAME}.{nameof(ParseCultureGroups)}",
                           src,
                           ref v,
                           c,
                           Culture.Field.CultureGroups);
   }

   private static void ParseOpinions(BlockNode bn, Culture c, LocationContext ctx, string src, ref bool v, FileObj f)
   {
      List<ContentNode> opinionNodes = [];
      foreach (var sn in bn.Children)
      {
         if (!sn.IsContentNode(ctx, src, $"{STEP_NAME}.{nameof(ParseOpinions)}", ref v, out var cn))
            continue;

         opinionNodes.Add(cn);
      }

      Opinions2NdParse.TryAdd(f, []);
      Opinions2NdParse[f].Add(new(opinionNodes, c));
   }

   private static void ParseGfxTags(BlockNode bn, Culture c, LocationContext ctx, string src, ref bool v, FileObj f)
   {
      bn.SetIdentifierList(ctx, $"{STEP_NAME}.{nameof(ParseGfxTags)}", src, ref v, c, Culture.Field.GfxTags);
   }

   #endregion

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
      => throw new NotImplementedException();

   public struct ContentNodeGroup(List<ContentNode> nodes, Culture culture)
   {
      public readonly List<ContentNode> Nodes = nodes;
      public readonly Culture Culture = culture;
   }
}