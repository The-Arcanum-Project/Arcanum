using System.ComponentModel;
using System.Globalization;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using ModifierDefinition = Arcanum.Core.GameObjects.InGame.Common.ModifierDefinition;

namespace Arcanum.Core.CoreSystems.Jomini.Modifiers;

/// <summary>
/// An instance of a modifier definition with an associated value.
/// </summary>
[ObjectSaveAs(savingMethod: "ModValInstanceSaving")]
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
   [SuppressAgs]
   [DefaultValue(null)]
   public ModifierDefinition Definition { get; set; } = null!;

   public string UniqueId
   {
      get => Definition.UniqueId;
      set => Definition.UniqueId = value;
   }
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   /// <summary>
   /// The value of the modifier.
   /// </summary>
   [Description("The value of the modifier.")]
   [DefaultValue(0)]
   [SuppressAgs]
   public object Value { get; set; } = null!;
   /// <summary>
   /// The type of the modifier, inferred from the definition.
   /// </summary>
   [Description("The type of the modifier, inferred from the definition.")]
   [DefaultValue(ModifierType.Float)]
   [SuppressAgs]
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

   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, Definition.UniqueId, string.Empty);
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ModValInstanceAgsSettings;

   public string FormatModifierPatternToCode()
   {
      switch (Type)
      {
         case ModifierType.Boolean:
            return (bool)Value ? "yes" : "no";
         case ModifierType.ScriptedValue:
            return Value.ToString() ??
                   throw new InvalidOperationException("Identifier modifier value cannot be null");
         case ModifierType.Percentage:
         case ModifierType.Float:
            if (Value is string strValue)
               return strValue;

            return
               $"{Convert.ToDouble(Value, CultureInfo.InvariantCulture).ToString($"0.{new string('#', Definition.NumDecimals)}", CultureInfo.InvariantCulture)}";
         case ModifierType.Integer:
            if (Value is string strValue2)
               return strValue2;

            return Convert.ToInt32(Value).ToString();
         default:
            throw new
               ArgumentOutOfRangeException($"Unknown modifier type {Type} for modifier {UniqueId}");
      }
   }
}