#region

using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

#endregion

namespace Arcanum.Core.GameObjects.InGame.Cultural;

[ObjectSaveAs]
public partial class NamingWeight : IEmbeddedEu5Object<NamingWeight>
{
   [SaveAs]
   [DefaultValue(0)]
   [ParseAs("value")]
   [Description("The weight value for the specified name collection.")]
   public int Value { get; set; }

   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.NamingWeightSettings;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.NamingWeight;
   [PropertyConfig(true)]
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public static NamingWeight Empty { get; } = new() { UniqueId = "NamingWeight_EMPTY" };
}