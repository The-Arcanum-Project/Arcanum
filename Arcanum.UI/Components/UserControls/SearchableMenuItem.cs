using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Queastor;
using Common;

namespace Arcanum.UI.Components.UserControls;

public class SearchableMenuItem : MenuItem, ISearchable
{
   public string GetNamespace { get; private set; } = string.Empty;
   public string ResultName { get; private set; }

   public List<string> SearchTerms { get; set; } = [];
   // is modified via Xaml
   // ReSharper disable once CollectionNeverUpdated.Global
   internal IList<string> XamlSearchTerms { get; set; } = [];

   private const string FALL_BACK = "MenuBar";

   protected override void OnInitialized(EventArgs e)
   {
      base.OnInitialized(e);

      SearchTerms = new(XamlSearchTerms);

      if (Header?.ToString() is { } header)
         SearchTerms.AddRange(QueastorUtils.ExtractSearchTerms(header));

      ResultName = Header?.ToString() ?? string.Empty;
      GetNamespace = GetParentAsNamespace();
      Queastor.GlobalInstance.AddToIndex(this);
   }

   private string GetNamespaceCombined()
   {
      var header = Header?.ToString() ?? string.Empty;
      if (!string.IsNullOrEmpty(GetNamespace))
         return GetNamespace + ">" + header;

      var parent = GetParentMenuItem(this);
      GetNamespace = parent == null ? FALL_BACK : parent.GetNamespaceCombined();

      return GetNamespace + ">" + header;
   }

   public void OnSearchSelected()
   {
      // Traverse up to find the root MenuItem and open it
      var parent = GetParentMenuItem(this);
      while (parent != null)
      {
         if (!parent.IsSubmenuOpen)
         {
            parent.IsSubmenuOpen = true;
         }

         parent = GetParentMenuItem(parent);
      }

      IsSubmenuOpen = true;

      StaysOpenOnClick = true;
      Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input,
                                                 new Action(() =>
                                                 {
                                                    Focus();
                                                    Keyboard.Focus(this);
                                                 }));
   }

   public ISearchResult VisualRepresentation
      => new SearchResultItem(null, Header?.ToString() ?? string.Empty, GetNamespaceCombined());
   
   public IQueastorSearchSettings.Category SearchCategory { get; } = IQueastorSearchSettings.Category.UiElements;

   private string GetParentAsNamespace()
   {
      if (!string.IsNullOrEmpty(GetNamespace))
         return GetNamespace;

      var parent = GetParentMenuItem(this);
      return parent == null ? FALL_BACK : parent.GetNamespaceCombined();
   }

   private static SearchableMenuItem? GetParentMenuItem(SearchableMenuItem item)
   {
      return item.Parent as SearchableMenuItem;
   }
}