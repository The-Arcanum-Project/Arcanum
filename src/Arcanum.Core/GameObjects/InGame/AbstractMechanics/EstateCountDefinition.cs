using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.InGame.Cultural;

namespace Arcanum.Core.GameObjects.InGame.AbstractMechanics;

[ObjectSaveAs(savingMethod: "EstateCountDefinitionSaving")]
public partial class EstateCountDefinition : IEmbeddedEu5Object<EstateCountDefinition>
{
   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("-", iEu5KeyType: typeof(Estate))]
   [Description("The estate this definition applies to.")]
   public Estate Estate { get; set; } = Estate.Empty;

   [SaveAs]
   [DefaultValue(0)]
   [ParseAs("count")]
   [Description("The number of estates of this type.")]
   public int Count { get; set; }

   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.EstateCountDefinitonSettings;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.EstateCountDefiniton;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public static EstateCountDefinition Empty { get; } = new() { UniqueId = "Arcanum_Empty_EstateDefinition" };
}