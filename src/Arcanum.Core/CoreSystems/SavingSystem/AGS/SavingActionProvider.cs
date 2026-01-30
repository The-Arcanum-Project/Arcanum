using System.Diagnostics;
using Arcanum.Core.AgsRegistry;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Jomini.Date;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.Serialization;
using Arcanum.Core.CoreSystems.SavingSystem.Serialization.Nodes;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.InGame.Court.State;
using Arcanum.Core.GameObjects.InGame.Cultural.SubObjects;
using Arcanum.Core.GameObjects.InGame.Religious.SubObjects;
using static Arcanum.Core.CoreSystems.SavingSystem.AGS.SavingUtil;
using CharacterNameDeclaration = Arcanum.Core.GameObjects.InGame.Court.CharacterNameDeclaration;
using Continent = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Continent;
using Country = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Country;
using DefaultMapDefinition = Arcanum.Core.GameObjects.InGame.Map.DefaultMapDefinition;
using DemandData = Arcanum.Core.GameObjects.InGame.Economy.SubClasses.DemandData;
using EstateCountDefinition = Arcanum.Core.GameObjects.InGame.AbstractMechanics.EstateCountDefinition;
using InstitutionPresence = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.SubObjects.InstitutionPresence;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;
using MapMovementAssist = Arcanum.Core.GameObjects.InGame.Map.SubObjects.MapMovementAssist;
using Road = Arcanum.Core.GameObjects.InGame.Map.Road;
using SocientalValueEntry = Arcanum.Core.GameObjects.InGame.Court.State.SubClasses.SocientalValueEntry;
using SoundToll = Arcanum.Core.GameObjects.InGame.Map.SoundToll;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

/// <summary>
/// The class provides saving methods for AGS properties. <br/>
/// Custom saving methods can be defined here and referenced in <see cref="PropertySavingMetadata.SavingMethod"/>.
/// </summary>
public static class SavingActionProvider
{
   public static void RulerTermSaving(IEu5Object target,
                                      HashSet<PropertySavingMetadata> metadata,
                                      IndentedStringBuilder sb,
                                      // ReSharper disable once RedundantAssignment
                                      bool asOneLine)
   {
      asOneLine = true;
      if (target is not RulerTerm rt)
         throw new InvalidOperationException("RulerTermSaving can only be used with RulerTerm instances.");

      var trueOneLiner = rt.CoRulers.Count == 0;

      sb.Append("ruler_term").AppendOpeningBrace(asOneLine: trueOneLiner).AppendSpacer();
      if (trueOneLiner)
         RulerTerm.FormatRulerTerm(sb, asOneLine, rt);
      else
         using (sb.Indent())
            RulerTerm.FormatRulerTerm(sb, asOneLine, rt);

      if (trueOneLiner)
      {
         sb.AppendClosingBrace(asOneLine: true).AppendLine();
         return;
      }

      foreach (var ct in rt.CoRulers)
      {
         sb.AppendLine();
         using (sb.Indent())
            RulerTerm.FormatRulerTerm(sb, asOneLine, ct);
      }

      sb.AppendClosingBrace();
   }

   public static void LocationSaving(IEu5Object target,
                                     HashSet<PropertySavingMetadata> metadata,
                                     IndentedStringBuilder sb,
                                     bool asOneLine)
   {
      if (target is not Location location)
         throw new InvalidOperationException("LocationSaving can only be used with Location instances.");

      sb.Append(location.UniqueId)
        .AppendSeparator()
        .Append(location.Color.AsHexString().ToLowerInvariant());
   }

   public static void SaveIdentifierStringKvp(IEu5Object target,
                                              HashSet<PropertySavingMetadata> metadata,
                                              IndentedStringBuilder sb,
                                              bool asOneLine)
   {
      if (target is not IStringKvp targetKvp)
      {
         Debug.Fail("Unexpected type in SaveIdentifierStringKvp");
         throw new
            InvalidOperationException("SaveIdentifierStringKvp can only be used with IIdentifierStringKvp instances.");
      }

      sb.Append(targetKvp.Key)
        .AppendSeparator()
        .Append(targetKvp.Value);
   }

