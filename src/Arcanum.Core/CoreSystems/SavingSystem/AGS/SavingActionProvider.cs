using System.Diagnostics;
using Arcanum.Core.AgsRegistry;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Jomini.Date;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.InGame.Court.State;
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
   public static void RulerTermSaving(IAgs target,
                                      HashSet<PropertySavingMetadata> metadata,
                                      IndentedStringBuilder sb,
                                      bool asOneLine)
   {
      asOneLine = true;
      if (target is not RulerTerm rt)
         throw new InvalidOperationException("RulerTermSaving can only be used with RulerTerm instances.");

      var trueOneLiner = rt.CoRulers.Count == 0;

      if (trueOneLiner)
      {
         sb.Append("ruler_term = { ");
         RulerTerm.FormatRulerTerm(sb, asOneLine, rt);
      }
      else
      {
         sb.AppendLine("ruler_term = {");
         using (sb.Indent())
            RulerTerm.FormatRulerTerm(sb, asOneLine, rt);
      }

      if (trueOneLiner)
      {
         sb.Append(" }");
         sb.AppendLine();
         return;
      }

      foreach (var ct in rt.CoRulers)
      {
         sb.AppendLine();
         using (sb.Indent())
            RulerTerm.FormatRulerTerm(sb, asOneLine, ct);
      }

      sb.AppendLine();
      sb.Append("}");
      sb.AppendLine();
   }

   public static void LocationSaving(IAgs target,
                                     HashSet<PropertySavingMetadata> metadata,
                                     IndentedStringBuilder sb,
                                     bool asOneLine)
   {
      if (target is not Location location)
         throw new InvalidOperationException("LocationSaving can only be used with Location instances.");

      sb.Append(location.UniqueId).Append(" = ").Append(location.Color.AsHexString().ToLowerInvariant());
   }

   public static void SaveIdentifierStringKvp(IAgs target,
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

      AsOneLine(false, sb, $"{targetKvp.Key} = {targetKvp.Value}");
   }

   public static void SocientalValueEntrySaving(IAgs target,
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
         AsOneLine(asOneLine, sb, $"{sve.SocientalValue.UniqueId} = {FormatValue(SavingValueType.Int, sve, SocientalValueEntry.Field.Value)}");
      if (!asOneLine)
         sb.AppendLine();
   }

   public static void RoadSavingMethod(IAgs target, HashSet<PropertySavingMetadata> metadata, IndentedStringBuilder sb, bool asOneLine)
   {
      if (target is not Road road)
         throw new InvalidOperationException("RoadSavingMethod can only be used with AgsRoad instances.");

      AsOneLine(asOneLine, sb, $"{road.StartLocation.UniqueId} = {road.EndLocation.UniqueId}");
   }

   public static void JominiDate(IAgs target, HashSet<PropertySavingMetadata> metadata, IndentedStringBuilder sb, bool asOneLine)
   {
      if (target is not JominiDate date)
         throw new InvalidOperationException("JominiDate can only be used with JominiDate instances.");

      if (metadata.Count != 1)
         return;

      var md = metadata.First();
      AsOneLine(asOneLine, sb, $"{md.Keyword} {GetSeparator(md.Separator)} {date}");
   }

   public static void SaveNameDeclaration(IAgs target,
                                          HashSet<PropertySavingMetadata> metadata,
                                          IndentedStringBuilder sb,
                                          bool asOneLine)
   {
      if (target is not CharacterNameDeclaration cnd)
         throw new
            InvalidOperationException("SaveNameDeclaration can only be used with CharacterNameDeclaration instances.");

      var str = string.Empty;
      if (cnd.IsRandom)
         str = $"{cnd.SavingKey} = {cnd.Name}";
      else if (!string.IsNullOrEmpty(cnd.Name))
         str = $"{cnd.SavingKey} = {{ name = {cnd.Name} }}";
      AsOneLine(asOneLine, sb, str);
   }

   public static void SaveIAgsEnumKvp(IAgs target, HashSet<PropertySavingMetadata> metadata, IndentedStringBuilder sb, bool asOneLine)
   {
      if (target is not IIagsEnumKvp<IAgs, Enum> kvp)
      {
         Debug.Fail("Unexpected type in SaveIAgsEnumKvp");
         throw new InvalidOperationException("SaveIAgsEnumKvp can only be used with IIagsEnumKvp<IAgs> instances.");
      }

      AsOneLine(asOneLine, sb, $"{kvp.Key.SavingKey} = {EnumAgsRegistry.GetKey(kvp.Value)}");
   }

   public static void SaveDemandData(IAgs target, HashSet<PropertySavingMetadata> metadata, IndentedStringBuilder sb, bool asOneLine)
   {
      if (target is not DemandData dd)
         throw new InvalidOperationException("SaveDemandData can only be used with DemandData instances.");

      string str;
      if (dd.TargetAll > 0f)
         str = $"all = {FormatValue(SavingValueType.Float, dd, DemandData.Field.TargetAll)}";
      else if (dd.TargetUpper > 0f)
         str = $"upper = {FormatValue(SavingValueType.Float, dd, DemandData.Field.TargetUpper)}";
      else
         str =
            $"{FormatValue(SavingValueType.Identifier, dd, DemandData.Field.PopType)} = {FormatValue(SavingValueType.Float, dd, DemandData.Field.TargetUpper)}";

      AsOneLine(asOneLine, sb, str);
   }

   public static void SaveWealthImpactData(IAgs target,
                                           HashSet<PropertySavingMetadata> metadata,
                                           IndentedStringBuilder sb,
                                           bool asOneLine)
   {
      if (target is not DemandData dd)
         throw new InvalidOperationException("SaveWealthImpactData can only be used with WealthImpactData instances.");

      string str;
      if (dd.TargetAll > 0f)
         str = $"all = {FormatValue(SavingValueType.Float, dd, DemandData.Field.TargetAll)}";
      else
         str =
            $"{FormatValue(SavingValueType.Identifier, dd, DemandData.Field.PopType)} = {FormatValue(SavingValueType.Float, dd, DemandData.Field.TargetUpper)}";

      AsOneLine(asOneLine, sb, str);
   }

   public static void MapMovementAssistSaving(IAgs target,
                                              HashSet<PropertySavingMetadata> metadata,
                                              IndentedStringBuilder sb,
                                              bool asOneLine)
   {
      if (target is not MapMovementAssist mma)
         throw new
            InvalidOperationException("MapMovementAssistSaving can only be used with MapMovementAssist instances.");

      AsOneLine(asOneLine,
                sb,
                $"movement_assistance = {{ {FormatValue(SavingValueType.Float, mma._getValue(MapMovementAssist.Field.X), null)} {FormatValue(SavingValueType.Float, mma._getValue(MapMovementAssist.Field.Y), null)} }}");
   }

   public static void EstateCountDefinitionSaving(IAgs target,
                                                  HashSet<PropertySavingMetadata> metadata,
                                                  IndentedStringBuilder sb,
                                                  bool asOneLine)
   {
      if (target is not EstateCountDefinition ecd)
         throw new
            InvalidOperationException("EstateCountDefinitionSaving can only be used with EstateCountDefinition instances.");

      AsOneLine(asOneLine,
                sb,
                $"{FormatValue(SavingValueType.Identifier, ecd, EstateCountDefinition.Field.Estate)} = {FormatValue(SavingValueType.Int, ecd, EstateCountDefinition.Field.Count)}");
   }

   public static void ModValInstanceSaving(IAgs target,
                                           HashSet<PropertySavingMetadata> metadata,
                                           IndentedStringBuilder sb,
                                           bool asOneLine)
   {
      if (target is not ModValInstance mvi)
         throw new
            InvalidOperationException("ModValInstanceSaving can only be used with ModValInstance instances.");

      AsOneLine(asOneLine, sb, FormatValue(SavingValueType.Modifier, (object)mvi, null));
   }

   public static void SoundTollsSaving(IAgs target,
                                       HashSet<PropertySavingMetadata> metadata,
                                       IndentedStringBuilder sb,
                                       bool asOneLine)
   {
      if (target is not SoundToll st)
         throw new
            InvalidOperationException("SoundTollsSaving can only be used with SoundTolls instances.");

      AsOneLine(asOneLine,
                sb,
                $"{FormatValue(SavingValueType.Identifier, st, SoundToll.Field.StraitLocationOne)} = {FormatValue(SavingValueType.Identifier, st, SoundToll.Field.StraitLocationTwo)}");
   }

   public static void DefaultMapDefinitionSaving(IAgs target,
                                                 HashSet<PropertySavingMetadata> metadata,
                                                 IndentedStringBuilder sb,
                                                 bool asOneLine)
   {
      if (target is not DefaultMapDefinition dmd)
         throw new
            InvalidOperationException("DefaultMapDefinitionSaving can only be used with DefaultMapDefinition instances.");

      var settings = dmd.AgsSettings;
      var orderedProperties = metadata.ToList();
      for (var i = 0; i < orderedProperties.Count; i++)
      {
         var prop = orderedProperties[i];
         if (settings.Format == SavingFormat.Spacious && i > 0)
            sb.AppendLine();
         prop.Format(dmd, sb, asOneLine, "#", settings);
      }
   }

   public static void InstitutionPresenceSaving(IAgs target,
                                                HashSet<PropertySavingMetadata> metadata,
                                                IndentedStringBuilder sb,
                                                bool asOneLine)
   {
      if (target is not InstitutionPresence ip)
         throw new
            InvalidOperationException("InstitutionPresenceSaving can only be used with InstitutionPresence instances.");

      AsOneLine(asOneLine,
                sb,
                $"{FormatValue(SavingValueType.Identifier, ip, InstitutionPresence.Field.Institution)} = {FormatValue(SavingValueType.Bool, ip, InstitutionPresence.Field.IsPresent)}");
   }

   public static void Setup_vars_saving(IAgs target,
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
                  sb.AppendLine($"flag = \"{vard.Flag}\"");
                  sb.Append("data");
                  ((IAgs)vard.DataBlock).ToAgsContext().BuildContext(sb);
               }
            }
         }
      }
   }

   public static void DefinitionSaving(IAgs target,
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