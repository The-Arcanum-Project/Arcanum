using System.Collections.ObjectModel;
using System.Windows.Input;
using Arcanum.UI.AppFeatures;
using Arcanum.UI.Commands;
using Arcanum.UI.Components.Windows.DebugWindows;

namespace Arcanum.UI.Components.Windows.HelpWindow.ViewModels;

public class FeatureExplorerViewModel : HelpPageViewModelBase
{
   public FeatureExplorerViewModel()
   {
      BuildTree();
      SpotlightCommand = new RelayCommand(_ => ExecuteSpotlight());
      UpdateLocationGrid(FeatureLocation.Center, FeatureScale.Standard); // Default
   }

   public override string Title => "Feature Explorer";

   public ObservableCollection<FeatureTreeItem> FeatureTree { get; } = [];
   public event Action<FeatureTreeItem?>? RequestSelectionUpdate;

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

   public FeatureTreeItem? SelectedItem
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

   public IAppFeature? SelectedFeature => SelectedItem?.Feature;

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

   private void BuildTree()
   {
      FeatureTree.Clear();
      var all = FeatureRegistry.GetAllFeatures().ToList();

      // Find roots (no parent)
      var roots = all.Where(f => string.IsNullOrEmpty(f.ParentFeatureId?.Value));

      foreach (var rootFeature in roots)
      {
         var rootItem = new FeatureTreeItem(rootFeature);
         AddChildrenRecursive(rootItem, all);
         FeatureTree.Add(rootItem);
      }
   }

   public void SelectFeature(IAppFeature feature)
   {
      // Find the corresponding tree item and select it
      var item = FindTreeItem(FeatureTree, feature);
      if (item != null)
      {
         SelectedItem = item;
         RequestSelectionUpdate?.Invoke(item);
      }
   }

   private static FeatureTreeItem? FindTreeItem(ObservableCollection<FeatureTreeItem> featureTree, IAppFeature feature)
   {
      foreach (var item in featureTree)
      {
         if (item.Feature.Id.Value == feature.Id.Value)
            return item;

         var foundInChildren = FindTreeItem(item.Children, feature);
         if (foundInChildren != null)
         {
            item.IsExpanded = true;
            return foundInChildren;
         }
      }

      return null;
   }

   private static void AddChildrenRecursive(FeatureTreeItem parent, List<IAppFeature> all)
   {
      var children = all.Where(f => f.ParentFeatureId?.Value == parent.Feature.Id.Value);
      foreach (var childFeature in children)
      {
         var childItem = new FeatureTreeItem(childFeature);
         AddChildrenRecursive(childItem, all);
         parent.Children.Add(childItem);
      }
   }

   private void UpdateDetails()
   {
      if (SelectedFeature == null)
         return;

      // 1. Find Commands for this feature's scopes
      var scopes = SelectedFeature.AssociatedScopes.ToHashSet();
      AssociatedCommands = CommandRegistry.AllCommands
                                          .Where(c => scopes.Contains(c.Scope))
                                          .ToList();

      // 2. Update the Spatial Map
      UpdateLocationGrid(SelectedFeature.Location, SelectedFeature.Scale);
   }

   private void UpdateLocationGrid(FeatureLocation location, FeatureScale scale)
   {
      var primaries = new HashSet<int>();
      var secondaries = new HashSet<int>();

      // 1. Identify the Core Index
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

      // 2. Calculate "Spillover" based on Scale
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

      // 3. Special visual for "Floating/Contextual"
      // We can make these look distinct by always making them "Center" but adding 
      // a different secondary pattern.

      var cells = new List<LocationGridCell>();
      for (var i = 0; i < 9; i++)
         cells.Add(new(primaries.Contains(i), secondaries.Contains(i)));
      LocationGridCells = cells;
   }

   private void ApplyFilter()
   {
      // Disable selection updates temporarily if needed for performance
      foreach (var item in FeatureTree)
         FilterRecursive(item, SearchQuery);
   }

   private bool FilterRecursive(FeatureTreeItem item, string query)
   {
      // Case 1: Empty Query - Show Everything
      if (string.IsNullOrWhiteSpace(query))
      {
         item.IsVisible = true;
         foreach (var child in item.Children)
            FilterRecursive(child, query);
         return true;
      }

      // Case 2: Check for matches in this specific node
      var matches = item.Feature.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    item.Feature.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    item.Feature.SearchSynonyms.Any(s => s.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    item.Feature.Id.Value.Contains(query, StringComparison.OrdinalIgnoreCase);

      // Case 3: Check children recursively
      var anyChildMatches = false;
      foreach (var child in item.Children)
         if (FilterRecursive(child, query))
            anyChildMatches = true;

      // A node is visible if IT matches OR any of its CHILDREN match
      item.IsVisible = matches || anyChildMatches;

      // Auto-expand if a child is a match so the user sees the result
      if (anyChildMatches)
         item.IsExpanded = true;

      return item.IsVisible;
   }

   private void ExecuteSpotlight()
   {
   }
}