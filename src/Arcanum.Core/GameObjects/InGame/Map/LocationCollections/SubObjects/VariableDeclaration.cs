using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Common.UI;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.InGame.Map.LocationCollections.SubObjects;

[ObjectSaveAs]
[NexusConfig]
public partial class VariableDeclaration : IEu5Object<VariableDeclaration>
{
   [SaveAs(SavingValueType.String)]
   [ParseAs("flag")]
   [DefaultValue("")]
   [Description("The flag associated with this variable declaration.")]
   public string Flag { get; set; } = string.Empty;

   [SaveAs]
   [ParseAs("data", isEmbedded: true, nodeType: AstNodeType.BlockNode)]
   [Description("The data block associated with this variable declaration.")]
   [DefaultValue(null)]
   public VariableDataBlock DataBlock { get; set; } = VariableDataBlock.Empty;

#pragma warning disable AGS004
   [Description("Unique key of this VariableDeclaration. Must be unique among all objects of this type.")]
   [DefaultValue("")]
   public string UniqueId { get; set; } = string.Empty;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Data.Variables.{nameof(VariableDeclaration)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.VariableDeclarationSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.VariableDeclarationAgsSettings;
   public static Dictionary<string, VariableDeclaration> GetGlobalItems() => [];
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static VariableDeclaration Empty { get; } = new() { UniqueId = "Arcanum_Empty_VariableDeclaration" };

   #endregion
}