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
using Nexus.Core;

namespace Arcanum.Core.GameObjects.Map;

[ObjectSaveAs]
public partial class SoundToll : IEu5Object<SoundToll>
{
   #region Nexus Properties

   [SaveAs]
   [ParseAs(Globals.DO_NOT_PARSE_ME)]
   [DefaultValue(null)]
   [Description("The first location that defines the strait for this SoundToll.")]
   public Location StraitLocationOne { get; set; } = Location.Empty;

   [SaveAs]
   [ParseAs(Globals.DO_NOT_PARSE_ME)]
   [DefaultValue(null)]
   [Description("The second location that defines the strait for this SoundToll.")]
   public Location StraitLocationTwo { get; set; } = Location.Empty;

   #endregion

#pragma warning disable AGS004
   [ReadonlyNexus]
   [IgnoreModifiable]
   [Description("Unique key of this SoundToll. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId
   {
      get => $"{StraitLocationOne.UniqueId}_{StraitLocationTwo.UniqueId}_SoundToll";
      set { }
   }

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Map.{nameof(SoundToll)}";
   public void OnSearchSelected() => UIHandle.Instance.MainWindowsHandle.SetToNui(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.SoundTollSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.SoundTollAgsSettings;

   public static Dictionary<string, SoundToll> GetGlobalItems()
      => Globals.DefaultMapDefinition.SoundTolls.ToDictionary(x => x.UniqueId, x => x);

   public static SoundToll Empty { get; } = new() { UniqueId = "Arcanum_Empty_SoundToll" };

   public override string ToString() => UniqueId;

   #endregion
}