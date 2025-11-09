using System.Windows;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Common.UI;
using Common.UI.MBox;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.InjectReplaceLogic;

public static class InjectionHelper
{
   public static void HandleObjectsWithOptionalInjectLogic(List<IEu5Object> objsToSave)
   {
      var categorized = CategorizeSaveableObjects(objsToSave).ToList();
      EnsureCompatibilityWithExistingInjections(categorized);
      PickFilesForInjection(categorized);
      // Every object now knows how and where it needs to be saved.
      CalculateFinalInjects(categorized);
      // We now know that all objects are compatible, have a file to save to and have their final injects calculated.
      SaveMaster.AppendOrCreateFileWithInjects(categorized);
   }

   private static void CalculateFinalInjects(List<CategorizedSaveable> cssos)
   {
      foreach (var csso in cssos)
      {
         List<(Enum, object)> finalInjects = [];
         var existingInjects = csso.Target.GetInjects();

         var target = csso.Target;
         if (!target.Source.IsVanilla)
         {
            UIHandle.Instance.PopUpHandle
                    .ShowMBox("Attempted to save an injection for an object from a non vanilla source. " +
                              "This is not allowed as only vanilla objects can be modified directly. \n" +
                              "WHY DON'T WE SUPPORT THIS? \n Ask Stiopa",
                              "Invalid Injection Target",
                              MBoxButton.OK,
                              MessageBoxImage.Error);
            continue;
         }

         // TODO: Get the info from commands otherwise this shit
         foreach (var prop in target.GetAllProperties())
         {
            var dVal = target.GetDefaultValue(prop);
            var curVal = target._getValue(prop);

            // No change
            if (Equals(dVal, curVal))
               continue;

            if (csso.SavingCategory.IsReplace())
            {
               // In case of a replace we do not care about existing injects
               finalInjects.Add((prop, curVal));
               continue;
            }

            if (existingInjects.Length != 0)
               for (var i = 0; i < existingInjects.Length; i++)
               {
                  var (@enum, value) = existingInjects[i];
                  if (Equals(@enum, prop))
                  {
                     // Found the property in the existing injects but the value is different than
                     // what we have now so we need to update it
                     if (!Equals(value, curVal))
                        finalInjects.Add((@enum, curVal));
                     // We handled this property, break
                     break;
                  }
               }

            // If we reach this point the property was not found in existing injects
            // so we need to add it
            finalInjects.Add((prop, curVal));
         }

         csso.Injects = finalInjects.ToArray();
      }
   }

   internal static CategorizedSaveable[] CategorizeSaveableObjects(List<IEu5Object> objsToSave)
   {
      var categorized = new CategorizedSaveable[objsToSave.Count];

      // Replace and Inject logic
      for (var i = 0; i < objsToSave.Count; i++)
      {
         var obj = objsToSave[i];
         var numOfProperties = obj.GetAllProperties().Count;
         var changes = SaveMaster.GetChangesForObject(obj);
         var percentageModified = changes.Length / (float)numOfProperties;

         SavingCategory category;
         if (percentageModified >= Config.Settings.SavingConfig.MaxPercentageToInject)
         {
            category = SavingCategoryExtensions.FromInjRepStrategy(Config.Settings.SavingConfig.DefaultReplaceType);
            // The object is not from vanilla so we cannot just inject to it but have to create it if it does not exist
            // TryInject would also work if you count no errors as working but we don't.
            if (!obj.Source.IsVanilla && category is SavingCategory.Inject or SavingCategory.TryInject)
               category = SavingCategory.InjectOrCreate;
         }
         else
         {
            category = SavingCategoryExtensions.FromInjRepStrategy(Config.Settings.SavingConfig.DefaultInjectType);
            // The object is not from a vanilla source (this includes base mods so we have to use ReplaceOrCreate
            if (!obj.Source.IsVanilla && category is SavingCategory.Replace or SavingCategory.TryReplace)
               category = SavingCategory.ReplaceOrCreate;
         }

         categorized[i] = new(obj, category);
      }

      return categorized;
   }

