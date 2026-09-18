#region

using System.Collections.ObjectModel;
using System.Windows;
using Arcanum.UI.Components.Windows.MinorWindows.PopUpEditors;

#endregion

namespace Arcanum.UI.Documentation.Implementation;

public partial class FeatureDocEditorView
{
   public static readonly DependencyProperty SelectedFeatureProperty =
      DependencyProperty.Register(nameof(SelectedFeature),
                                  typeof(FeatureDoc),
                                  typeof(FeatureDocEditorView),
                                  new(null, OnSelectedFeatureChanged));

   public FeatureDocEditorView()
   {
      DataContext = new FeatureDocEditorViewModel(null);
      InitializeComponent();
   }

   public FeatureDoc SelectedFeature
   {
      get => (FeatureDoc)GetValue(SelectedFeatureProperty);
      set => SetValue(SelectedFeatureProperty, value);
   }

   private static void OnSelectedFeatureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      if (d is FeatureDocEditorView view)
         view.DataContext = new FeatureDocEditorViewModel(e.NewValue as FeatureDoc);
   }
}

public class FeatureDocEditorViewModel : ViewModelBase
{
   private readonly FeatureDoc? _doc;

   public FeatureDocEditorViewModel(FeatureDoc? doc)
   {
      _doc = doc;
      LinksBuffer = string.Join(", ", doc?.Links ?? []);
      KeywordsBuffer = string.Join(", ", doc?.SearchKeywords ?? []);
      ScopesBuffer = string.Join(", ", doc?.AssociatedScopes ?? []);
   }

   public FeatureDoc? Doc => _doc;

   public string LinksBuffer { get; set; }
   public string KeywordsBuffer { get; set; }
   public string ScopesBuffer { get; set; }

   // Autocomplete properties
   public ObservableCollection<string> CurrentSuggestions { get; } = new();
   public bool IsSuggesting { get; set; }
   public int SuggestionIndex { get; set; }

   public void UpdateSuggestions(string fullText, string[] sourcePool)
   {
      var lastPart = fullText.Split(',').LastOrDefault()?.Trim().ToLower() ?? "";
      CurrentSuggestions.Clear();

      if (lastPart.Length >= 1)
      {
         var matches = sourcePool
                      .Where(s => s.ToLower().Contains(lastPart) && !fullText.Contains(s))
                      .Take(10);

         foreach (var match in matches)
            CurrentSuggestions.Add(match);
      }

      IsSuggesting = CurrentSuggestions.Count > 0;
      if (IsSuggesting)
         SuggestionIndex = 0;
   }

   public string ApplySelection(string fullText, string selectedValue)
   {
      var parts = fullText.Split(',').Select(p => p.Trim()).ToList();
      if (parts.Count > 0)
         parts.RemoveAt(parts.Count - 1);
      parts.Add(selectedValue);
      return string.Join(", ", parts) + ", ";
   }

   public void Save()
   {
      _doc?.Links = LinksBuffer.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
      _doc?.SearchKeywords = KeywordsBuffer.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
      _doc?.AssociatedScopes = ScopesBuffer.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
   }
}