using static Arcanum.UI.AppFeatures.FeatureId;

namespace Arcanum.UI.AppFeatures;

public static class FeatureIds
{
   public static class Editor
   {
      private const string PATH = nameof(Editor);

      public static readonly FeatureId MainWindow = Create(PATH);

      public static readonly FeatureId Queastor = Create(PATH);
      public static readonly FeatureId Map = Create(PATH);
   }
}