   /// <summary>
   /// Creates a new fileobject for the injections
   /// </summary>
   public static void PickFilesForInjection(List<CategorizedSaveable> cssos)
   {
      Dictionary<Type, List<CategorizedSaveable>> byType = new();
      foreach (var csso in cssos)
      {
         var type = csso.Target.GetType();
         if (!byType.ContainsKey(type))
            byType[type] = [];
         byType[type].Add(csso);
      }

      foreach (var typeGroup in byType.Values)
      {
         var (path, descriptor) = FileManager.GeneratePathForNewObject(typeGroup[0].Target, true);
         var fo = new Eu5FileObj(new(path[..^1], path[^1], FileManager.ModDataSpace), descriptor);

         foreach (var csso in typeGroup)
            if (csso.SaveLocation == Eu5FileObj.Empty)
               csso.SaveLocation = fo;
      }
   }

   /// <summary>
   /// Makes sure all existing injections for this object are compatible with the new injections.
   /// If not the object is removed from the list and a message box is shown to the user.
   /// <br/>
   /// <br/>
   /// Also sets the fileObject to save to.
   /// <br/>
   /// TODO: In the future we can also think about merging injections if they are compatible. 
   /// </summary>
   public static void EnsureCompatibilityWithExistingInjections(List<CategorizedSaveable> cssos)
   {
      for (var i = cssos.Count - 1; i >= 0; i--)
      {
         var csso = cssos[i];

         if (csso.SavingCategory == SavingCategory.FileOverride ||
             !InjectManager.Injects.TryGetValue(csso.Target, out var existingInj))
            continue;

         // We have to check that all of them are inject or there is one singular replace
         // If there are injects and replaces mixed we cannot merge them safely and the user has to override
         // the file and thus the entire object to ensure all the changes are applied correctly.
         // If we have only injects in a vanilla and a mod source we can simply apply one more inject on top.
         // InjectCollision defines what we do in that case. Default is to create a 2nd inject.
         var isInject = csso.SavingCategory.IsInject();
         var isIllegal = false;
         List<Eu5FileObj> injectsInModFiles = [];
         foreach (var ej in existingInj)
         {
            // We check if they are the of the same category (inject vs replace)
            if (SavingCategoryExtensions.FromInjRepStrategy(ej.InjRepType).IsInject() == isInject)
            {
               if (ej.SourceFile.IsModded)
                  injectsInModFiles.Add(ej.SourceFile);
               continue;
            }

            // We have a conflict
            UIHandle.Instance.PopUpHandle.ShowMBox($"The object \"{ej.Target.UniqueId}\" is being modified with " +
                                                   $"an injection of type \"{SavingCategoryExtensions.FromInjRepStrategy(ej.InjRepType)}\") " +
                                                   $"as well as an injection of type \"{csso.SavingCategory}\". " +
                                                   $"Mixing injection and replacement modifications is not supported. " +
                                                   $"Please either change the injection type of one of the injections to resolve this conflict, " +
                                                   $"or override the file containing the object to ensure all changes are applied correctly. \n\n" +
                                                   $"Any change to this object WILL NOT BE SAVED until this conflict is resolved.",
                                                   "Injection Conflict Detected",
                                                   MBoxButton.OK,
                                                   MessageBoxImage.Exclamation);
            isIllegal = true;
            break;
         }

         // Remove the illegal one
         if (isIllegal)
         {
            cssos.RemoveAt(i);
            continue;
         }

         // Only compatible types are found so we check if we already have an inject in the mod to expand it.
         // If not the Eu5FileObj will remain empty and be assigned in the next step.
         if (injectsInModFiles.Count > 0)
            csso.SaveLocation = injectsInModFiles[0];
      }
   }
}