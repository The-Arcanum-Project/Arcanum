using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Common.UI;

namespace Arcanum.Core.GameObjects.Pops;

[ObjectSaveAs]
public partial class EstateAttributeDefinition : IEu5Object<EstateAttributeDefinition>
{
   #region Nexus Properties

   [SaveAs]
   [ParseAs("is_cossacks")]
   [DefaultValue(false)]
   [Description(Globals.REPLACE_DESCRIPTION)]
   public bool IsCossacks { get; set; }

   [SaveAs]
   [ParseAs("is_dhimmi")]
   [DefaultValue(false)]
   [Description(Globals.REPLACE_DESCRIPTION)]
   public bool IsDhimmi { get; set; }

   [SaveAs]
   [ParseAs("is_gaelic_clans")]
   [DefaultValue(false)]
   [Description(Globals.REPLACE_DESCRIPTION)]
   public bool IsGaelicClans { get; set; }

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this EstateAttributeDefinition. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Pops.{nameof(EstateAttributeDefinition)}";
   public void OnSearchSelected() => UIHandle.Instance.MainWindowsHandle.SetToNui(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.EstateAttributeDefinitionSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.EstateAttributeDefinitionAgsSettings;
   public static Dictionary<string, EstateAttributeDefinition> GetGlobalItems() => [];

   public static EstateAttributeDefinition Empty { get; } =
      new() { UniqueId = "Arcanum_Empty_EstateAttributeDefinition" };

   public override string ToString() => UniqueId;

   #endregion
}