using System.ComponentModel;
using System.Numerics;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.InGame.gfx.map;

[ObjectSaveAs]
[NexusConfig]
public partial class NudgeData : IEu5Object<NudgeData>
{
   #region Nexus Properties

   [Description("The location this nudge data is associated with.")]
   [DefaultValue(null)]
   [ParseAs("id")]
   [SaveAs]
   public Location TargetLocation { get; set; } = Location.Empty;

   [Description("The position of the nudge data.")]
   [DefaultValue("0,0,0")]
   [ParseAs("position", AstNodeType.BlockNode)]
   [SaveAs]
   public Vector3 Position { get; set; } = Vector3.Zero;

   [Description("The rotation of the nudge data.")]
   [DefaultValue("0,0,0,1")]
   [ParseAs("rotation", AstNodeType.BlockNode)]
   [SaveAs]
   public Quaternion Rotation { get; set; } = Quaternion.Identity;

   [Description("The scale of the nudge data.")]
   [DefaultValue("1,1,1")]
   [ParseAs("scale", AstNodeType.BlockNode)]
   [SaveAs]
   public Vector3 Scale { get; set; } = Vector3.One;

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this NudgeData. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Gfx.Map.{nameof(NudgeData)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.NudgeDataSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.NudgeData;
   public static Dictionary<string, NudgeData> GetGlobalItems() => [];
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static NudgeData Empty { get; } = new() { UniqueId = "Arcanum_Empty_NudgeData" };

   #endregion
}