   public static void SocientalValueEntrySaving(IEu5Object target,
                                                PropertySavingMetadata metadata,
                                                IndentedStringBuilder sb,
                                                bool asOneLine)
   {
      var sves = (ObservableRangeCollection<SocientalValueEntry>)target._getValue(metadata.NxProp);

      if (sves.Count == 0)
         return;

      if (!asOneLine)
         sb.AppendLine();
      foreach (var sve in sves)
         sb.AppendNewLineIfNone()
           .Append(sve.SocientalValue.UniqueId)
           .AppendSeparator()
           .Append(FormatValue(SavingValueType.Int, sve, SocientalValueEntry.Field.Value));

      sb.AppendBlockNewLines();
   }

   public static void RoadSavingMethod(IEu5Object target, HashSet<PropertySavingMetadata> metadata, IndentedStringBuilder sb, bool asOneLine)
   {
      if (target is not Road road)
         throw new InvalidOperationException("RoadSavingMethod can only be used with AgsRoad instances.");

      sb.Append(road.StartLocation.UniqueId)
        .AppendSeparator()
        .Append(road.EndLocation.UniqueId);
   }

   public static void JominiDate(IEu5Object target, HashSet<PropertySavingMetadata> metadata, IndentedStringBuilder sb, bool asOneLine)
   {
      if (target is not JominiDate date)
         throw new InvalidOperationException("JominiDate can only be used with JominiDate instances.");

      if (metadata.Count != 1)
         return;

      var md = metadata.First();
      sb.Append(md.Keyword).AppendSpacer().Append(GetSeparator(md.Separator)).AppendSpacer().Append(date);
   }

   public static void SaveNameDeclaration(IEu5Object target,
                                          HashSet<PropertySavingMetadata> metadata,
                                          IndentedStringBuilder sb,
                                          bool asOneLine)
   {
      if (target is not CharacterNameDeclaration cnd)
         throw new
            InvalidOperationException("SaveNameDeclaration can only be used with CharacterNameDeclaration instances.");

      if (cnd.IsRandom)
         sb.Append(cnd.SavingKey)
           .AppendSeparator()
           .Append(cnd.Name);
      else if (!string.IsNullOrEmpty(cnd.Name))
         sb.Append(cnd.SavingKey)
           .AppendOpeningBrace()
           .AppendSpacer()
           .Append("name")
           .AppendSeparator()
           .Append(cnd.Name)
           .AppendSpacer()
           .Append('}');
   }

   public static void SaveReligiousSchoolOpinionValue(IEu5Object target, HashSet<PropertySavingMetadata> metadata, IndentedStringBuilder sb, bool asOneLine)
   {
      if (target is not ReligiousSchoolOpinionValue kvp)
      {
         Debug.Fail("Unexpected type in SaveCultureOpinionValue");
         throw new InvalidOperationException("SaveCultureOpinionValue can only be used with ReligiousSchoolOpinionValue instances.");
      }

      sb.Append(kvp.Key.UniqueId)
        .AppendSeparator()
        .Append(EnumAgsRegistry.GetKey(kvp.Value));
   }

   public static void SaveCultureOpinionValue(IEu5Object target, HashSet<PropertySavingMetadata> metadata, IndentedStringBuilder sb, bool asOneLine)
   {
      if (target is not CultureOpinionValue kvp)
      {
         Debug.Fail("Unexpected type in SaveCultureOpinionValue");
         throw new InvalidOperationException("SaveCultureOpinionValue can only be used with CultureOpinionValue instances.");
      }

      sb.Append(kvp.Key.UniqueId)
        .AppendSeparator()
        .Append(EnumAgsRegistry.GetKey(kvp.Value));
   }

