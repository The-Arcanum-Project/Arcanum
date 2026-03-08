using Arcanum.UI.AppFeatures.FeatureInitializers;
using Common.Logger;

namespace Arcanum.UI.AppFeatures;

public static class FeatureLibrary
{
   public static void Initialize()
   {
      // All Features from ../FeatureInitializers should be initialized here
      EditorFeatures.Initialize();
      SpecializedEditorFeatures.Initialize();

      ArcLog.Write("DOC", LogLevel.INF, "Self-Documentation Engine loaded {0} features.", FeatureRegistry.GetAllFeatures().Count);
   }

   public static AppFeature AddToRegistry(this AppFeature feature)
   {
      FeatureRegistry.Register(feature);
      return feature;
   }
}