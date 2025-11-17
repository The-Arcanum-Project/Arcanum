using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.InjectReplaceLogic;

public enum SavingCategory
{
   ForcedOverride, // If there is a specific file objects of this type must be saved to.
   FileOverride,
   Inject,
   TryInject,
   InjectOrCreate,
   Replace,
   TryReplace,
   ReplaceOrCreate,
   Modify,
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
         InjRepType.INJECT => SavingCategory.Inject,
         InjRepType.TRY_INJECT => SavingCategory.TryInject,
         InjRepType.INJECT_OR_CREATE => SavingCategory.InjectOrCreate,
         InjRepType.REPLACE => SavingCategory.Replace,
         InjRepType.TRY_REPLACE => SavingCategory.TryReplace,
         InjRepType.REPLACE_OR_CREATE => SavingCategory.ReplaceOrCreate,
         _ => SavingCategory.FileOverride,
      };
   }

   public static InjRepType ToInjRepStrategy(this SavingCategory category)
   {
      return category switch
      {
         SavingCategory.Inject => InjRepType.INJECT,
         SavingCategory.TryInject => InjRepType.TRY_INJECT,
         SavingCategory.InjectOrCreate => InjRepType.INJECT_OR_CREATE,
         SavingCategory.Replace => InjRepType.REPLACE,
         SavingCategory.TryReplace => InjRepType.TRY_REPLACE,
         SavingCategory.ReplaceOrCreate => InjRepType.REPLACE_OR_CREATE,
         _ => InjRepType.None,
      };
   }
}