   public static void SaveReligionOpinionValue(IEu5Object target, HashSet<PropertySavingMetadata> metadata, IndentedStringBuilder sb, bool asOneLine)
   {
      if (target is not ReligionOpinionValue kvp)
      {
         Debug.Fail("Unexpected type in SaveReligionOpinionValue");
         throw new InvalidOperationException("SaveReligionOpinionValue can only be used with ReligionOpinionValue instances.");
      }

      sb.Append(kvp.Key.UniqueId)
        .AppendSeparator()
        .Append(EnumAgsRegistry.GetKey(kvp.Value));
   }

   public static void SaveDemandData(IEu5Object target, HashSet<PropertySavingMetadata> metadata, IndentedStringBuilder sb, bool asOneLine)
   {
      if (target is not DemandData dd)
         throw new InvalidOperationException("SaveDemandData can only be used with DemandData instances.");

      if (dd.TargetAll > 0f)
         sb.Append("all").AppendSeparator().Append(FormatValue(SavingValueType.Float, dd, DemandData.Field.TargetAll));
      else if (dd.TargetUpper > 0f)
         sb.Append("upper").AppendSeparator().Append(FormatValue(SavingValueType.Float, dd, DemandData.Field.TargetUpper));
      else
         sb.Append(FormatValue(SavingValueType.Identifier, dd, DemandData.Field.PopType))
           .AppendSeparator()
           .Append(FormatValue(SavingValueType.Float, dd, DemandData.Field.TargetUpper));
   }

   public static void SaveWealthImpactData(IEu5Object target,
                                           HashSet<PropertySavingMetadata> metadata,
                                           IndentedStringBuilder sb,
                                           bool asOneLine)
   {
      if (target is not DemandData dd)
         throw new InvalidOperationException("SaveWealthImpactData can only be used with WealthImpactData instances.");

      if (dd.TargetAll > 0f)
         sb.Append("all").AppendSeparator().Append(FormatValue(SavingValueType.Float, dd, DemandData.Field.TargetAll));
      else
         sb.Append(FormatValue(SavingValueType.Identifier, dd, DemandData.Field.PopType))
           .AppendSeparator()
           .Append(FormatValue(SavingValueType.Float, dd, DemandData.Field.TargetUpper));
   }

   public static void MapMovementAssistSaving(IEu5Object target,
                                              HashSet<PropertySavingMetadata> metadata,
                                              IndentedStringBuilder sb,
                                              bool asOneLine)
   {
      if (target is not MapMovementAssist mma)
         throw new
            InvalidOperationException("MapMovementAssistSaving can only be used with MapMovementAssist instances.");

      sb.Append("movement_assistance")
        .AppendOpeningBrace()
        .Append(FormatValue(SavingValueType.Float, mma._getValue(MapMovementAssist.Field.X), null))
        .AppendSpacer()
        .Append(FormatValue(SavingValueType.Float, mma._getValue(MapMovementAssist.Field.Y), null))
        .AppendClosingBrace();
   }

   public static void EstateCountDefinitionSaving(IEu5Object target,
                                                  HashSet<PropertySavingMetadata> metadata,
                                                  IndentedStringBuilder sb,
                                                  bool asOneLine)
   {
      if (target is not EstateCountDefinition ecd)
         throw new
            InvalidOperationException("EstateCountDefinitionSaving can only be used with EstateCountDefinition instances.");

      sb.Append(FormatValue(SavingValueType.Identifier, ecd, EstateCountDefinition.Field.Estate))
        .AppendSeparator()
        .Append(FormatValue(SavingValueType.Int, ecd, EstateCountDefinition.Field.Count));
   }

   public static void ModValInstanceSaving(IEu5Object target,
                                           HashSet<PropertySavingMetadata> metadata,
                                           IndentedStringBuilder sb,
                                           bool asOneLine)
   {
      if (target is not ModValInstance mvi)
         throw new
            InvalidOperationException("ModValInstanceSaving can only be used with ModValInstance instances.");

      sb.Append(FormatValue(SavingValueType.Modifier, (object)mvi, null));
   }

