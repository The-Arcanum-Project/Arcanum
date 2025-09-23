using Arcanum.Core.AgsRegistry;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Jomini.Date;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.Court;
using Arcanum.Core.GameObjects.Map;
using Nexus.Core;

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
         throw new
            InvalidOperationException("SaveIdentifierStringKvp can only be used with IIdentifierStringKvp instances.");

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

      sb.AppendLine(date.ToString());
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
         throw new InvalidOperationException("SaveIAgsEnumKvp can only be used with IIagsEnumKvp<IAgs> instances.");

      sb.AppendLine($"{kvp.Key.SavingKey} = {EnumAgsRegistry.GetKey(kvp.Value)}");
   }
}