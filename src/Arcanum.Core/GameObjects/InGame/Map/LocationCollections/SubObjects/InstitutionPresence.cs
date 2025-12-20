using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.InGame.Cultural;

namespace Arcanum.Core.GameObjects.InGame.Map.LocationCollections.SubObjects;

[ObjectSaveAs(savingMethod: "InstitutionPresenceSaving")]
public partial class InstitutionPresence : IEmbeddedEu5Object<InstitutionPresence>
{
   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("-", iEu5KeyType: typeof(Institution))]
   [Description("The institution this presence applies to.")]
   public Institution Institution { get; set; } = Institution.Empty;

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("MISSING_CUSTOM_SAVING")]
   [Description("Whether the institution is present in this location.")]
   public bool IsPresent { get; set; }

   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.InstitutionPresenceSettings;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.InstitutionPresenceAgsSettings;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public static InstitutionPresence Empty => new() { UniqueId = "Arcanum_Empty_InstitutionPresence" };
}