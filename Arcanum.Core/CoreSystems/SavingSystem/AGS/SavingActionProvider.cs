using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.GameObjects.Map;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

/// <summary>
/// The class provides saving methods for AGS properties. <br/>
/// Custom saving methods can be defined here and referenced in <see cref="PropertySavingMetadata.SavingMethod"/>.
/// </summary>
public static class SavingActionProvider
{
   public static void ExampleCustomSavingMethod(IAgs target, PropertySavingMetadata metadata, IndentedStringBuilder sb)
   {
      object value = null!;
      Nx.ForceGet(target, metadata.NxProp, ref value);
      sb.AppendLine($"# Custom saving for property: {metadata.Keyword}");
      sb.AppendLine($"# Value: {value}");
      sb.AppendLine($"{metadata.Keyword} = {value}");
   }

   public static void RoadSavingMethod(IAgs target, HashSet<PropertySavingMetadata> metadata, IndentedStringBuilder sb)
   {
      if (target is not Road road)
         throw new InvalidOperationException("RoadSavingMethod can only be used with AgsRoad instances.");

      sb.AppendLine($"{road.StartLocation.Name} = {road.EndLocation.Name}");
   }
}