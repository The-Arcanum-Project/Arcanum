using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.Analytics.MapData;
using Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.SpecializedEditors.Management;
using Common;
using Common.UI;

namespace Arcanum.UI.SpecializedEditors.Editors;

public class ProvinceEditor : ISpecializedEditor
{
   private bool _wasValidated;
   public string DisplayName => "Province Children Editor";
   public string? IconResource => null;
   public int Priority => 10;
   public bool SupportsMultipleTargets => false;

   public ProvinceEditor()
   {
      FileStateManager.FileChanged += OnFileStateManagerOnFileChanged;
   }

   ~ProvinceEditor()
   {
      FileStateManager.FileChanged -= OnFileStateManagerOnFileChanged;
   }

   public bool CanEdit(object[] targets, Enum? prop)
   {
      if (!_wasValidated)
      {
         _wasValidated = AnalyzeLocationCollections.VerifyUniquenessOfChildren(Globals.Provinces.Values
                                                                                 .Cast<ILocationCollection<Location>>()
                                                                                 .ToArray(),
                                                                               out var msgs);

         if (!_wasValidated)
         {
            var errorString = "Province Children Editor cannot be opened due to the following errors:\n" +
                              string.Join("\n", msgs.Select(m => $"- {m}"));
            UIHandle.Instance.PopUpHandle.ShowMBox(errorString, "Cannot Open Province Children Editor");
            return false;
         }
      }

      return targets.All(t => t is Province);
   }

   private void OnFileStateManagerOnFileChanged(object? _, FileChangedEventArgs args)
   {
      if (args.FullPath.EndsWith("definitions.txt"))
         _wasValidated = false;
   }

   public void Reset()
   {
   }

   public void ResetFor(object[] targets)
   {
   }

   public FrameworkElement GetEditorControl()
   {
      return new TextBlock
      {
         Text = "Province Children Editor UI goes here.",
         VerticalAlignment = VerticalAlignment.Center,
         HorizontalAlignment = HorizontalAlignment.Center,
      };
   }

   public IEnumerable<MenuItem> GetContextMenuActions() => [];
}