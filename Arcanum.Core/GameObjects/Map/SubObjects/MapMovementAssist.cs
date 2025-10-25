using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.GameObjects.Map.SubObjects;

[ObjectSaveAs(savingMethod: "MapMovementAssistSaving")]
public partial class MapMovementAssist : IEmbeddedEu5Object<MapMovementAssist>
{
   [SuppressAgs]
   [Description("The factor on the X axis for map movement assistance.")]
   [DefaultValue(0f)]
   public float X { get; set; }

   [SuppressAgs]
   [Description("The factor on the Y axis for map movement assistance.")]
   [DefaultValue(0f)]
   public float Y { get; set; }

   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.MapMovementAssistSettings;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.MapMovementAssistAgsSettings;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public static MapMovementAssist Empty { get; } = new();
}