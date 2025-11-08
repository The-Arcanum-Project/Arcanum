using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.AbstractMechanics;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Common.UI;

namespace Arcanum.Core.GameObjects.Cultural;

[ObjectSaveAs]
public partial class ArtistType : IEu5Object<ArtistType>
{
    #region Nexus Properties
    #endregion

#pragma warning disable AGS004
    [Description("Unique key of this Artist Type. Must be unique among all objects of this type.")]
    [DefaultValue("null")]
    public string UniqueId { get; set; } = null!;

    [SuppressAgs]
    public Eu5FileObj Source { get; set; } = null!;
    public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

    #region IEu5Object

    public string GetNamespace => $"Court.{nameof(ArtistType)}";
    public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
    public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
    public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
    public bool IsReadonly => true;
    public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ArtistTypeSettings;
    public INUINavigation[] Navigations => [];
    public AgsSettings AgsSettings => Config.Settings.AgsSettings.ArtistTypeAgsSettings;
    public InjRepType InjRepType { get; set; } = InjRepType.None;
    public static Dictionary<string, ArtistType> GetGlobalItems() => Globals.ArtistTypes;

    public static ArtistType Empty { get; } = new() { UniqueId = "Arcanum_Empty_ArtistType" };

    public override string ToString() => UniqueId;

    #endregion

}
