#region

using Arcanum.UI.Documentation.Implementation;

#endregion

namespace Arcanum.UI.AppFeatures;

public static class FeatureRegistry
{
   private static readonly Dictionary<string, FeatureDoc> ActiveFeatures = new();

   public static void AddActiveFeature(FeatureDoc feature)
   {
      ActiveFeatures[feature.Id] = feature;
   }

   public static void RemoveActiveFeature(FeatureDoc feature)
   {
      ActiveFeatures.Remove(feature.Id);
   }

   public static IReadOnlyCollection<FeatureDoc> GetActiveFeatures() => ActiveFeatures.Values;
}