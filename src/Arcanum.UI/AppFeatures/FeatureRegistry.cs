namespace Arcanum.UI.AppFeatures;

public static class FeatureRegistry
{
   private static readonly Dictionary<string, IAppFeature> Features = new();
   private static readonly Dictionary<string, IAppFeature> ActiveFeatures = new();

   public static void Register(IAppFeature feature) => Features[feature.Id] = feature;

   public static void AddActiveFeature(IAppFeature feature)
   {
      ActiveFeatures[feature.Id] = feature;
   }

   public static void RemoveActiveFeature(IAppFeature feature)
   {
      ActiveFeatures.Remove(feature.Id);
   }

   public static IReadOnlyCollection<IAppFeature> GetAllFeatures() => Features.Values;
   public static IAppFeature? GetFeature(string id) => Features.GetValueOrDefault(id);
   public static IReadOnlyCollection<IAppFeature> GetActiveFeatures() => ActiveFeatures.Values;
}