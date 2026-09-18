#region

using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Serialization;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

#endregion

namespace Arcanum.Core.GameObjects.InGame.Cultural;

[ObjectSaveAs]
public partial class NameCollection : IEmbeddedEu5Object<NameCollection>
{
   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(null)]
   [Description("All names in this given subset of names")]
   [ParseAs("-", AstNodeType.KeyOnlyNode, isShatteredList: true)]
   [AgsCollectionFormat(ItemsPerRow = 10)]
   public ObservableRangeCollection<string> NameEntries { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [Description("The weight for these names to be applied")]
   [ParseAs("weight", AstNodeType.BlockNode, isEmbedded: true)]
   public NamingWeight Weight { get; set; } = NamingWeight.Empty;

   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.NameCollectionSettings;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.NameCollection;
   [PropertyConfig(true)]
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public static NameCollection Empty { get; } = new() { UniqueId = "NameCollection_EMPTY" };
}