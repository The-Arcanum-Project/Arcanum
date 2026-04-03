#region

using System.Collections.ObjectModel;
using System.Windows.Input;
using Arcanum.UI.AppFeatures;
using Arcanum.UI.Commands;
using Arcanum.UI.Components.Windows.DebugWindows;
using Arcanum.UI.Documentation.Implementation;

#endregion

namespace Arcanum.UI.Components.Windows.HelpWindow.ViewModels;

public class FeatureExplorerViewModel : HelpPageViewModelBase
{
   public FeatureExplorerViewModel()
   {
      SetFeatures();
      SpotlightCommand = new RelayCommand(_ => ExecuteSpotlight());
      UpdateLocationGrid(FeatureLocation.Center, FeatureScale.Standard); // Default
   }

   public override string Title => "Feature Explorer";

   public ObservableCollection<FeatureItem> Features { get; } = [];
   public event Action<FeatureItem?>? RequestSelectionUpdate;

   public string SearchQuery
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
         ApplyFilter();
      }
   } = "";

   public FeatureItem? SelectedItem
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
         OnPropertyChanged(nameof(SelectedFeature));
         UpdateDetails();
      }
   }

   public FeatureDoc? SelectedFeature => SelectedItem?.Documentation;

   public List<IAppCommand> AssociatedCommands
   {
      get;
      private set
      {
         field = value;
         OnPropertyChanged();
      }
   } = [];

   public List<LocationGridCell> LocationGridCells
   {
      get;
      private set
      {
         field = value;
         OnPropertyChanged();
      }
   } = [];

   public ICommand SpotlightCommand { get; }

   private void SetFeatures()
   {
      Features.Clear();

      foreach (var feature in DocuRegistry.GetAllDocuPages)
         Features.Add(new(feature));
   }

   public void SelectFeature(FeatureDoc feature)
   {
      var item = FindFeature(Features, feature);
      if (item != null)
      {
         SelectedItem = item;
         RequestSelectionUpdate?.Invoke(item);
      }
   }

   private static FeatureItem? FindFeature(ObservableCollection<FeatureItem> features, FeatureDoc feature)
   {
      foreach (var item in features)
         if (item.Documentation.Id == feature.Id)
            return item;

      return null;
   }

   private void UpdateDetails()
   {
      if (SelectedFeature == null)
         return;

      // Find Commands for this feature's scopes
      var scopes = SelectedFeature.AssociatedScopes.ToHashSet();
      AssociatedCommands = CommandRegistry.AllCommands
                                          .Where(c => scopes.Contains(c.Scope))
                                          .ToList();

      // Update the Spatial Map
      UpdateLocationGrid(SelectedFeature.Location, SelectedFeature.Scale);
   }

   private void UpdateLocationGrid(FeatureLocation location, FeatureScale scale)
   {
      var primaries = new HashSet<int>();
      var secondaries = new HashSet<int>();

      // Identify the Core Index
      var core = location switch
      {
         FeatureLocation.TopLeft => 0,
         FeatureLocation.Top => 1,
         FeatureLocation.TopRight => 2,
         FeatureLocation.Left => 3,
         FeatureLocation.Center => 4,
         FeatureLocation.Right => 5,
         FeatureLocation.BottomLeft => 6,
         FeatureLocation.Bottom => 7,
         FeatureLocation.BottomRight => 8,
         _ => 4,
      };

      primaries.Add(core);

      // Calculate "Spillover" based on Scale
      if (scale == FeatureScale.Full)
      {
         for (var i = 0; i < 9; i++)
            if (i != core)
               secondaries.Add(i);
      }
      else if (scale == FeatureScale.Major)
         // Major components expand to their immediate neighbors
         switch (location)
         {
            case FeatureLocation.Top:
               secondaries.UnionWith([0, 2]);
               break;
            case FeatureLocation.Bottom:
               secondaries.UnionWith([6, 8]);
               break;
            case FeatureLocation.Left:
               secondaries.UnionWith([0, 6]);
               break;
            case FeatureLocation.Right:
               secondaries.UnionWith([2, 8]);
               break;
            case FeatureLocation.Center:
               secondaries.UnionWith([1, 3, 5, 7]);
               break;
            case FeatureLocation.TopLeft:
               secondaries.UnionWith([1, 3]);
               break;
            case FeatureLocation.TopRight:
               secondaries.UnionWith([1, 5]);
               break;
            case FeatureLocation.BottomLeft:
               secondaries.UnionWith([3, 7]);
               break;
            case FeatureLocation.BottomRight:
               secondaries.UnionWith([5, 7]);
               break;
            case FeatureLocation.Floating:
            case FeatureLocation.Contextual:
               secondaries.UnionWith([0, 1, 2, 3, 5, 6, 7, 8]);
               break;
         }
      // Standard and Compact only occupy the core cell

      // Special visual for "Floating/Contextual"
      // We can make these look distinct by always making them "Center" but adding 
      // a different secondary pattern.

      var cells = new List<LocationGridCell>();
      for (var i = 0; i < 9; i++)
         cells.Add(new(primaries.Contains(i), secondaries.Contains(i)));
      LocationGridCells = cells;
   }

   private void ApplyFilter()
   {
      var query = SearchQuery.Trim();
      foreach (var item in Features)
      {
         var matches = item.Documentation.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                       item.Documentation.Summary.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                       item.Documentation.SearchKeywords.Any(s => s.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                       item.Documentation.Id.Value.Contains(query, StringComparison.OrdinalIgnoreCase);

         item.IsVisible = matches;
      }
   }

   private void ExecuteSpotlight()
   {
   }
}