   public static void SoundTollsSaving(IEu5Object target,
                                       HashSet<PropertySavingMetadata> metadata,
                                       IndentedStringBuilder sb,
                                       bool asOneLine)
   {
      if (target is not SoundToll st)
         throw new
            InvalidOperationException("SoundTollsSaving can only be used with SoundTolls instances.");

      sb.Append(FormatValue(SavingValueType.Identifier, st, SoundToll.Field.StraitLocationOne))
        .AppendSeparator()
        .Append(FormatValue(SavingValueType.Identifier, st, SoundToll.Field.StraitLocationTwo));
   }

   public static void DefaultMapDefinitionSaving(IEu5Object target,
                                                 HashSet<PropertySavingMetadata> metadata,
                                                 IndentedStringBuilder sb,
                                                 bool asOneLine)
   {
      if (target is not DefaultMapDefinition dmd)
         throw new
            InvalidOperationException("DefaultMapDefinitionSaving can only be used with DefaultMapDefinition instances.");

      var dmdNode = TreeBuilder.Construct(dmd, false, null, true);
      if (dmdNode is not BlockSerializationNode bsn)
         throw new InvalidOperationException($"DefaultMapDefinitionSaving expected a BlockSerializationNode but found a {dmdNode.GetType()}.");

      var commentChar = "#";
      bsn.SerializeChildElements(sb, ref commentChar, asOneLine);
   }

   public static void InstitutionPresenceSaving(IEu5Object target,
                                                HashSet<PropertySavingMetadata> metadata,
                                                IndentedStringBuilder sb,
                                                bool asOneLine)
   {
      if (target is not InstitutionPresence ip)
         throw new
            InvalidOperationException("InstitutionPresenceSaving can only be used with InstitutionPresence instances.");

      sb.Append(FormatValue(SavingValueType.Identifier, ip, InstitutionPresence.Field.Institution))
        .AppendSeparator()
        .Append(FormatValue(SavingValueType.Bool, ip, InstitutionPresence.Field.IsPresent));
   }

   public static void Setup_vars_saving(IEu5Object target,
                                        PropertySavingMetadata metadata,
                                        IndentedStringBuilder sb,
                                        bool asOneLine)
   {
      if (target is not Country country)
         throw new
            InvalidOperationException("Setup_vars_saving can only be used with Country instances.");

      if (country.Variables.Count == 0)
         return;

      using (sb.BlockWithName("variables"))
      {
         using (sb.BlockWithName("data"))
         {
            foreach (var vard in country.Variables)
            {
               using (sb.Block())
               {
                  sb.Append("flag")
                    .AppendSeparator()
                    .Append('"')
                    .Append(vard.Flag)
                    .Append('"')
                    .AppendLine()
                    .Append("data");
                  ((IEu5Object)vard.DataBlock).ToAgsContext().BuildContext(sb);
               }
            }
         }
      }
   }

   public static void DefinitionSaving(IEu5Object target,
                                       HashSet<PropertySavingMetadata> metadata,
                                       IndentedStringBuilder sb,
                                       bool asOneLine)
   {
      if (target is not Continent c)
         throw new
            InvalidOperationException("DefinitionSaving can only be used with LocationCollectionDefinition instances.");

      using (sb.BlockWithName(c.UniqueId, addNewLineBeforeClosing: true))
         foreach (var sr in c.SuperRegions)
            using (sb.BlockWithName(sr.UniqueId, addNewLineBeforeClosing: true))
               foreach (var r in sr.Regions)
                  using (sb.BlockWithName(r.UniqueId, addNewLineBeforeClosing: true))
                     foreach (var a in r.Areas)
                        using (sb.BlockWithName(a.UniqueId, addNewLineBeforeClosing: true))
                           foreach (var p in a.Provinces)
                              using (sb.BlockWithName(p.UniqueId, addNewLineBeforeClosing: true))
                                 sb.AppendList(p.Locations.Select(x => x.UniqueId).ToList(), " ");
   }
}