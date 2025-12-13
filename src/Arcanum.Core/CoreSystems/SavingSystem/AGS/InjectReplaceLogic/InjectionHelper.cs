using System.Diagnostics;
using System.Windows;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;
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
      CalculateFinalInjects(categorized, out var toRemoveFromFiles, out var abort);
      if (abort)
         return;

      // We now know that all objects are compatible, have a file to save to and have their final injects calculated.
      SaveMaster.AppendOrCreateFiles(categorized, toRemoveFromFiles);
   }

   private static void CalculateFinalInjects(List<CategorizedSaveable> cssos,
                                             out List<InjectObj> toRemoveFromFiles,
                                             out bool abort)
   {
      abort = false;
      toRemoveFromFiles = [];

      Dictionary<IEu5Object, List<Enum>> changedProperties = new();
      CalculatedChangedProperties(changedProperties);

      foreach (var csso in cssos)
      {
         if (csso.SavingCategory == SavingCategory.FileOverride)
         {
            // We do not add it to objects in file here as the updating of the objects in an EU5 FileObj will do so
            csso.SaveLocation.ObjectsInFile.Add(csso.Target);
            continue;
         }

         List<(Enum, object)> finalInjects = [];

         var target = csso.Target;
         // If the target's source is Empty we know that we have a newly created object that does not get saved by injection.
         // So we just add it to the objects in the file and it will be saved via Modification write.
         if (target.Source == Eu5FileObj.Empty)
            continue;

         if (!target.Source.IsVanilla)
         {
            // If the target is not a replace or inject we just have a mod defined object that has to be rewritten.
            if (target.InjRepType == InjRepType.None)
            {
               csso.SavingCategory = SavingCategory.Modify;
               continue;
            }

            // We can not inject into an object that is not from a vanilla source unless we are replacing it.
            if (target.InjRepType is InjRepType.INJECT or InjRepType.TRY_INJECT or InjRepType.INJECT_OR_CREATE)
            {
               UIHandle.Instance.PopUpHandle
                       .ShowMBox("Attempted to save an injection for an object from a non vanilla source. " +
                                 "This is not allowed as only vanilla objects can be modified directly. \n" +
                                 "WHY DON'T WE SUPPORT THIS? \n Ask Stiopa",
                                 "Invalid Injection Target",
                                 MBoxButton.OK,
                                 MessageBoxImage.Error);
               abort = true;
               continue;
            }

            Debug.Assert(csso.SavingCategory.IsReplace(),
                         "Saving category is not replace for an object that is marked as replace.");
         }

         if (!changedProperties.TryGetValue(target, out var changedProps))
         {
#if DEBUG
            Debug.Fail("Changed properties not found for an object that is supposed to be saved.");
#else
            ArcLog.WriteLine("IJH",
                             LogLevel.CRT,
                             "Changed properties not found for an object that is supposed to be saved.");
            UIHandle.Instance.PopUpHandle
                    .ShowMBox("Changed properties not found for an object that is supposed to be saved.",
                              "Internal Error",
                              MBoxButton.OK,
                              MessageBoxImage.Error);
#endif
            abort = true;
            continue;
         }

         // TODO: For now this is "dumb" injection handling for collections.
         // In the future we have to only do this if we have a remove or clear operation on the collection,
         // or if we are adding smth that is already contained in the collection to prevent it showing up twice in game.
         foreach (var cp in changedProps)
            if (csso.Target.IsCollection(cp))
            {
               csso.SavingCategory =
                  SavingCategoryExtensions.FromInjRepStrategy(Config.Settings.SavingConfig.DefaultReplaceType);
               break;
            }

         switch (csso.SavingCategory)
         {
            case SavingCategory.FileOverride:
               Debug.Fail("FileOverride should not be handled here.");
               break;
            case SavingCategory.Inject:
            case SavingCategory.TryInject:
            case SavingCategory.InjectOrCreate:
               foreach (var cp in changedProps)
                  finalInjects.Add((cp, target._getValue(cp)));

               var existingInjects = csso.Target.GetInjectsForTarget();
               // We have existing injects, so we check if they are in the same file and category so that we can merge them.
               if (existingInjects.Length > 0)
                  // At this point we know that all existing injects are of the same type (inject) and compatible.
                  // The only time we could have switched type is the collection check which would have made us replace.
                  // So we simply add all existing injects to the final injects to be saved.
                  foreach (var ei in existingInjects)
                  {
                     // Stuff in base mods cannot be merged into our mod inject so we skip them.
                     if (ei.Source.IsVanilla)
                        // TODO: Create a Replace here if valid? @Stiopa
                        continue;

                     foreach (var prop in ei.InjectedProperties)
                     {
                        if (csso.Target.IsCollection(prop.Key))
                        {
                           finalInjects.Add((prop.Key, prop.Value));
                           continue;
                        }

                        // If we have already added this property from the changed properties we skip it. 
                        // This might not be the ideal action to take as they might be complementing each other but for now this is fine.
                        // This check is skipped if we have a collection as we want to always add all changes in that case.
                        if (finalInjects.Exists(fi => fi.Item1.Equals(prop.Key)))
                           continue;

                        finalInjects.Add((prop.Key, prop.Value));
                     }

                     // We mark the existing inject for removal as we are merging it into our new inject.
                     toRemoveFromFiles.Add(ei);
                     InjectManager.UnregisterInjectObj(ei);
                  }

               break;
            case SavingCategory.Replace:
            case SavingCategory.TryReplace:
            case SavingCategory.ReplaceOrCreate:
               foreach (var prop in target.GetAllProperties())
                  finalInjects.Add((prop, target._getValue(prop)));

               var eInjects = csso.Target.GetInjectsForTarget();

               if (csso.Target.InjRepType is InjRepType.REPLACE
                                          or InjRepType.TRY_REPLACE
                                          or InjRepType.REPLACE_OR_CREATE)
               {
                  // If the target is already a replace we can simply modify it
                  csso.SavingCategory = SavingCategory.Modify;
                  continue;
               }

               // We have existing injects are are now going to perform a check if we can simply bundle them into a single replace.
               // This is only possible if there are no conflicting properties.
               if (eInjects.Length > 0)
                  foreach (var injObject in eInjects)
                  {
                     if (injObject.Source.IsVanilla)
                     {
                        // Any action we take will cause issues with the base mod. So we abort here.
                        // TODO: Add a setting or use the DialogResult here to determine if we should continue taking the risk
#if DEBUG
                        Debug.Fail("Existing injects from a base mod found for an object that is being replaced.");
#else
                        ArcLog.WriteLine("IJH",
                                         LogLevel.CRT,
                                         "Existing injects in a basemod found for an object that is being replaced.");
                        UIHandle.Instance.PopUpHandle
                                .ShowMBox("Existing injects found for an object that is being replaced.",
                                          "Internal Error",
                                          MBoxButton.OK,
                                          MessageBoxImage.Error);
#endif
                        abort = true;
                        return;
                     }

                     // As we are replacing which is pulling all properties from the objects we can simply remove all existing injects.
                     // And as all injects are already in the finalInjects we don't have to do anything special here.
                     toRemoveFromFiles.Add(injObject);
                  }

               break;
            default:
               throw new ArgumentOutOfRangeException();
         }

         var newInjectObj = new InjectObj
         {
            Target = csso.Target,
            Source = csso.SaveLocation,
            InjRepType = csso.SavingCategory.ToInjRepStrategy(),
            InjectedProperties =
               finalInjects.Select(fi => new KeyValuePair<Enum, object>(fi.Item1, fi.Item2)).ToArray(),
            FileLocation = Eu5ObjectLocation.Empty,
         };
         InjectManager.RegisterInjectObj(newInjectObj);
         csso.InjectedObj = newInjectObj;
         csso.InjectedObj.Source.ObjectsInFile.Add(csso.InjectedObj);

         csso.Injects = finalInjects.ToArray();
      }
   }

   private static void CalculatedChangedProperties(Dictionary<IEu5Object, List<Enum>> cps)
   {
      var commands =
         AppData.HistoryManager.GetCommandsSinceLastSave(SaveMaster.LastSavedHistoryNode ??
                                                         AppData.HistoryManager.Root);
      foreach (var command in commands)
      {
         foreach (var target in command.GetTargets())
         {
            if (!cps.ContainsKey(target))
               cps[target] = [];
            cps[target].Add(command.Attribute!);
         }
      }
   }

   internal static CategorizedSaveable[] CategorizeSaveableObjects(List<IEu5Object> objsToSave)
   {
      var categorized = new CategorizedSaveable[objsToSave.Count];

      // Replace and Inject logic
      for (var i = 0; i < objsToSave.Count; i++)
      {
         var obj = objsToSave[i];

         SavingCategory category;
         var descriptor = DescriptorDefinitions.TypeToDescriptor[obj.GetType()];
         if (!descriptor.AllowMultipleFiles || obj.FileLocation == Eu5ObjectLocation.Empty)
         {
            // We get the single allowed path and do not move it to our mod as the saving does so
            categorized[i] = new(obj, SavingCategory.FileOverride) { SaveLocation = descriptor.Files[0] };
            continue;
         }

         var numOfProperties = obj.GetAllProperties().Length;
         var changes = SaveMaster.GetChangesForObject(obj);
         var percentageModified = changes.Length / (float)numOfProperties;

         if (percentageModified >= Config.Settings.SavingConfig.MaxPercentageToInject ||
             obj.InjRepType is InjRepType.REPLACE or InjRepType.TRY_REPLACE or InjRepType.REPLACE_OR_CREATE)
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
            // The object is not from a vanilla source (this includes base mods so we have to use ReplaceOrCreate)

            if (!obj.Source.IsVanilla)
            {
               if (category is SavingCategory.Replace or SavingCategory.TryReplace)
                  category = SavingCategory.ReplaceOrCreate;
            }
            else
            {
               Debug.Assert(obj.Source != Eu5FileObj.Empty,
                            "Object source is empty in injection categorization despite previous checks.");
               Debug.Assert(obj.Source.IsVanilla,
                            "Object source is not vanilla in injection categorization despite previous checks.");
               categorized[i] = new(obj, category) { SaveLocation = obj.Source };
               continue;
            }
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
         var obj = typeGroup[0];

         if (obj.SavingCategory == SavingCategory.FileOverride)
         {
            if (obj.SaveLocation != Eu5FileObj.Empty)
               continue;

            // If we want to edit vanilla files we have to save to the original source
            if (!Config.Settings.SavingConfig.MoveFilesToModdedDataSpaceOnSaving)
            {
               obj.SaveLocation = obj.Target.Source;
               continue;
            }
         }

         var fo = FileStateManager.CreateEu5FileObject(obj.Target);

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
            var ejStrat = SavingCategoryExtensions.FromInjRepStrategy(ej.InjRepType);
            // We check if they are the of the same category (inject vs replace)
            if (ejStrat.IsInject() == isInject)
            {
               if (ej.Source.IsModded)
                  injectsInModFiles.Add(ej.Source);
               continue;
            }

            // If we have a replace already in the mod for this object and we want to inject, we promote the inject to 
            // a replace which will just rewrite the entire object.
            if (ejStrat.IsReplace() && isInject && ej.Source.IsModded)
            {
               csso.SavingCategory =
                  SavingCategoryExtensions.FromInjRepStrategy(Config.Settings.SavingConfig.DefaultReplaceType);
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