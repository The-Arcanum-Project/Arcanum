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

      public static class SpecializedEditors
      {
         private const string SPECIALIZED_PATH = $"{PATH}.{nameof(SpecializedEditors)}";

         public static readonly FeatureId InstitutionEditor = Create(SPECIALIZED_PATH);
         public static readonly FeatureId PoliticalEditor = Create(SPECIALIZED_PATH);
      }
   }
}