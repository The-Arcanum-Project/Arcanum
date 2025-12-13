using System.Diagnostics;
using Arcanum.Core.AgsRegistry;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Jomini.Date;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.GameObjects.AbstractMechanics;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.Court;
using Arcanum.Core.GameObjects.Economy.SubClasses;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.LocationCollections.SubObjects;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.GameObjects.Map.SubObjects;
using static Arcanum.Core.CoreSystems.SavingSystem.AGS.SavingUtil;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

/// <summary>
/// The class provides saving methods for AGS properties. <br/>
/// Custom saving methods can be defined here and referenced in <see cref="PropertySavingMetadata.SavingMethod"/>.
/// </summary>
public static class SavingActionProvider
{
   public static void SaveIdentifierStringKvp(IAgs target,
                                              HashSet<PropertySavingMetadata> metadata,
                                              IndentedStringBuilder sb)
   {
      if (target is not IStringKvp targetKvp)
      {
         Debug.Fail("Unexpected type in SaveIdentifierStringKvp");
         throw new
            InvalidOperationException("SaveIdentifierStringKvp can only be used with IIdentifierStringKvp instances.");
      }

      sb.AppendLine($"{targetKvp.Key} = {targetKvp.Value}");
   }

   public static void RoadSavingMethod(IAgs target, HashSet<PropertySavingMetadata> metadata, IndentedStringBuilder sb)
   {
      if (target is not Road road)
         throw new InvalidOperationException("RoadSavingMethod can only be used with AgsRoad instances.");

      sb.AppendLine($"{road.StartLocation.UniqueId} = {road.EndLocation.UniqueId}");
   }

   public static void JominiDate(IAgs target, HashSet<PropertySavingMetadata> metadata, IndentedStringBuilder sb)
   {
      if (target is not JominiDate date)
         throw new InvalidOperationException("JominiDate can only be used with JominiDate instances.");

      if (metadata.Count != 1)
         return;

      var md = metadata.First();
      sb.AppendLine($"{md.Keyword} {GetSeparator(md.Separator)} {date}");
   }

   public static void SaveNameDeclaration(IAgs target,
                                          HashSet<PropertySavingMetadata> metadata,
                                          IndentedStringBuilder sb)
   {
      if (target is not CharacterNameDeclaration cnd)
         throw new
            InvalidOperationException("SaveNameDeclaration can only be used with CharacterNameDeclaration instances.");

      if (cnd.IsRandom)
         sb.AppendLine($"{cnd.SavingKey} = {cnd.Name}");
      else if (!string.IsNullOrEmpty(cnd.Name))
         sb.AppendLine($"{cnd.SavingKey} = {{ name = {cnd.Name} }}");
   }

   public static void SaveIAgsEnumKvp(IAgs target, HashSet<PropertySavingMetadata> metadata, IndentedStringBuilder sb)
   {
      if (target is not IIagsEnumKvp<IAgs, Enum> kvp)
      {
         Debug.Fail("Unexpected type in SaveIAgsEnumKvp");
         throw new InvalidOperationException("SaveIAgsEnumKvp can only be used with IIagsEnumKvp<IAgs> instances.");
      }

      sb.AppendLine($"{kvp.Key.SavingKey} = {EnumAgsRegistry.GetKey(kvp.Value)}");
   }

   public static void SaveDemandData(IAgs target, HashSet<PropertySavingMetadata> metadata, IndentedStringBuilder sb)
   {
      if (target is not DemandData dd)
         throw new InvalidOperationException("SaveDemandData can only be used with DemandData instances.");

      if (dd.TargetAll > 0f)
         sb.AppendLine($"all = {FormatValue(SavingValueType.Float, dd, DemandData.Field.TargetAll)}");
      else if (dd.TargetUpper > 0f)
         sb.AppendLine($"upper = {FormatValue(SavingValueType.Float, dd, DemandData.Field.TargetUpper)}");
      else
         sb.AppendLine($"{FormatValue(SavingValueType.Identifier, dd, DemandData.Field.PopType)} = {FormatValue(SavingValueType.Float, dd, DemandData.Field.TargetUpper)}");
   }

   public static void MapMovementAssistSaving(IAgs target,
                                              HashSet<PropertySavingMetadata> metadata,
                                              IndentedStringBuilder sb)
   {
      if (target is not MapMovementAssist mma)
         throw new
            InvalidOperationException("MapMovementAssistSaving can only be used with MapMovementAssist instances.");

      sb.AppendLine($"movement_assistance = {{ {FormatValue(SavingValueType.Float, mma, MapMovementAssist.Field.X)} {FormatValue(SavingValueType.Float, mma, MapMovementAssist.Field.Y)} }}");
   }

   public static void EstateCountDefinitionSaving(IAgs target,
                                                  HashSet<PropertySavingMetadata> metadata,
                                                  IndentedStringBuilder sb)
   {
      if (target is not EstateCountDefinition ecd)
         throw new
            InvalidOperationException("EstateCountDefinitionSaving can only be used with EstateCountDefinition instances.");

      sb.AppendLine($"{FormatValue(SavingValueType.Identifier, ecd, EstateCountDefinition.Field.Estate)} = {FormatValue(SavingValueType.Int, ecd, EstateCountDefinition.Field.Count)}");
   }

   public static void ModValInstanceSaving(IAgs target,
                                           HashSet<PropertySavingMetadata> metadata,
                                           IndentedStringBuilder sb)
   {
      if (target is not ModValInstance mvi)
         throw new
            InvalidOperationException("ModValInstanceSaving can only be used with ModValInstance instances.");

      sb.AppendLine(FormatValue(SavingValueType.Modifier, (object)mvi, null));
   }

   public static void SoundTollsSaving(IAgs target,
                                       HashSet<PropertySavingMetadata> metadata,
                                       IndentedStringBuilder sb)
   {
      if (target is not SoundToll st)
         throw new
            InvalidOperationException("SoundTollsSaving can only be used with SoundTolls instances.");

      sb.AppendLine($"{FormatValue(SavingValueType.Identifier, st, SoundToll.Field.StraitLocationOne)} = {FormatValue(SavingValueType.Identifier, st, SoundToll.Field.StraitLocationTwo)}");
   }

   public static void DefaultMapDefinitionSaving(IAgs target,
                                                 HashSet<PropertySavingMetadata> metadata,
                                                 IndentedStringBuilder sb)
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
         prop.Format(dmd, sb, "#", settings);
      }
   }

   public static void LocationSaving(IAgs target,
                                     HashSet<PropertySavingMetadata> metadata,
                                     IndentedStringBuilder sb)
   {
      if (target is not Location loc)
         throw new
            InvalidOperationException("LocationSaving can only be used with Location instances.");

      sb.AppendLine($"{loc.UniqueId} = {loc.Color.AsHexString().ToLower()}");
   }

   public static void InstitutionPresenceSaving(IAgs target,
                                                HashSet<PropertySavingMetadata> metadata,
                                                IndentedStringBuilder sb)
   {
      if (target is not InstitutionPresence ip)
         throw new
            InvalidOperationException("InstitutionPresenceSaving can only be used with InstitutionPresence instances.");

      sb.AppendLine($"{FormatValue(SavingValueType.Identifier, ip, InstitutionPresence.Field.Institution)} = {FormatValue(SavingValueType.Bool, ip, InstitutionPresence.Field.IsPresent)}");
   }

   public static void Setup_vars_saving(IAgs target,
                                        PropertySavingMetadata metadata,
                                        IndentedStringBuilder sb)
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
}