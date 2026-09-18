#region

using System.Windows.Input;
using Arcanum.Core.CoreSystems.Clipboard;
using Arcanum.Core.CoreSystems.Nexus;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.Core.GameObjects.InGame.Pops;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.Windows.MinorWindows.PopUpEditors;
using CommunityToolkit.Mvvm.Input;

#endregion

namespace Arcanum.UI.Components.UserControls.ValueAllocators;

public sealed class PopEditorVm : ViewModelBase
{
   public PopEditorVm()
   {
      Title = "Pop Editor";
      PasteFromLocationCommand = new RelayCommand<Location>(PastePopsFromLocation);
      PasteFromLocationVariationCommand = new RelayCommand<Location>(PastePopsFromLocationWithVariation);
      UndoCommand = new RelayCommand(Undo);
      ApplyChangesCommand = new RelayCommand(ApplyChanges);

      if (Selection.GetSelectedLocations.Count == 1)
         ActiveEditor = new AllocatorViewModel(Selection.GetSelectedLocations[0]);
      else if (Selection.GetSelectedLocations.Count > 1)
         ActiveEditor = new MassPopPainterViewModel(Selection.GetSelectedLocations.ToArray());
   }

   public object? ActiveEditor
   {
      get;
      set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
      }
   }

   public bool PasteInEntireSelection { get; set; }
   public bool ReplaceExistingPops { get; set; }
   public float PasteVariationMax
   {
      get;
      set
      {
         if (value.Equals(field))
            return;

         field = value;
         OnPropertyChanged();
      }
   }
   public float PasteVariationMin
   {
      get;
      set
      {
         if (value.Equals(field))
            return;

         field = value;
         OnPropertyChanged();
      }
   }
   public ICommand PasteFromLocationCommand { get; }
   public ICommand PasteFromLocationVariationCommand { get; }
   public ICommand UndoCommand { get; }
   public ICommand ApplyChangesCommand { get; }
   public string Title
   {
      get;
      set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
      }
   }

   private void ApplyChanges()
   {
      if (ActiveEditor is AllocatorViewModel avm)
         avm.ApplyChanges();
      else if (ActiveEditor is MassPopPainterViewModel mass)
         mass.ApplyChanges();
   }

   private void Undo()
   {
      if (ActiveEditor is AllocatorViewModel avm)
         avm.Undo();
      else if (ActiveEditor is MassPopPainterViewModel mass)
         mass.Undo();
   }

   private void PastePopsFromLocationWithVariation(Location? obj)
   {
      PastePops(true);
   }

   private void PastePopsFromLocation(Location? obj)
   {
      PastePops(false);
   }

   private void PastePops(bool withVariation)
   {
      if (ArcClipboard.CurrentPayload?.Value is not Location cl)
         return;

      var random = new Random();
      var diff = 0d;
      var avm = ActiveEditor as AllocatorViewModel;
      var targets = PasteInEntireSelection
                       ? Selection.GetSelectedLocations.ToList()
                       : [avm?.LoadedLocation ?? Location.Empty];

      foreach (var location in targets)
      {
         // Skip invalid locations or pasting into the source itself
         if (location == null! || location == Location.Empty || location == cl || !Globals.DefaultMapDefinition.IsLand(location))
            continue;

         if (ReplaceExistingPops)
         {
            Nx.RemoveRangeFromCollection(location, Location.Field.Pops, location.Pops);
            if (avm != null && location == avm.LoadedLocation)
            {
               avm.Items.Clear();
               avm._totalLimit = 0;
            }
         }

         foreach (var pop in cl.Pops)
         {
            var newPop = (PopDefinition)pop.DeepClone();
            if (withVariation)
            {
               var step1 = (float)random.NextDouble() * (PasteVariationMax - PasteVariationMin) + PasteVariationMin;
               step1 *= random.NextDouble() < 0.5f ? -1 : 1;
               var variation = 1 + step1 / 100f;

               newPop.Size *= Math.Max(0.001f, variation);
            }

            AddToExistingOrNew(newPop, location);

            // Only update the UI list if we are looking at this location
            if (avm != null && location == avm.LoadedLocation)
            {
               avm.AddItem(newPop, false);
               diff += newPop.Size;
            }
         }
      }

      avm?._totalLimit += (int)(diff * 1000);

      avm?.RunAutoLogScale();
      avm?.OnPropertyChanged(nameof(avm.TotalLimit));
   }

   private static void AddToExistingOrNew(PopDefinition @new, Location target)
   {
      foreach (var pop in target.Pops)
         if (pop.Culture == @new.Culture && pop.Religion == @new.Religion && pop.PopType == @new.PopType)
         {
            Nx.Set(pop, PopDefinition.Field.Size, pop.Size + @new.Size);
            return;
         }

      Nx.AddToCollection(target, Location.Field.Pops, @new);
   }
}