#region

using System.Collections.ObjectModel;
using System.Windows.Input;
using Arcanum.UI.AppFeatures;
using Arcanum.UI.Commands;
using Arcanum.UI.Documentation.Implementation;
using CommunityToolkit.Mvvm.Input;
using RelayCommand = Arcanum.UI.Components.Windows.DebugWindows.RelayCommand;

#endregion

namespace Arcanum.UI.Components.Windows.HelpWindow.ViewModels;

public class FeatureExplorerViewModel : HelpPageViewModelBase
{
   private readonly Dictionary<string, Type> _filterTypes = new()
   {
      { "Status", typeof(FeatureStatus) },
      { "Category", typeof(FeatureCategory) },
      { "Level", typeof(FeatureLevel) },
      { "Scale", typeof(FeatureScale) },
      { "Location", typeof(FeatureLocation) },
   };

   public FeatureExplorerViewModel()
   {
      SetFeatures();
      SpotlightCommand = new RelayCommand(_ => ExecuteSpotlight());
      CompleteSearchCommand = new RelayCommand<string>(CompleteSearch);
      UpdateLocationGrid(FeatureLocation.Center, FeatureScale.Standard); // Default
   }

   public override string Title => "Feature Explorer";

   public ObservableCollection<FeatureItem> Features { get; } = [];

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

   public int SuggestionIndex
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   } = -1;

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
   public bool IsSuggesting
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   }

   public ObservableCollection<string> SearchSuggestions { get; } = [];

   public ICommand SpotlightCommand { get; }
   public ICommand CompleteSearchCommand { get; }
   public event Action<FeatureItem?>? RequestSelectionUpdate;

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
      UpdateSuggestions(query);

      if (string.IsNullOrEmpty(query))
      {
         foreach (var f in Features)
            f.IsVisible = true;
         return;
      }

      var parts = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
      var tags = parts.Where(p => p.StartsWith("@")).ToArray();
      var textTerms = parts.Where(p => !p.StartsWith("@")).ToArray();

      foreach (var item in Features)
      {
         var matchesTags = true;

         foreach (var tag in tags)
         {
            // Expected format: @Type:Value
            var kvp = tag[1..].Split(':');
            if (kvp.Length < 2)
               continue; // Ignore incomplete tags for filtering

            var type = kvp[0];
            var val = kvp[1];

            var tagMatch = type.ToLower() switch
            {
               "status" => item.Documentation.Status.ToString().Equals(val, StringComparison.OrdinalIgnoreCase),
               "category" => item.Documentation.Category.ToString().Equals(val, StringComparison.OrdinalIgnoreCase),
               "level" => item.Documentation.Level.ToString().Equals(val, StringComparison.OrdinalIgnoreCase),
               "scale" => item.Documentation.Scale.ToString().Equals(val, StringComparison.OrdinalIgnoreCase),
               "location" => item.Documentation.Location.ToString().Equals(val, StringComparison.OrdinalIgnoreCase),
               _ => false,
            };

            if (!tagMatch)
            {
               matchesTags = false;
               break;
            }
         }

         var matchesText = textTerms.Length == 0 ||
                           textTerms.Any(t =>
                                            item.Documentation.Title.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                                            item.Documentation.Summary.Contains(t, StringComparison.OrdinalIgnoreCase));

         item.IsVisible = matchesTags && matchesText;
      }
   }

   private void UpdateSuggestions(string query)
   {
      var currentToken = query.Split(' ').LastOrDefault(s => s.StartsWith("@"));
      if (currentToken == null)
      {
         IsSuggesting = false;
         return;
      }

      SearchSuggestions.Clear();

      if (!currentToken.Contains(":"))
      {
         // Suggesting the Type
         var inputType = currentToken[1..].ToLower();
         foreach (var type in _filterTypes.Keys)
            if (type.ToLower().Contains(inputType))
               SearchSuggestions.Add($"@{type}:");
      }
      else
      {
         // Suggesting the Value
         var parts = currentToken[1..].Split(':');
         var typeKey = _filterTypes.Keys.FirstOrDefault(k => k.Equals(parts[0], StringComparison.OrdinalIgnoreCase));

         if (typeKey != null)
         {
            var inputValue = parts[1].ToLower();
            var enumValues = Enum.GetNames(_filterTypes[typeKey]);
            foreach (var val in enumValues)
               if (val.ToLower().Contains(inputValue))
                  SearchSuggestions.Add($"@{typeKey}:{val}");
         }
      }

      if (SearchSuggestions.Count > 0)
      {
         if (SuggestionIndex == -1)
            SuggestionIndex = 0;
      }
      else
         SuggestionIndex = -1;

      IsSuggesting = SearchSuggestions.Count > 0;
   }

   public void CompleteSearch(string suggestion)
   {
      if (string.IsNullOrEmpty(suggestion))
         return;

      var parts = SearchQuery.Split(' ').ToList();
      if (parts.Count > 0)
         parts.RemoveAt(parts.Count - 1); // Remove the partial token (e.g., "@Stat")

      parts.Add(suggestion);

      // Add a space if we finished a full tag, keep it tight if we just added the colon
      var separator = suggestion.EndsWith(":") ? "" : " ";
      SearchQuery = string.Join(" ", parts) + separator;

      IsSuggesting = suggestion.EndsWith(":");
      SuggestionIndex = -1;
   }

   private void ExecuteSpotlight()
   {
   }
}