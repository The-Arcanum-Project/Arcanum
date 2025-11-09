using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.InjectReplaceLogic;

public enum SavingCategory
{
   FileOverride,
   Inject,
   TryInject,
   InjectOrCreate,
   Replace,
   TryReplace,
   ReplaceOrCreate,
}

public static class SavingCategoryExtensions
{
   public static bool IsInject(this SavingCategory category)
   {
      return category is SavingCategory.Inject or SavingCategory.TryInject or SavingCategory.InjectOrCreate;
   }

   public static bool IsReplace(this SavingCategory category)
   {
      return category is SavingCategory.Replace or SavingCategory.TryReplace or SavingCategory.ReplaceOrCreate;
   }

   public static SavingCategory FromInjRepStrategy(InjRepType strategy)
   {
      return strategy switch
      {
         InjRepType.Inject => SavingCategory.Inject,
         InjRepType.TryInject => SavingCategory.TryInject,
         InjRepType.InjectOrCreate => SavingCategory.InjectOrCreate,
         InjRepType.Replace => SavingCategory.Replace,
         InjRepType.TryReplace => SavingCategory.TryReplace,
         InjRepType.ReplaceOrCreate => SavingCategory.ReplaceOrCreate,
         _ => SavingCategory.FileOverride,
      };
   }

   public static InjRepType ToInjRepStrategy(this SavingCategory category)
   {
      return category switch
      {
         SavingCategory.Inject => InjRepType.Inject,
         SavingCategory.TryInject => InjRepType.TryInject,
         SavingCategory.InjectOrCreate => InjRepType.InjectOrCreate,
         SavingCategory.Replace => InjRepType.Replace,
         SavingCategory.TryReplace => InjRepType.TryReplace,
         SavingCategory.ReplaceOrCreate => InjRepType.ReplaceOrCreate,
         _ => InjRepType.None,
      };
   }
}