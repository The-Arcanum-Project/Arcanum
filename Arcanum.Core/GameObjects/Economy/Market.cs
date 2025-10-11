using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;
using Common.UI;

namespace Arcanum.Core.GameObjects.Economy;

/// <summary>
/// Placeholder for the market type. Not sure how to do it yet.
/// </summary>
[ObjectSaveAs]
public partial class Market : IEu5Object<Market>
{
   #region Nexus Properties

   [SaveAs]
   [ParseAs("add_market")]
   [DefaultValue(null)]
   [Description("The location where this market is situated.")]

   public Location Location { get; set; } = Location.Empty;

   #endregion

#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key of this Market. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId
   {
      get => Location.UniqueId + "_Market";
      set { }
   }

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Economy.{nameof(Market)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.MarketSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.MarketAgsSettings;
   public static Dictionary<string, Market> GetGlobalItems() => Globals.Markets;

   public static Market Empty { get; } = new() { UniqueId = "Arcanum_Empty_Market" };

   public override string ToString() => UniqueId;

   #endregion
}