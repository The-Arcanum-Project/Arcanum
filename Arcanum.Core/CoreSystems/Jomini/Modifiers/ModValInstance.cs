using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.Common;
using Common.UI;

namespace Arcanum.Core.CoreSystems.Jomini.Modifiers;

/// <summary>
/// An instance of a modifier definition with an associated value.
/// </summary>
[ObjectSaveAs]
public partial class ModValInstance : IEu5Object<ModValInstance>
{
   /// <summary>
   /// An instance of a modifier definition with an associated value.
   /// </summary>
   /// <param name="definition"></param>
   /// <param name="value"></param>
   /// <param name="type"></param>
   [Obsolete("Use the ModifierManager to create instances.")]
   public ModValInstance(ModifierDefinition definition, object value, ModifierType type)
   {
      Definition = definition;
      Value = value;
      Type = type;
   }

   public ModValInstance()
   {
   }

   /// <summary>
   /// The definition of the modifier.
   /// </summary>
   [Description("The definition of the modifier.")]
   [DefaultValue(null)]
   public ModifierDefinition Definition { get; set; } = null!;

   public string UniqueId
   {
      get => Definition.UniqueId;
      set => Definition.UniqueId = value;
   }
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   /// <summary>
   /// The value of the modifier.
   /// </summary>
   [Description("The value of the modifier.")]
   [DefaultValue(0)]
   public object Value { get; set; } = null!;
   /// <summary>
   /// The type of the modifier, inferred from the definition.
   /// </summary>
   [Description("The type of the modifier, inferred from the definition.")]
   [DefaultValue(ModifierType.Float)]
   public ModifierType Type { get; set; }

   public static Dictionary<string, ModValInstance> GetGlobalItems() => [];

   public override string ToString()
   {
      return $"{Definition.UniqueId} : {Value} ({Type})";
   }

   public bool IsReadonly => false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.ModValInstanceSettings;
   public INUINavigation[] Navigations { get; } = [];
#pragma warning disable CS0618 // Type or member is obsolete
   public static ModValInstance Empty { get; } = new(ModifierDefinition.Empty, 0, ModifierType.Integer);
#pragma warning restore CS0618 // Type or member is obsolete
   public string GetNamespace => $"Jomini.{nameof(ModValInstance)}";

   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, Definition.UniqueId, string.Empty);
   public IQueastorSearchSettings.Category SearchCategory => IQueastorSearchSettings.Category.GameObjects;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ModValInstanceAgsSettings;
}