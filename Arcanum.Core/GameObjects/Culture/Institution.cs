using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.AbstractMechanics;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;
using Common.UI;

namespace Arcanum.Core.GameObjects.Culture;

[ObjectSaveAs]
public partial class Institution : IEu5Object<Institution>
{
   #region Nexus Properties

   [SaveAs]
   [ParseAs("age")]
   [Description("The age in which this institution first appears.")]
   [DefaultValue(null)]
   public Age Age { get; set; } = Age.Empty;

   #endregion

#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key of this Institution. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Court.{nameof(Institution)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.InstitutionSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.InstitutionAgsSettings;
   public static Dictionary<string, Institution> GetGlobalItems() => Globals.Institutions;

   public static Institution Empty { get; } = new() { UniqueId = "Arcanum_Empty_Institution" };

   public override string ToString() => UniqueId;

   #endregion
}